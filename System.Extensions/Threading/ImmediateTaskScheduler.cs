using System.Threading.Tasks;

namespace System.Threading
{
	/// <summary>
	///     A <see cref="ISerialTaskScheduler" /> implementation which executes all tasks
	///     from within <see cref="ISerialTaskScheduler.StartNew(Action)" />.
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
			return StartNew(fn, new CancellationToken(false));
		}

		/// <inheritdoc />
		public Task StartNew(Action fn, CancellationToken cancellationToken)
		{
			return StartNew(unused => fn(), cancellationToken);
		}

		/// <inheritdoc />
		public Task StartNew(Action<CancellationToken> fn, CancellationToken cancellationToken)
		{
			var info = new TaskCompletionSource<int>();
			if (_isDisposed || cancellationToken.IsCancellationRequested)
			{
				info.SetCanceled();
			}
			else
			{
				try
				{
					fn(cancellationToken);
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
			return StartNew(fn, new CancellationToken(false));
		}

		/// <inheritdoc />
		public Task<T> StartNew<T>(Func<T> fn, CancellationToken cancellationToken)
		{
			return StartNew(unused => fn(), cancellationToken);
		}

		/// <inheritdoc />
		public Task<T> StartNew<T>(Func<CancellationToken, T> fn, CancellationToken cancellationToken)
		{
			var info = new TaskCompletionSource<T>();
			if (_isDisposed || cancellationToken.IsCancellationRequested)
			{
				info.SetCanceled();
			}
			else
			{
				try
				{
					var result = fn(cancellationToken);
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