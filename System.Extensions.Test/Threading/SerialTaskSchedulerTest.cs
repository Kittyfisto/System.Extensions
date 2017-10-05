using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace System.Extensions.Test.Threading
{
	[TestFixture]
	public sealed class SerialTaskSchedulerTest
		: AbstractSerialTaskSchedulerTest
	{
		protected override ISerialTaskScheduler Create()
		{
			return new SerialTaskScheduler();
		}

		[Test]
		public void TestCtor1()
		{
			using (var scheduler = new SerialTaskScheduler())
			{
				scheduler.DebugName.Should().BeNull("because we haven't specified any name");
			}
		}

		[Test]
		public void TestCtor2()
		{
			using (var scheduler = new SerialTaskScheduler("Foobar"))
			{
				scheduler.DebugName.Should().Be("Foobar");

				// TODO: Test if the thread has been given the correct name...
			}
		}

		[Test]
		[Description("Verifies that tasks which have not been executed yet will be cancelled when the scheduler is disposed of")]
		public void TestDispose1()
		{
			Task task;

			using (var scheduler = Create())
			{
				scheduler.StartNew(() => Thread.Sleep(TimeSpan.FromSeconds(10)));
				task = scheduler.StartNew(() => 42);
			}

			task.IsCanceled.Should().BeTrue("because the scheduler should've cancelled the 2nd task because it's still busy executing the first task");
		}
	}
}