using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace System.Extensions.Test.IO
{
	[TestFixture]
	public sealed class Path2Test
	{
#if NET
        public static IEnumerable<string> InvalidPaths => new[]
        {
            null,
            "",
            "\0"
        };
#else
		public static IEnumerable<string> InvalidPaths => new[]
		{
			null,
			"",
			@"C:\:",
			@"C:\?",
			@"X:\>",
			"\0"
		};
#endif

		public static IEnumerable<string> ValidPaths => new[]
		{
			@"C:\",
			@"C:\system\windows32"
		};

		[Test]
		public void TestIsValidPath1([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			Path2.IsValidPath(invalidPath).Should().BeFalse();
		}

		[Test]
		public void TestIsValidPath2([ValueSource(nameof(ValidPaths))] string validPath)
		{
			Path2.IsValidPath(validPath).Should().BeTrue();
		}

		[Test]
		public void TestRemoveTrailingSeparators()
		{
			Path2.RemoveTrailingSeparators(@"C:\users\temp\").Should().Be(@"C:\users\temp");
			Path2.RemoveTrailingSeparators(@"Debug\").Should().Be("Debug");
			Path2.RemoveTrailingSeparators(@"Debug/").Should().Be("Debug");
			Path2.RemoveTrailingSeparators(@"Debug").Should().Be("Debug");
		}
	}
}