namespace System.Threading
{
	/// <summary>
	///     The interface for a periodic task (similar to a timer) which executes a delegate
	///     in a user defined interval. <see cref="ITaskScheduler.StartPeriodic(System.Action,System.TimeSpan,string)" />
	///     and <see cref="ITaskScheduler.StopPeriodic" />.
	/// </summary>
	public interface IPeriodicTask
	{
		/// <summary>
		///     The name of this task for debugging purposes.
		/// </summary>
		string Name { get; }

		/// <summary>
		///     The number of failures that occured while updating this task.
		///     More specifically, the number of exceptions that were thrown by the task's update function.
		/// </summary>
		int NumFailures { get; }

		/// <summary>
		///     The amount of time that should still ellapse until the next invocation of this periodic task.
		/// </summary>
		TimeSpan RemainingTimeUntilNextInvocation { get; }
	}
}