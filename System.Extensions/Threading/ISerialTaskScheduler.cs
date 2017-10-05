using System.Threading.Tasks;

namespace System.Threading
{
	/// <summary>
	///     Schedules all tasks it's been given in a serial fashion:
	///     - At no time will two tasks added to the same scheduler be executed concurrently
	///     - Tasks will be executed in the order they were added
	///     - A non-terminating task will cause all subsequent tasks to never be executed, ever
	///     - All pending tasks will be cancelled when the scheduler is disposed of
	/// </summary>
	public interface ISerialTaskScheduler
		: IDisposable
	{
		/// <summary>
		///     Starts a new task which will execute the given action of a background thread.
		///     The action will execute as soon as all previously started tasks have finished or faulted.
		/// </summary>
		/// <remarks>
		///     Returns an already cancelled task when <see cref="IDisposable.Dispose" /> has been called of.
		/// </remarks>
		/// <param name="fn"></param>
		/// <returns></returns>
		Task StartNew(Action fn);

		/// <summary>
		///     Starts a new task which will execute the given action of a background thread.
		///     The action will execute as soon as all previously started tasks have finished or faulted.
		/// </summary>
		/// <remarks>
		///     Returns an already cancelled task when <see cref="IDisposable.Dispose" /> has been called of.
		/// </remarks>
		/// <param name="fn"></param>
		/// <returns></returns>
		Task<T> StartNew<T>(Func<T> fn);
	}
}