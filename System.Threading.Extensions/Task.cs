namespace System.Threading
{
	/// <summary>
	/// </summary>
	public static class Task2
	{
		/// <summary>
		///     Creates a new task that is finished and presents the given result.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="result"></param>
		/// <returns></returns>
		public static ITask<T> FromResult<T>(T result)
		{
			return new Task2<T>(result);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static ITask<T> FromFailure<T>()
		{
			return new Task2<T>(default(T), isFaulted: true);
		}
	}

	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class Task2<T>
		: ITask<T>
	{
		private readonly bool _isCompleted;
		private readonly bool _isFaulted;
		private readonly T _result;
		private readonly bool _isCanceled;

		internal Task2(T result, bool isFaulted = false)
		{
			_result = result;
			_isCompleted = true;
			_isFaulted = isFaulted;
			_isCanceled = false;
		}

		/// <inheritdoc />
		public bool IsCompleted => _isCompleted;

		/// <inheritdoc />
		public bool IsCanceled => _isCanceled;

		/// <inheritdoc />
		public bool IsFaulted => _isFaulted;

		/// <inheritdoc />
		public void Wait()
		{
		}

		/// <inheritdoc />
		public bool Wait(TimeSpan timeout)
		{
			return true;
		}

		/// <inheritdoc />
		public T Result => _result;
	}
}