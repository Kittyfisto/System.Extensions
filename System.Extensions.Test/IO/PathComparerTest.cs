using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace System.Extensions.Test.IO
{
	[TestFixture]
	public sealed class PathComparerTest
	{
		public static IEnumerable<string> InvalidPaths => new[]
		{
			null,
			"",
			"?"
		};

		[Test]
		public void TestEquals1([ValueSource(nameof(InvalidPaths))] string path)
		{
			var comparer = new PathComparer();
			comparer.Equals(path, "C:\\foo").Should().BeFalse();
			comparer.Equals("C:\\foo", path).Should().BeFalse();
		}

		[Test]
		public void TestEquals2()
		{
			var comparer = new PathComparer();
			comparer.Equals("C:\\foo", "c:/foo").Should().BeTrue();
		}

		[Test]
		public void TestEquals3()
		{
			var comparer = new PathComparer();
			comparer.Equals("FOO", "foo").Should().BeTrue();
		}

		[Test]
		public void TestEquals4()
		{
			var comparer = new PathComparer();
			comparer.Equals("FOO\\bar", "foo\\BAR").Should().BeTrue();
		}

		[Test]
		public void TestGetHashCode1([ValueSource(nameof(InvalidPaths))] string path)
		{
			var comparer = new PathComparer();
			new Action(() => comparer.GetHashCode(path)).ShouldNotThrow();
			comparer.GetHashCode(path).Should().Be(comparer.GetHashCode(path));
		}

		[Test]
		public void TestGetHashCode2()
		{
			var comparer = new PathComparer();
			comparer.GetHashCode("C:\\foo").Should().Be(comparer.GetHashCode("c:/foo"));
		}
	}
}