using System.IO;
using NUnit.Framework;

namespace System.Extensions.Test.IO
{
	[TestFixture]
	[Ignore("Class not yet fully implemented")]
	public sealed class InMemoryFilesystemTest
		: AbstractFilesystemTest
	{
		protected override IFilesystem Create()
		{
			return new InMemoryFilesystem();
		}
	}
}