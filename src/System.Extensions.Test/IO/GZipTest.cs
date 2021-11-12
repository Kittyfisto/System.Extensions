using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace System.Extensions.Test.IO
{
	[TestFixture]
	public sealed class GZipTest
	{
		public static IEnumerable<byte[]> TestData => new[]
		{
			new byte[0],
			new byte[] {1},
			new byte[] {1, 2, 3, 4},
			new byte[42],
			new byte[1024],
			Enumerable.Range(0, 1024 * 1024).Select(i => (byte)(i % byte.MaxValue)).ToArray()
		};

		public static IEnumerable<CompressionLevel> Levels => new[]
		{
			CompressionLevel.Optimal,
			CompressionLevel.Fastest,
			CompressionLevel.NoCompression
		};

		[Test]
		public void TestRoundtrip1([ValueSource(nameof(TestData))] byte[] data)
		{
			var compressed = GZip.Compress(data);
			var uncompressed = GZip.Decompress(compressed);
			uncompressed.Should().Equal(data);

			Console.WriteLine("Compressed {0} bytes to {1} bytes, down to {2:F1}% of original", data.Length, compressed.Length,
				100.0*compressed.Length/data.Length);
		}

		[Test]
		public void TestRoundtrip2([ValueSource(nameof(TestData))] byte[] data,
			[ValueSource(nameof(Levels))] CompressionLevel level)
		{
			var compressed = GZip.Compress(data, level);
			var uncompressed = GZip.Decompress(compressed);
			uncompressed.Should().Equal(data);

			Console.WriteLine("Compressed {0} bytes to {1} bytes, down to {2:F1}% of original", data.Length, compressed.Length,
				100.0 * compressed.Length / data.Length);
		}
	}
}