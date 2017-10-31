using System.IO.Compression;

namespace System.IO
{
	/// <summary>
	///     Provides methods to compress/decompress byte array (using the <see cref="GZipStream" />).
	/// </summary>
	public static class GZip
	{
		/// <summary>
		///     The size of the buffer used to perform block-copies.
		/// </summary>
		/// <remarks>
		///     It makes sense to align this with the typical page size, no?.
		/// </remarks>
		private const int BufferSize = 4096;

		/// <summary>
		///     Compressses the given byte array using the GZip algorithm
		///     and <see cref="CompressionLevel.Optimal" />.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] Compress(byte[] data)
		{
			return Compress(data, CompressionLevel.Optimal);
		}

		/// <summary>
		///     Compressses the given byte array using the GZip algorithm
		///     and the <paramref name="compressionLevel" />.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="compressionLevel"></param>
		/// <returns></returns>
		public static byte[] Compress(byte[] data, CompressionLevel compressionLevel)
		{
			using (var uncompressed = new MemoryStream(data))
			{
				using (var compressed = new MemoryStream())
				{
					using (var gzip = new GZipStream(compressed, compressionLevel))
					{
						uncompressed.CopyTo(gzip);
					}

					return compressed.ToArray();
				}
			}
		}

		/// <summary>
		///     Decompresses the given data which has been previously compressed using the GZip algorithm
		///     (either via <see cref="Compress(byte[])" />, <see cref="Compress(byte[], CompressionLevel)" />,
		///     <see cref="GZipStream" /> or any equivalent method).
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] Decompress(byte[] data)
		{
			using (var compressedStream = new MemoryStream(data))
			{
				using (var decompressed = new MemoryStream())
				{
					using (var gzip = new GZipStream(compressedStream, CompressionMode.Decompress))
					{
						gzip.CopyTo(decompressed);
					}

					return decompressed.ToArray();
				}
			}
		}
	}
}