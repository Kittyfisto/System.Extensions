using FluentAssertions;
using NUnit.Framework;

namespace System.Extensions.Test
{
	[TestFixture]
	public sealed class ByteArrayExtensionsTest
	{
		[Test]
		public void TestToHexString1()
		{
			new Action(() => ByteArrayExtensions.ToHexString(null)).Should().Throw<ArgumentNullException>();
		}

		[Test]
		public void TestToHexString2()
		{
			new byte[0].ToHexString().Should().BeEmpty();
		}

		[Test]
		public void TestToHexString3()
		{
			new byte[1].ToHexString().Should().Be("00");
		}

		[Test]
		public void TestToHexString4()
		{
			new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}.ToHexString().Should().Be("0102030405060708090a");
		}

		[Test]
		public void TestToHexString5()
		{
			new byte[] { 16, 15, 14, 13, 12, 11, 10 }.ToHexString().Should().Be("100f0e0d0c0b0a");
		}
	}
}