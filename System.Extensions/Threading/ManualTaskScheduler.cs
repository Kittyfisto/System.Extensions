using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.Threading
{
	/// <summary>
	///     <see cref="ITaskScheduler" /> implementation that allows the user to control when and if tasks are executed.
	/// </summary>
	public sealed class ManualTaskScheduler
		: ITaskScheduler
	{
		private readonly object _syncRoot;
		private readonly List<PeriodicTask> _tasks;
		private long _lastId;

		/// <summary>
		///     Initializes this scheduler.
		/// </summary>
		public ManualTaskScheduler()
		{
			_syncRoot = new object();
			_tasks = new List<PeriodicTask>();
		}

		/// <summary>
		///     All started periodic tasks (which haven't been stopped yet).
		/// </summary>
		public IEnumerable<IPeriodicTask> PeriodicTasks
		{
			get
			{
				lock (_syncRoot)
				{
					return _tasks.ToList();
				}
			}
		}

		/// <inheritdoc />
		public int PeriodicTaskCount
		{
			get
			{
				lock (_syncRoot)
				{
					return _tasks.Count;
				}
			}
		}

		/// <inheritdoc />
		public Task Start(Action callback)
		{
			return Task.Factory.StartNew(callback);
		}

		/// <inheritdoc />
		public Task<T> Start<T>(Func<T> callback)
		{
			return Task.Factory.StartNew(callback);
		}

		/// <inheritdoc />
		public IPeriodicTask StartPeriodic(Action callback, TimeSpan minimumWaitTime, string name = null)
		{
			var task = new PeriodicTask(Interlocked.Increment(ref _lastId), callback, minimumWaitTime, name);
			lock (_syncRoot)
			{
				_tasks.Add(task);
			}
			return task;
		}

		/// <inheritdoc />
		public IPeriodicTask StartPeriodic(Func<TimeSpan> callback, string name = null)
		{
			var task = new PeriodicTask(Interlocked.Increment(ref _lastId), callback, name);
			lock (_syncRoot)
			{
				_tasks.Add(task);
			}
			return task;
		}

		/// <inheritdoc />
		public bool StopPeriodic(IPeriodicTask task)
		{
			var actualTask = task as PeriodicTask;
			if (actualTask == null)
				return false;

			lock (_syncRoot)
			{
				return _tasks.Remove(actualTask);
			}
		}

		/// <summary>
		///     Runs every task exactly once.
		/// </summary>
		public void RunOnce()
		{
			IEnumerable<PeriodicTask> tasks;
			lock (_syncRoot)
			{
				tasks = _tasks.ToList();
			}

			foreach (var task in tasks)
				task.Run();
		}

		/// <summary>
		///     Executes all pending tasks for the given amount of times.
		///     This method may be desired when it is known that the first run will
		///     spawn subsequent tasks which shall also run.
		/// </summary>
		/// <param name="count"></param>
		public void Run(int count)
		{
			for (var i = 0; i < count; ++i)
				RunOnce();
		}
	}
}