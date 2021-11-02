using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace System.Extensions.Test.IO
{
	[TestFixture]
	public sealed class Directory2Test
	{
		[Test]
		public void TestTokenize1()
		{
			Directory2.Tokenise(@"M:\")
				.Should().Equal(new object[]
				{
					@"M:\"
				});
		}

		[Test]
		public void TestTokenize2()
		{
			Directory2.Tokenise(@"\\Server\Share")
				.Should().Equal(new object[]
				{
					@"\\Server\",
					@"\\Server\Share"
				});
		}

		[Test]
		public void TestTokenize3()
		{
			Directory2.Tokenise(@"M:\Some\Stuff")
				.Should().Equal(new object[]
				{
					@"M:\",
					@"M:\Some",
					@"M:\Some\Stuff"
				});
		}

		[Test]
		public void TestTokenize4()
		{
			Directory2.Tokenise(@"C:\Snapshots\Metrolib\Metrolib.Test\Panel\GridPanelTest.cs")
				.Should().Equal(new object[]
				{
					@"C:\",
					@"C:\Snapshots",
					@"C:\Snapshots\Metrolib",
					@"C:\Snapshots\Metrolib\Metrolib.Test",
					@"C:\Snapshots\Metrolib\Metrolib.Test\Panel",
					@"C:\Snapshots\Metrolib\Metrolib.Test\Panel\GridPanelTest.cs"
				});
		}

		[Test]
		public void TestSplit1()
		{
			Directory2.Split(@"M:\")
				.Should().Equal(new object[]
				{
					@"M:\"
				});
		}

		[Test]
		public void TestSplit2()
		{
			Directory2.Split(@"\\Server\")
				.Should().Equal(new object[]
				{
					@"\\Server\"
				});
		}

		[Test]
		public void TestSplit3()
		{
			Directory2.Split(@"\\Server\Share")
				.Should().Equal(new object[]
				{
					@"\\Server\",
					"Share"
				});
		}

		[Test]
		public void TestSplit4()
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
		public void TestSplit5()
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
		public void TestSplit6()
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
		public void TestSplit7()
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
		public void TestSplit8()
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