using System.Threading;
using NUnit.Framework;

namespace System.Extensions.Test.Threading
{
	[TestFixture]
	public sealed class ImmediateTaskSchedulerTest
		: AbstractSerialTaskSchedulerTest
	{
		protected override ISerialTaskScheduler Create()
		{
			return new ImmediateTaskScheduler();
		}
	}
}