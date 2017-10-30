namespace System.IO
{
	/// <summary>
	///     Provides extension methods for the stream class.
	/// </summary>
	public static class StreamExtensions
	{
		/// <summary>
		///     Returns a new byte array which contains from this stream's <see cref="Stream.Position" />
		///     until its end.
		/// </summary>
		/// <remarks>
		///     This method will throw a <see cref="OutOfMemoryException" /> when no byte array of the required
		///     size can be allocated.
		/// </remarks>
		/// <param name="that"></param>
		/// <returns></returns>
		public static byte[] ReadToEnd(this Stream that)
		{
			if (that == null)
				throw new NullReferenceException();

			using (var destination = CreateMemoryStream(that))
			{
				var buffer = new byte[4096];
				int bytesRead;
				while ((bytesRead = that.Read(buffer, offset: 0, count: buffer.Length)) > 0)
					destination.Write(buffer, offset: 0, count: bytesRead);

				return destination.ToArray();
			}
		}

		private static MemoryStream CreateMemoryStream(Stream that)
		{
			if (that.CanSeek)
			{
				// We can find out the length of the buffer we need to allocate:
				// This is preferrable because it allows us to skip the resizings
				// MemoryStream would have to do...
				var pos = that.Position;
				var length = that.Length;
				var remaining = length - pos;
				var buffer = new byte[remaining];
				return new MemoryStream(buffer);
			}

			// We cannot possibly find out the size of the stream without consuming it so
			// resizing MemoryStream's internal buffer cannot be avoided.
			return new MemoryStream();
		}
	}
}