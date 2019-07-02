using System.IO;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace System.Extensions.Test.IO
{
	[TestFixture]
	public sealed class DirectoryInfoCompatibilityTest
		: AbstractFileTest
	{
		private Filesystem _filesystem;
		private DirectoryInfo _directory;
		private IDirectoryInfo _asyncDirectory;
		private ITaskScheduler _taskScheduler;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			_taskScheduler = new DefaultTaskScheduler();
			_filesystem = new Filesystem(_taskScheduler);

			_directory = new DirectoryInfo(Directory.GetCurrentDirectory());
			_asyncDirectory = _filesystem.Current;
		}

		[OneTimeTearDown]
		public void OneTimeTeardown()
		{}

		[Test]
		[Ignore("Not yet implemented")]
		public void TestCreateSubdirectory([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => _directory.CreateSubdirectory(invalidPath)).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.Current.CreateSubdirectory(invalidPath)).ShouldThrow<ArgumentException>();
		}
	}
}