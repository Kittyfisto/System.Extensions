using System.IO;
using NUnit.Framework;

namespace System.Extensions.Test.IO
{
	[TestFixture]
	public sealed class InMemoryFilesystemTest
		: AbstractFilesystemTest
	{
		protected override IFilesystem Create()
		{
			return new InMemoryFilesystem();
		}
	}
}