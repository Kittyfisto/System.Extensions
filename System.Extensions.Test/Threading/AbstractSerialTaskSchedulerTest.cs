using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace System.Extensions.Test.Threading
{
	[TestFixture]
	public abstract class AbstractSerialTaskSchedulerTest
	{
		protected abstract ISerialTaskScheduler Create();

		[Test]
		public void TestStartNew1()
		{
			using (var scheduler = Create())
			{
				bool executed = false;
				var task = scheduler.StartNew(() => executed = true);
				task.Should().NotBeNull();

				task.Wait(TimeSpan.FromSeconds(1)).Should().BeTrue("because the task should've been executed within 1 second");
				executed.Should().BeTrue("because the actual functor should've been executed");
			}
		}

		[Test]
		public void TestStartNew2()
		{
			using (var scheduler = Create())
			{
				var task = scheduler.StartNew(() =>
				{
					throw new UnauthorizedAccessException("Stuff");
				});
				task.Should().NotBeNull();

				new Action(() => task.Wait(TimeSpan.FromSeconds(1)))
					.ShouldThrow<AggregateException>()
					.WithInnerException<UnauthorizedAccessException>()
					.WithInnerMessage("Stuff");
			}
		}

		[Test]
		public void TestStartNew3([Values(0, 42, 9001)] int result)
		{
			using (var scheduler = Create())
			{
				var task = scheduler.StartNew(() => result);
				task.Should().NotBeNull();

				task.Wait(TimeSpan.FromSeconds(1)).Should().BeTrue("because the task should've been executed within 1 second");
				task.Result.Should().Be(result, "because the result from the functor should've been forwarded");
			}
		}

		[Test]
		public void TestStartNew4()
		{
			using (var scheduler = Create())
			{
				var task = scheduler.StartNew<string>(() =>
				{
					throw new UnauthorizedAccessException("Stuff");
				});
				task.Should().NotBeNull();

				new Action(() => task.Wait(TimeSpan.FromSeconds(1)))
					.ShouldThrow<AggregateException>()
					.WithInnerException<UnauthorizedAccessException>()
					.WithInnerMessage("Stuff");
			}
		}

		[Test]
		[Description("Verifies that tasks are indeed executed in the order they are started in")]
		public void TestStartInOrder()
		{
			const int taskCount = 4096;

			var results = new List<int>(taskCount);
			var tasks = new List<Task>(taskCount);

			using (var scheduler = Create())
			{
				for (int i = 0; i < taskCount; ++i)
				{
					int taskIndex = i;
					var task = scheduler.StartNew(() => results.Add(taskIndex));
					tasks.Add(task);
				}

				Task.WaitAll(tasks.ToArray());

				results.Count.Should().Be(taskCount);
				results.Should().Equal(Enumerable.Range(0, taskCount));
			}
		}

		[Test]
		[Description("Verifies that dispose may be called multiple times")]
		public void TestDispose2()
		{
			using (var scheduler = Create())
			{
				scheduler.Dispose();
				scheduler.Dispose();
			}
		}

		[Test]
		[Description("Verifies that tasks may be created on multiple threads")]
		public void TestStartManyTasks()
		{
			var tasks = new ConcurrentQueue<Task<int>>();

			const int numThreads = 10;
			const int numTasksPerThread = 4096;

			using (var scheduler = Create())
			{
				var threads = Enumerable.Range(0, numThreads).Select(threadIndex => Task.Factory.StartNew(() =>
				{
					for (int i = 0; i < numTasksPerThread; ++i)
					{
						int result = threadIndex * numTasksPerThread + i;
						var task = scheduler.StartNew(() => result);
						tasks.Enqueue(task);
					}
				}, TaskCreationOptions.LongRunning)).ToArray();

				Task.WaitAll(threads);
				Task.WaitAll(tasks.ToArray());

				var results = new HashSet<int>(tasks.Select(x => x.Result));
				for (int i = 0; i < numThreads * numTasksPerThread; ++i)
				{
					results.Should().Contain(i);
				}
			}
		}

		[Test]
		[Description("Verifies that tasks started after the scheduler has been disposed of are automatically cancelled")]
		public void TestStartNewAfterDispose1()
		{
			ISerialTaskScheduler scheduler;
			using (scheduler = Create())
			{ }

			bool executed = false;
			var task = scheduler.StartNew(() => executed = true);
			task.Should().NotBeNull("Because StartNew() should still return new tasks, even after having been disposed of");
			task.IsCanceled.Should().BeTrue("Because tasks created after StartNew() should be immediately cancelled");
			executed.Should().BeFalse();
		}

		[Test]
		[Description("Verifies that tasks started after the scheduler has been disposed of are automatically cancelled")]
		public void TestStartNewAfterDispose2()
		{
			var tasks = new ConcurrentQueue<Task>();

			const int numThreads = 10;
			const int numTasksPerThread = 4096;

			using (var scheduler = Create())
			{
				var threads = Enumerable.Range(0, numThreads).Select(threadIndex => Task.Factory.StartNew(() =>
				{
					for (int i = 0; i < numTasksPerThread; ++i)
					{
						if (i == 0 && i == numTasksPerThread / 2)
						{
							scheduler.Dispose();
						}

						var task = scheduler.StartNew(() => { });
						tasks.Enqueue(task);
					}
				}, TaskCreationOptions.LongRunning)).ToArray();

				Task.WaitAll(threads);
			}

			var cancelled = new HashSet<Task>(tasks.Where(x => x.IsCanceled));
			var nonCancelled = tasks.Where(x => !cancelled.Contains(x)).ToList();

			foreach (var task in nonCancelled)
			{
				task.IsCanceled.Should().BeFalse();
				task.IsFaulted.Should().BeFalse("because each and every non cancelled task should NOT have faulted");
				task.IsCompleted.Should().BeTrue("because each and every non cancelled task should've finished executing (and NOT stay in limbo)");
			}
		}
	}
}