namespace System.Threading
{
	/// <summary>
	///     The interface for an asynchronous operation where the caller is NOT interested
	///     in why the operation failed. Contrary to <see cref="NoThrowTask" />, failures are NOT
	///     communicated by throwing exceptions from <see cref="ITask.Wait()" />, but
	///     have to be queried EXCPLITLY.
	/// </summary>
	public interface ITask
	{
		/// <summary>
		///     True when this task has been completed, false otherwise.
		/// </summary>
		bool IsCompleted { get; }

		/// <summary>
		///     True when this task has been cancelled, false otherwise.
		/// </summary>
		bool IsCanceled { get; }

		/// <summary>
		///     True when this task has faulted, false otherwise.
		/// </summary>
		bool IsFaulted { get; }

		/// <summary>
		///     Blocks until the task completes or faults.
		/// </summary>
		void Wait();

		/// <summary>
		///     Blocks until the task completes, faults, or the given amount of time elapses.
		/// </summary>
		/// <param name="timeout"></param>
		/// <returns></returns>
		bool Wait(TimeSpan timeout);
	}

	/// <summary>
	///     The interface for a future where the caller is NOT interested
	///     in why the operation failed. Contrary to <see cref="NoThrowTask{T}" />, failures are NOT
	///     communicated by throwing exceptions from <see cref="ITask.Wait()" />, but
	///     have to be queried EXCPLITLY.
	/// </summary>
	public interface ITask<out T>
		: ITask
	{
		/// <summary>
		///     The result of the task or default(T) when the operation failed or was cancelled.
		///     <see cref="ITask.IsFaulted" /> and <see cref="ITask.IsCanceled" /> should be queried
		///     to make sure that the result may be used.
		/// </summary>
		T Result { get; }
	}
}