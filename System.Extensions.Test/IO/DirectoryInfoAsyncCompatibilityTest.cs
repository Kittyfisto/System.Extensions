using System.IO;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace System.Extensions.Test.IO
{
	[TestFixture]
	public sealed class DirectoryInfoAsyncCompatibilityTest
		: AbstractFileTest
	{
		private SerialTaskScheduler _scheduler;
		private Filesystem _filesystem;
		private DirectoryInfo _directory;
		private IDirectoryInfoAsync _asyncDirectory;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			_scheduler = new SerialTaskScheduler();
			_filesystem = new Filesystem(_scheduler);

			_directory = new DirectoryInfo(Directory.GetCurrentDirectory());
			_asyncDirectory = _filesystem.Current;
		}

		[OneTimeTearDown]
		public void OneTimeTeardown()
		{
			_scheduler.Dispose();
		}

		[Test]
		[Ignore("Not yet implemented")]
		public void TestCreateSubdirectory([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => _directory.CreateSubdirectory(invalidPath)).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.Current.CreateSubdirectory(invalidPath)).ShouldThrow<ArgumentException>();
		}
	}
}