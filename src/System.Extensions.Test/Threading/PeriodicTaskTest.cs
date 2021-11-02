using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace System.Extensions.Test.Threading
{
	[TestFixture]
	public sealed class PeriodicTaskTest
	{
		[Test]
		public void TestCtor()
		{
			var task = new PeriodicTask(42, () => { }, TimeSpan.FromSeconds(1));
			task.Id.Should().Be(42);
			task.IsStopped.Should().BeFalse();
			task.LastInvocation.Should().Be(DateTime.MinValue);
		}
	}
}