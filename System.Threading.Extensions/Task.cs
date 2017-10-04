namespace System.Threading
{
	public static class Task2
	{
		public static ITask<T> FromResult<T>(T result)
		{
			return new Task2<T>(result);
		}

		public static ITask<T> FromFailure<T>()
		{
			return new Task2<T>(default(T), isFaulted: true);
		}
	}

	public sealed class Task2<T>
		: ITask<T>
	{
		private readonly T _result;
		private readonly bool _isCompleted;
		private readonly bool _isFaulted;
		private bool _isCanceled;

		internal Task2(T result, bool isFaulted = false)
		{
			_result = result;
			_isCompleted = true;
			_isFaulted = isFaulted;
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