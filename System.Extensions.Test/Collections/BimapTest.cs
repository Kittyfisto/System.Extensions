using System.Collections;
using FluentAssertions;
using NUnit.Framework;

namespace System.Extensions.Test.Collections
{
	[TestFixture]
	public sealed class BimapTest
	{
		[Test]
		public void Test()
		{
			var map = new Bimap<int, string>
			{
				{1, "1" },
				{2, "2"}
			};
			map.Forward.ContainsKey(1).Should().BeTrue();
			map.Backward.ContainsKey("1").Should().BeTrue();
			map.Forward.ContainsKey(3).Should().BeFalse();
			map.Backward.ContainsKey("3").Should().BeFalse();
		}
	}
}
