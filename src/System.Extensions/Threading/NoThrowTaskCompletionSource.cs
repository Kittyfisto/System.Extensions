using System.Threading.Tasks;

namespace System.Threading
{
	/// <summary>
	///     Similar to <see cref="TaskCompletionSource{TResult}" />, but produces <see cref="ITask" />s
	///     which require explicit checking of whether the task failed or not.
	/// </summary>
	public class NoThrowTaskCompletionSource
	{
		private readonly NoThrowTask _task;

		/// <summary>
		///     Initializes this object.
		/// </summary>
		public NoThrowTaskCompletionSource()
		{
			_task = new NoThrowTask();
		}

		/// <summary>
		/// </summary>
		public ITask Task => _task;

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public bool TrySetFinished()
		{
			return _task.TrySetFinished();
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public bool TrySetCanceled()
		{
			return _task.TrySetCanceled();
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public bool TrySetFaulted()
		{
			return _task.TrySetFaulted();
		}
	}

	/// <summary>
	///     Similar to <see cref="TaskCompletionSource{T}" />, but produces <see cref="ITask" />s
	///     which require explicit checking of whether the task failed or not.
	/// </summary>
	public sealed class NoThrowTaskCompletionSource<T>
	{
		private readonly NoThrowTask<T> _task;

		/// <summary>
		///     Initializes this object.
		/// </summary>
		public NoThrowTaskCompletionSource()
		{
			_task = new NoThrowTask<T>();
		}

		/// <summary>
		/// 
		/// </summary>
		public ITask<T> Task => _task;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TrySetResult(T value)
		{
			return _task.TrySetFinished(value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool TrySetCanceled()
		{
			return _task.TrySetCanceled();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public bool TrySetException(Exception e)
		{
			return _task.TrySetException(e);
		}
	}
}