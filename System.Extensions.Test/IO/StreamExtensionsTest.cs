using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace System.Extensions.Test.IO
{
	[TestFixture]
	public sealed class StreamExtensionsTest
	{
		[Test]
		public void TestReadToEnd1()
		{
			new Action(() => ((Stream) null).ReadToEnd()).Should().Throw<NullReferenceException>();
		}

		[Test]
		public void TestReadToEnd2()
		{
			var stream = new MemoryStream();
			stream.ReadToEnd().Should().BeEmpty();
		}

		[Test]
		public void TestReadToEnd3()
		{
			var rng = new Random(42);
			var data = new byte[4097];
			rng.NextBytes(data);
			var stream = new MemoryStream(data);
			stream.ReadToEnd().Should().Equal(data);
		}

		[Test]
		public void TestReadToEnd4()
		{
			var rng = new Random(42);
			var data = new byte[4097];
			rng.NextBytes(data);
			var stream = new StreamProxy(new MemoryStream(data), allowSeek: false);
			stream.ReadToEnd().Should().Equal(data);
		}
	}
}