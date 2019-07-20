using System.Runtime.InteropServices.WindowsRuntime;
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
		[Description("Verifies that a task can still be cancelled if it hasn't started executing yet")]
		public void TestCancelAfterStart1()
		{
			using (var scheduler = Create())
			using (var waitHandle = new ManualResetEvent(false))
			{
				var task1 = scheduler.StartNew(() => waitHandle.WaitOne());

				var cancellationTokenSource = new CancellationTokenSource();
				bool executed = false;
				var task2 = scheduler.StartNew(() =>
				{
					executed = true;
				}, cancellationTokenSource.Token);

				task2.IsCompleted.Should().BeFalse();
				task2.IsCanceled.Should().BeFalse();
				task2.IsFaulted.Should().BeFalse();

				cancellationTokenSource.Cancel();
				waitHandle.Set();
				task1.Wait(TimeSpan.FromSeconds(1)).Should().BeTrue("because the first task should've finished by now");

				new Action(() => task2.Wait(TimeSpan.FromSeconds(1))).Should().Throw<TaskCanceledException>("because the second task should've been cancelled");
				task2.IsCompleted.Should().BeTrue("because the second task should've been cancelled");
				task2.IsCanceled.Should().BeTrue("because the second task should've been cancelled");
				task2.IsFaulted.Should().BeFalse("becuase the second task should've been cancelled");
				executed.Should().BeFalse();
			}
		}

		[Test]
		[Description("Verifies that a task can still be cancelled if it hasn't started executing yet")]
		public void TestCancelAfterStart2()
		{
			using (var scheduler = Create())
			using (var waitHandle = new ManualResetEvent(false))
			{
				var task1 = scheduler.StartNew(() => waitHandle.WaitOne());

				var cancellationTokenSource = new CancellationTokenSource();
				bool executed = false;
				var task2 = scheduler.StartNew(unused =>
				{
					executed = true;
				}, cancellationTokenSource.Token);

				task2.IsCompleted.Should().BeFalse();
				task2.IsCanceled.Should().BeFalse();
				task2.IsFaulted.Should().BeFalse();

				cancellationTokenSource.Cancel();
				waitHandle.Set();
				task1.Wait(TimeSpan.FromSeconds(1)).Should().BeTrue("because the first task should've finished by now");

				new Action(() => task2.Wait(TimeSpan.FromSeconds(1))).Should().Throw<TaskCanceledException>("because the second task should've been cancelled");
				task2.IsCompleted.Should().BeTrue("becuase the second task should've been cancelled");
				task2.IsCanceled.Should().BeTrue("because the second task should've been cancelled");
				task2.IsFaulted.Should().BeFalse("becuase the second task should've been cancelled");
				executed.Should().BeFalse();
			}
		}

		[Test]
		[Description("Verifies that a task can still be cancelled if it hasn't started executing yet")]
		public void TestCancelAfterStart3()
		{
			using (var scheduler = Create())
			using (var waitHandle = new ManualResetEvent(false))
			{
				var task1 = scheduler.StartNew(() => waitHandle.WaitOne());

				var cancellationTokenSource = new CancellationTokenSource();
				bool executed = false;
				var task2 = scheduler.StartNew(() =>
				{
					executed = true;
					return 42;
				}, cancellationTokenSource.Token);

				task2.IsCompleted.Should().BeFalse();
				task2.IsCanceled.Should().BeFalse();
				task2.IsFaulted.Should().BeFalse();

				cancellationTokenSource.Cancel();
				waitHandle.Set();
				task1.Wait(TimeSpan.FromSeconds(1)).Should().BeTrue("because the first task should've finished by now");

				new Action(() => task2.Wait(TimeSpan.FromSeconds(1))).Should().Throw<TaskCanceledException>("because the second task should've been cancelled");
				task2.IsCompleted.Should().BeTrue("becuase the second task should've been cancelled");
				task2.IsCanceled.Should().BeTrue("because the second task should've been cancelled");
				task2.IsFaulted.Should().BeFalse("becuase the second task should've been cancelled");
				executed.Should().BeFalse();
			}
		}

		[Test]
		[Description("Verifies that a task can still be cancelled if it hasn't started executing yet")]
		public void TestCancelAfterStart4()
		{
			using (var scheduler = Create())
			using (var waitHandle = new ManualResetEvent(false))
			{
				var task1 = scheduler.StartNew(() => waitHandle.WaitOne());

				var cancellationTokenSource = new CancellationTokenSource();
				bool executed = false;
				var task2 = scheduler.StartNew(unused =>
				{
					executed = true;
					return 42;
				}, cancellationTokenSource.Token);

				task2.IsCompleted.Should().BeFalse();
				task2.IsCanceled.Should().BeFalse();
				task2.IsFaulted.Should().BeFalse();

				cancellationTokenSource.Cancel();
				waitHandle.Set();
				task1.Wait(TimeSpan.FromSeconds(1)).Should().BeTrue("because the first task should've finished by now");

				new Action(() => task2.Wait(TimeSpan.FromSeconds(1))).Should().Throw<TaskCanceledException>("because the second task should've been cancelled");
				task2.IsCompleted.Should().BeTrue("becuase the second task should've been cancelled");
				task2.IsCanceled.Should().BeTrue("because the second task should've been cancelled");
				task2.IsFaulted.Should().BeFalse("becuase the second task should've been cancelled");
				executed.Should().BeFalse();
			}
		}

		[Test]
		[Description("Verifies that tasks which have not been executed yet will be cancelled when the scheduler is disposed of")]
		public void TestDispose2()
		{
			Task task;

			using (var scheduler = Create())
			{
				scheduler.StartNew(() => Thread.Sleep(TimeSpan.FromSeconds(10)));
				task = scheduler.StartNew(() => 42);
			}

			task.IsCanceled.Should().BeTrue("because the scheduler should've cancelled the 2nd task because it's still busy executing the first task");
		}

		[Test]
		[LocalTest("Mysteriously fails on AppVeyor - need to investigate as to why")]
		[Description("Verifies that tasks which have not been executed yet will be cancelled when the scheduler is disposed of")]
		public void TestDispose3()
		{
			Task task;

			using (var scheduler = Create())
			{
				scheduler.StartNew(() => Thread.Sleep(TimeSpan.FromSeconds(10)));
				task = scheduler.StartNew(() => 42, new CancellationToken(false));
			}

			task.IsCanceled.Should().BeTrue("because the scheduler should've cancelled the 2nd task because it's still busy executing the first task");
		}

		[Test]
		[LocalTest("Mysteriously fails on AppVeyor - need to investigate as to why")]
		[Description("Verifies that tasks which have not been executed yet will be cancelled when the scheduler is disposed of")]
		public void TestDispose4()
		{
			Task task;

			using (var scheduler = Create())
			{
				scheduler.StartNew(() => Thread.Sleep(TimeSpan.FromSeconds(10)));
				task = scheduler.StartNew(unused => 42, new CancellationToken(false));
			}

			task.IsCanceled.Should().BeTrue("because the scheduler should've cancelled the 2nd task because it's still busy executing the first task");
		}
	}
}