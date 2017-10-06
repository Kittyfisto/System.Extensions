using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace System.Extensions.Test.IO
{
	[TestFixture]
	public sealed class Directory2Test
	{
		[Test]
		public void TestSplit1()
		{
			Directory2.Split(@"M:\Some\Stuff")
				.Should().Equal(new object[]
				{
					@"M:\",
					"Some",
					"Stuff"
				});
		}

		[Test]
		public void TestSplit2()
		{
			Directory2.Split(@"M:\Some\Stuff\")
				.Should().Equal(new object[]
				{
					@"M:\",
					"Some",
					"Stuff\\"
				});
		}

		[Test]
		public void TestSplit3()
		{
			Directory2.Split(@"M:\Some\Stuff/")
				.Should().Equal(new object[]
				{
					@"M:\",
					"Some",
					@"Stuff\"
				});
		}

		[Test]
		public void TestSplit4()
		{
			Directory2.Split(@"M:/Some\Stuff/")
				.Should().Equal(new object[]
				{
					@"M:\",
					"Some",
					@"Stuff\"
				});
		}

		[Test]
		public void TestSplit5()
		{
			Directory2.Split(@"M:/Some/Stuff/")
				.Should().Equal(new object[]
				{
					@"M:\",
					"Some",
					@"Stuff\"
				});
		}
	}
}