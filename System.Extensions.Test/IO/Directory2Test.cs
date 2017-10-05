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
	}
}