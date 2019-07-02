using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using log4net;

namespace System.Threading
{
	/// <summary>
	///     Similar to <see cref="System.Threading.Tasks.TaskScheduler" />, it is capable of scheduling tasks.
	///     Can also schedule periodic tasks that are executed with a minimum time between them (until removed).
	///     <see cref="StartPeriodic(System.Action,System.TimeSpan,string)" /> and <see cref="StopPeriodic" />.
	/// </summary>
	public sealed class DefaultTaskScheduler
		: ITaskScheduler
			, IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly TaskScheduler _scheduler;
		private readonly List<PeriodicTask> _tasks;
		private long _lastTaskId;

		/// <summary>
		///     Initializes this scheduler.
		/// </summary>
		public DefaultTaskScheduler()
			: this(TaskScheduler.Default)
		{
		}

		private DefaultTaskScheduler(TaskScheduler scheduler)
		{
			if (scheduler == null)
				throw new ArgumentNullException(nameof(scheduler));

			_scheduler = scheduler;
			_tasks = new List<PeriodicTask>();
		}

		/// <inheritdoc />
		public void Dispose()
		{
			lock (_tasks)
			{
				foreach (var task in _tasks)
					task.IsStopped = true;
			}
		}

		/// <inheritdoc />
		public int PeriodicTaskCount
		{
			get
			{
				lock (_tasks)
				{
					return _tasks.Count;
				}
			}
		}

		/// <inheritdoc />
		public Task Start(Action callback)
		{
			var task = new Task(callback);
			task.Start(_scheduler);
			return task;
		}

		/// <inheritdoc />
		public Task<T> Start<T>(Func<T> callback)
		{
			var task = new Task<T>(callback);
			task.Start(_scheduler);
			return task;
		}

		/// <inheritdoc />
		public IPeriodicTask StartPeriodic(Action callback, TimeSpan minimumWaitTime, string name = null)
		{
			var id = Interlocked.Increment(ref _lastTaskId);
			var task = new PeriodicTask(id, callback, minimumWaitTime, name);

			if (Log.IsDebugEnabled)
				Log.DebugFormat("Starting periodic task {0} at {1}ms intervals", task, minimumWaitTime.TotalMilliseconds);

			lock (_tasks)
			{
				_tasks.Add(task);
			}

			RunOnce(task);

			return task;
		}

		/// <inheritdoc />
		public IPeriodicTask StartPeriodic(Func<TimeSpan> callback, string name = null)
		{
			var id = Interlocked.Increment(ref _lastTaskId);
			var task = new PeriodicTask(id, callback, name);

			if (Log.IsDebugEnabled)
				Log.DebugFormat("Starting periodic task {0} at irregular intervals", task);

			lock (_tasks)
			{
				_tasks.Add(task);
			}

			RunOnce(task);

			return task;
		}

		/// <inheritdoc />
		public bool StopPeriodic(IPeriodicTask task)
		{
			var periodicTask = task as PeriodicTask;
			if (periodicTask == null)
				return false;

			if (TryRemoveTask(periodicTask))
			{
				if (Log.IsDebugEnabled)
					Log.DebugFormat("Stopped periodic task {0}", task);

				return true;
			}

			return false;
		}

		private bool TryRemoveTask(PeriodicTask periodicTask)
		{
			lock (_tasks)
			{
				if (_tasks.Remove(periodicTask))
				{
					periodicTask.IsStopped = true;
					return true;
				}

				return false;
			}
		}

		private void RunOnce(PeriodicTask periodicTask)
		{
			var remainingWaitTime = periodicTask.RemainingTimeUntilNextInvocation;
			var waitTask = Task.Delay(remainingWaitTime);
			var actualTask = waitTask.ContinueWith(unused =>
			{
				periodicTask.Run();
				return periodicTask;
			}, _scheduler);
			actualTask.ContinueWith(OnPeriodicTaskFinished);
		}

		private void OnPeriodicTaskFinished(Task<PeriodicTask> task)
		{
			var periodicTask = task.Result;

			if (periodicTask.IsStopped)
			{
				Log.DebugFormat("Periodic task '{0}' has been stopped and will no longer be scheduled", periodicTask);
			}
			else
			{
				if (Log.IsDebugEnabled)
					Log.DebugFormat("Periodic task '{0}' has finished executing and is added to the task queue once more", periodicTask);

				RunOnce(periodicTask);
			}
		}
	}
}