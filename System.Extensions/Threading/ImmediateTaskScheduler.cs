using System.Threading.Tasks;

namespace System.Threading
{
	/// <summary>
	///     A <see cref="ISerialTaskScheduler" /> implementation which executes all tasks
	///     from within <see cref="ISerialTaskScheduler.StartNew" />.
	///     This class is intended to be used in unit tests.
	/// </summary>
	public sealed class ImmediateTaskScheduler
		: ISerialTaskScheduler
	{
		private bool _isDisposed;

		/// <inheritdoc />
		public void Dispose()
		{
			_isDisposed = true;
		}

		/// <inheritdoc />
		public Task StartNew(Action fn)
		{
			var info = new TaskCompletionSource<int>();
			if (_isDisposed)
			{
				info.SetCanceled();
			}
			else
			{
				try
				{
					fn();
					info.SetResult(42);
				}
				catch (Exception e)
				{
					info.SetException(e);
				}
			}
			return info.Task;
		}

		/// <inheritdoc />
		public Task<T> StartNew<T>(Func<T> fn)
		{
			var info = new TaskCompletionSource<T>();
			if (_isDisposed)
			{
				info.SetCanceled();
			}
			else
			{
				try
				{
					var result = fn();
					info.SetResult(result);
				}
				catch (Exception e)
				{
					info.SetException(e);
				}
			}
			return info.Task;
		}
	}
}