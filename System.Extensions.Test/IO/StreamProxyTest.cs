using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace System.Extensions.Test.IO
{
	[TestFixture]
	public sealed class StreamProxyTest
	{
		[Test]
		public void TestCtor1()
		{
			var stream = new MemoryStream();
			var proxy = new StreamProxy(stream);
			proxy.CanRead.Should().BeTrue();
			proxy.CanWrite.Should().BeTrue();
			proxy.Position.Should().Be(0);
			proxy.Length.Should().Be(0);
		}

		[Test]
		public void TestCtor2()
		{
			var stream = new MemoryStream();
			stream.Write(new byte[42], 0, 42);

			var proxy = new StreamProxy(stream);
			proxy.Position.Should().Be(42);
			proxy.Length.Should().Be(42);
		}

		[Test]
		public void TestCtor3([Values(true, false)] bool allowRead,
							  [Values(true, false)] bool allowWrite,
							  [Values(true, false)] bool allowSeek)
		{
			var stream = new MemoryStream();
			var proxy = new StreamProxy(stream, allowRead, allowWrite, allowSeek);
			proxy.CanRead.Should().Be(allowRead);
			proxy.CanWrite.Should().Be(allowWrite);
			proxy.CanSeek.Should().Be(allowSeek);
		}

		[Test]
		public void TestWrite1()
		{
			var stream = new MemoryStream();
			var proxy = new StreamProxy(stream, allowWrite: false);

			const string reason = "because the stream does not support writing";
			new Action(() => proxy.WriteByte(32)).ShouldThrow<NotSupportedException>(reason);
			new Action(() => proxy.Write(new byte[32], 0, 32)).ShouldThrow<NotSupportedException>(reason);

			stream.Length.Should().Be(0, "because the original stream should not have been written to");
		}

		[Test]
		public void TestWrite2()
		{
			var stream = new MemoryStream();
			var proxy = new StreamProxy(stream);

			proxy.WriteByte(32);
			stream.Length.Should().Be(1);
			stream.Position.Should().Be(1);
			stream.Position = 0;
			stream.ReadByte().Should().Be(32);
		}

		[Test]
		public void TestRead1()
		{
			var stream = new MemoryStream();
			stream.Write(new byte[10], 0, 10);
			stream.Position = 0;

			var proxy = new StreamProxy(stream, allowRead: false);

			const string reason = "because the stream does not support reading";
			new Action(() => proxy.ReadByte()).ShouldThrow<NotSupportedException>(reason);
			new Action(() => proxy.Read(new byte[32], 0, 32)).ShouldThrow<NotSupportedException>(reason);

			stream.Position.Should().Be(0, "because the original stream should not have been read from");
		}

		[Test]
		public void TestRead2()
		{
			var stream = new MemoryStream();
			stream.WriteByte(42);
			stream.Position = 0;

			var proxy = new StreamProxy(stream);
			proxy.ReadByte().Should().Be(42);
		}

		[Test]
		public void TestSetLength1()
		{
			var stream = new MemoryStream();
			var proxy = new StreamProxy(stream);
			stream.Length.Should().Be(0);

			proxy.SetLength(42);
			proxy.Length.Should().Be(42);
			stream.Length.Should().Be(42);
		}

		[Test]
		public void TestSetLength2()
		{
			var stream = new MemoryStream();
			var proxy = new StreamProxy(stream, allowWrite: false);
			stream.Length.Should().Be(0);

			new Action(() => proxy.SetLength(42)).ShouldThrow<NotSupportedException>("because the stream does not support writing");
			proxy.Length.Should().Be(0);
			stream.Length.Should().Be(0);
		}

		[Test]
		public void TestFlush1([Values(true, false)] bool allowWrite)
		{
			var stream = new MemoryStream();
			var proxy = new StreamProxy(stream, allowWrite: allowWrite);
			new Action(() => proxy.Flush()).ShouldNotThrow();
		}

		[Test]
		public void TestPosition1()
		{
			var stream = new MemoryStream();
			stream.WriteByte(1);
			stream.WriteByte(1);
			var proxy = new StreamProxy(stream, allowSeek: false);
			new Action(() => proxy.Position = 1).ShouldThrow<NotSupportedException>("because the stream does not support seeking");
		}

		[Test]
		public void TestPosition2()
		{
			var stream = new MemoryStream();
			stream.WriteByte(1);
			stream.WriteByte(42);

			var proxy = new StreamProxy(stream) {Position = 1};
			proxy.ReadByte().Should().Be(42);
			proxy.Position.Should().Be(2);
		}

		[Test]
		public void TestSeek1()
		{
			var stream = new MemoryStream();
			stream.WriteByte(1);
			stream.WriteByte(1);
			var proxy = new StreamProxy(stream, allowSeek: false);
			new Action(() => proxy.Seek(1, SeekOrigin.Begin)).ShouldThrow<NotSupportedException>("because the stream does not support seeking");
		}

		[Test]
		public void TestDispose()
		{
			var stream = new MemoryStream();
			var proxy = new StreamProxy(stream);
			proxy.Dispose();

			const string reason = "because the proxy has been disposed of";
			new Action(() => proxy.WriteByte(32)).ShouldThrow<ObjectDisposedException>(reason);
			new Action(() => proxy.Write(new byte[1], 0, 1)).ShouldThrow<ObjectDisposedException>(reason);
			new Action(() => proxy.ReadByte()).ShouldThrow<ObjectDisposedException>(reason);
			new Action(() => proxy.Read(new byte[1], 0, 1)).ShouldThrow<ObjectDisposedException>(reason);
			new Action(() => proxy.Position = 32).ShouldThrow<ObjectDisposedException>(reason);
			new Action(() => proxy.Seek(1, SeekOrigin.Current)).ShouldThrow<ObjectDisposedException>(reason);
			new Action(() => proxy.SetLength(1)).ShouldThrow<ObjectDisposedException>(reason);

			new Action(() => proxy.Flush()).ShouldNotThrow("because Flush() isn't specified to throw if called on a disposed object");
		}
	}
}