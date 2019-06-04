using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace System.Extensions.Test.Threading
{
	[TestFixture]
	[LocalTest("Won't work on AppVeyor anymore...")]
	public sealed class DefaultTaskSchedulerTest
	{
		[Test]
		public void TestDispose1()
		{
			var scheduler = new DefaultTaskScheduler();
			new Action(scheduler.Dispose).ShouldNotThrow();
		}

		[Test]
		public void TestStart1()
		{
			using (var scheduler = new DefaultTaskScheduler())
			{
				scheduler.Start(() => "foobar").Result.Should().Be("foobar");
			}
		}

		[Test]
		public void TestStartPeriodic1()
		{
			using (var scheduler = new DefaultTaskScheduler())
			{
				var task = scheduler.StartPeriodic(() => { }, TimeSpan.FromSeconds(1));
				task.Should().NotBeNull();
				scheduler.StopPeriodic(task).Should().BeTrue();
			}
		}

		[Test]
		public void TestStartPeriodic2()
		{
			using (var scheduler = new DefaultTaskScheduler())
			{
				int counter = 0;
				var task = scheduler.StartPeriodic(() => ++counter, TimeSpan.Zero);

				new object().Property(x => counter).ShouldAfter(TimeSpan.FromSeconds(5)).BeGreaterOrEqualTo(100,
				                                                                                          "because our periodic task should've been executed at least 100 times by now");

				scheduler.StopPeriodic(task);
			}
		}

		[Test]
		public void TestStopPeriodic1()
		{
			using (var scheduler = new DefaultTaskScheduler())
			{
				scheduler.StopPeriodic(new Mock<IPeriodicTask>().Object).Should().BeFalse();
			}
		}

		[Test]
		public void TestStopPeriodic2()
		{
			using (var scheduler = new DefaultTaskScheduler())
			{
				var task = scheduler.StartPeriodic(() => { }, TimeSpan.FromSeconds(1));
				var actual = task as PeriodicTask;
				actual.Should().NotBeNull();
				actual.IsRemoved.Should().BeFalse();

				scheduler.StopPeriodic(task).Should().BeTrue();
				actual.IsRemoved.Should().BeTrue();
			}
		}

		[Test]
		public void TestStopPeriodic3()
		{
			using (var scheduler1 = new DefaultTaskScheduler())
			using (var scheduler2 = new DefaultTaskScheduler())
			{
				var task = scheduler1.StartPeriodic(() => { }, TimeSpan.FromSeconds(1));
				var actual = task as PeriodicTask;
				actual.Should().NotBeNull();
				actual.IsRemoved.Should().BeFalse();

				scheduler2.StopPeriodic(task).Should().BeFalse("because this scheduler didn't create the task");
				actual.IsRemoved.Should().BeFalse("because the task shouldn't have been stopped");
			}
		}
	}
}