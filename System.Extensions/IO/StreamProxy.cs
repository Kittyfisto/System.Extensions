namespace System.IO
{
	/// <summary>
	///     A proxy for another <see cref="Stream" /> which does not own it.
	///     Calls to <see cref="IDisposable.Dispose" /> to not dispose of the inner stream.
	/// </summary>
	public sealed class StreamProxy
		: Stream
	{
		private readonly Stream _inner;
		private readonly bool _allowRead;
		private readonly bool _allowWrite;
		private readonly bool _allowSeek;
		private bool _isDisposed;

		/// <summary>
		///     Initializes this stream to provide access to the given one.
		///     Does not take ownership of the given stream: Disposing of this stream
		///     does not dispose of the inner stream!.
		/// </summary>
		/// <param name="inner"></param>
		/// <param name="allowRead">When set to false, then this stream may not be read from, even if the inner stream allows reading</param>
		/// <param name="allowWrite">When set to false, then this stream may not be written to, even if the inner stream allows writing</param>
		/// <param name="allowSeek">When set to false, then this stream may not be seeked, even if the inner stream allows seeking</param>
		public StreamProxy(Stream inner, bool allowRead = true, bool allowWrite = true, bool allowSeek = true)
		{
			if (inner == null)
				throw new ArgumentNullException(nameof(inner));

			_inner = inner;
			_allowRead = allowRead;
			_allowWrite = allowWrite;
			_allowSeek = allowSeek;
		}


		/// <inheritdoc />
		public override bool CanRead => _allowRead && _inner.CanRead;

		/// <inheritdoc />
		public override bool CanSeek => _allowSeek && _inner.CanSeek;

		/// <inheritdoc />
		public override bool CanWrite => _allowWrite && _inner.CanWrite;

		/// <inheritdoc />
		public override long Length => _inner.Length;

		/// <inheritdoc />
		public override long Position
		{
			get => _inner.Position;
			set
			{
				if (_isDisposed)
					throw new ObjectDisposedException("This stream has been disposed of");

				if (!CanSeek)
					throw new NotSupportedException("This stream does not support seeking");

				_inner.Position = value;
			}
		}

		/// <inheritdoc />
		public override void Flush()
		{
			// Yes, it's intentional we don't throw: The documentation
			// doesn't specify that this method may throw ObjectDisposedException...

			if (!_isDisposed)
			{
				_inner.Flush();
			}
		}

		/// <inheritdoc />
		public override long Seek(long offset, SeekOrigin origin)
		{
			if (_isDisposed)
				throw new ObjectDisposedException("This stream has been disposed of");

			if (!CanSeek)
				throw new NotSupportedException("This stream does not support seeking");

			return _inner.Seek(offset, origin);
		}

		/// <inheritdoc />
		public override void SetLength(long value)
		{
			if (_isDisposed)
				throw new ObjectDisposedException("This stream has been disposed of");

			if (!CanWrite)
				throw new NotSupportedException("This stream does not allow writing");

			_inner.SetLength(value);
		}

		/// <inheritdoc />
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (_isDisposed)
				throw new ObjectDisposedException("This stream has been disposed of");

			if (!CanRead)
				throw new NotSupportedException("This stream does not allow reading");

			return _inner.Read(buffer, offset, count);
		}

		/// <inheritdoc />
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (_isDisposed)
				throw new ObjectDisposedException("This stream has been disposed of");

			if (!CanWrite)
				throw new NotSupportedException("This stream does not allow writing");

			_inner.Write(buffer, offset, count);
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				_isDisposed = true;
			}
		}
	}
}