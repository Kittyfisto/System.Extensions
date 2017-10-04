using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using log4net;

namespace System.Threading
{
	/// <summary>
	///     Schedules all tasks it's been given in a serial fashion:
	///     - At no time will two tasks added to the same scheduler be executed concurrently
	///     - Tasks will be executed in the order they were added
	///     - A non-terminating task will cause all subsequent tasks to never be executed, ever
	///     - All pending tasks will be cancelled when the scheduler is disposed of
	/// </summary>
	public sealed class SerialTaskScheduler
		: ISerialTaskScheduler
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly CancellationTokenSource _disposed;
		private readonly Queue<ITaskInfo> _pendingTasks;
		private readonly SemaphoreSlim _semaphore;

		private readonly object _syncRoot;
		private readonly Thread _thread;

		private long _lastTaskId;

		/// <summary>
		///     Initializes this scheduler.
		/// </summary>
		/// <param name="debugName">
		///     Name used to differentiate between schedulers (used for logging as well as for naming BG
		///     threads)
		/// </param>
		public SerialTaskScheduler(string debugName = null)
		{
			_semaphore = new SemaphoreSlim(initialCount: 0, maxCount: int.MaxValue);
			_disposed = new CancellationTokenSource();
			_pendingTasks = new Queue<ITaskInfo>();
			_syncRoot = new object();
			DebugName = debugName;
			_thread = new Thread(RunTasks)
			{
				IsBackground = true,
				Name = debugName ?? "SerialTaskScheduler"
			};
			_thread.Start();
		}

		/// <summary>
		///     The name of this scheduler, if any.
		/// </summary>
		public string DebugName { get; }

		/// <inheritdoc />
		public void Dispose()
		{
			try
			{
				lock (_syncRoot)
				{
					// Setting this flag must be synchronized with TryEnqueue, hence the lock
					_disposed.Cancel();
				}

				CancelPendingTasks();
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		/// <summary>
		///     Starts a new task which will execute the given action of a background thread.
		///     The action will execute as soon as all previously started tasks have finished or faulted.
		/// </summary>
		/// <remarks>
		///     Returns an already cancelled task when <see cref="Dispose" /> has been called of.
		/// </remarks>
		/// <param name="fn"></param>
		/// <returns></returns>
		public Task StartNew(Action fn)
		{
			var id = Interlocked.Increment(ref _lastTaskId);
			var info = new TaskInfo(id, fn);

			TryEnqueue(info);

			return info.Task;
		}

		/// <summary>
		///     Starts a new task which will execute the given action of a background thread.
		///     The action will execute as soon as all previously started tasks have finished or faulted.
		/// </summary>
		/// <remarks>
		///     Returns an already cancelled task when <see cref="Dispose" /> has been called of.
		/// </remarks>
		/// <param name="fn"></param>
		/// <returns></returns>
		public Task<T> StartNew<T>(Func<T> fn)
		{
			var id = Interlocked.Increment(ref _lastTaskId);
			var info = new TaskInfo<T>(id, fn);

			TryEnqueue(info);

			return info.Task;
		}

		private void CancelPendingTasks()
		{
			ITaskInfo task;
			while (TryDequeue(out task))
				CancelTask(task);
		}

		private bool TryDequeue(out ITaskInfo task)
		{
			lock (_syncRoot)
			{
				if (_pendingTasks.Count > 0)
				{
					task = _pendingTasks.Dequeue();
					return true;
				}

				task = null;
				return false;
			}
		}

		private void CancelTask(ITaskInfo task)
		{
			try
			{
				task.Cancel();
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		/// <summary>
		///     Tries to enqueue the given task so it will be executed later on.
		///     Might not enqueue the task if this scheduler has been disposed of.
		/// </summary>
		/// <param name="info"></param>
		private void TryEnqueue(ITaskInfo info)
		{
			lock (_syncRoot)
			{
				// This must be synchronized with Dispose, hence the lock.
				// If we're already disposed of, then we immediately cancel the task
				// without enqueueing anything...

				if (_disposed.IsCancellationRequested)
				{
					info.Cancel();
					return;
				}

				_pendingTasks.Enqueue(info);
			}

			_semaphore.Release();
		}

		private void RunTasks()
		{
			try
			{
				var disposedToken = _disposed.Token;
				while (!disposedToken.IsCancellationRequested)
				{
					_semaphore.Wait(disposedToken);
					if (disposedToken.IsCancellationRequested)
						break;

					ITaskInfo info;
					if (TryDequeue(out info))
						Execute(info);
					else
						Log.WarnFormat("Expected to be able to consume one pending task ons cheduler {0} but none were available",
							DebugName);
				}
			}
			catch (OperationCanceledException)
			{
				Log.DebugFormat("Scheduler {0} thread will be stopped because the scheduler is being disposed of...", DebugName);
			}
			catch (Exception e)
			{
				Log.FatalFormat("Caught unexpected exception on scheduler {0}: {1}", DebugName, e);
			}
			finally
			{
				Log.DebugFormat("Stopping scheduler {0} thread...", DebugName);
			}
		}

		private void Execute(ITaskInfo info)
		{
			long? id = null;
			try
			{
				id = info.Id;

				Log.DebugFormat("Starting task #{0}", id);

				info.Execute();

				Log.DebugFormat("Finished task #{0}", id);
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception while executing task #{0}: {1}", id, e);
			}
		}

		private interface ITaskInfo
		{
			long Id { get; }

			void Execute();
			void Cancel();
		}

		private sealed class TaskInfo
			: ITaskInfo
		{
			private readonly Action _fn;
			private readonly long _id;
			private readonly TaskCompletionSource<int> _source;

			public TaskInfo(long id, Action fn)
			{
				_id = id;
				_fn = fn;
				_source = new TaskCompletionSource<int>();
			}

			public Task Task => _source.Task;

			public long Id => _id;

			public void Execute()
			{
				try
				{
					_fn();
					// Somebody might have cancelled the task already
					_source.TrySetResult(result: 42);
				}
				catch (Exception e)
				{
					// Somebody might have cancelled the task already
					_source.TrySetException(e);
				}
			}

			public void Cancel()
			{
				// We don't synchronize access to the source (Cancel() is called
				// when the scheduler is disposed of and Execute() is called from the BG
				// thread) and therefore we can't be sure if the BG thread isn't executing that
				// task or even trying to set the result. We don't care either way so we can
				// leave it up to the source to synchronise this...
				_source.TrySetCanceled();
			}
		}

		private sealed class TaskInfo<T>
			: ITaskInfo
		{
			private readonly Func<T> _fn;
			private readonly long _id;
			private readonly TaskCompletionSource<T> _source;

			public TaskInfo(long id, Func<T> fn)
			{
				_id = id;
				_fn = fn;
				_source = new TaskCompletionSource<T>();
			}

			public Task<T> Task => _source.Task;

			public long Id => _id;

			public void Execute()
			{
				try
				{
					var result = _fn();
					// Somebody might have cancelled the task already
					_source.TrySetResult(result);
				}
				catch (Exception e)
				{
					// Somebody might have cancelled the task already
					_source.TrySetException(e);
				}
			}

			public void Cancel()
			{
				// We don't synchronize access to the source (Cancel() is called
				// when the scheduler is disposed of and Execute() is called from the BG
				// thread) and therefore we can't be sure if the BG thread isn't executing that
				// task or even trying to set the result. We don't care either way so we can
				// leave it up to the source to synchronise this...
				_source.TrySetCanceled();
			}
		}
	}
}