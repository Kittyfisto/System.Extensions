using System.IO;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace System.Extensions.Test.IO
{
	[TestFixture]
	public sealed class FilesystemCompatibilityTest
		: AbstractFileTest
	{
		private SerialTaskScheduler _scheduler;
		private Filesystem _filesystem;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			_scheduler = new SerialTaskScheduler();
			_filesystem = new Filesystem(_scheduler);
		}

		[OneTimeTearDown]
		public void OneTimeTeardown()
		{
			_scheduler.Dispose();
		}

		[Test]
		[Description("Verifies that EnumerateFiles throws the same exception when given invalid paths")]
		public void TestEnumerateFiles1([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => Directory.EnumerateFiles(invalidPath)).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.EnumerateFiles(invalidPath)).ShouldThrow<ArgumentException>();
		}

		[Test]
		[Description("Verifies that EnumerateFiles throws the same exception when given invalid paths")]
		public void TestEnumerateFiles2([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => Directory.EnumerateFiles(invalidPath, "*.*")).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.EnumerateFiles(invalidPath, "*.*")).ShouldThrow<ArgumentException>();
		}

		[Test]
		[Description("Verifies that EnumerateFiles throws the same exception when given invalid paths")]
		public void TestEnumerateFiles3([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => Directory.EnumerateFiles(invalidPath, "*.*", SearchOption.AllDirectories)).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.EnumerateFiles(invalidPath, "*.*", SearchOption.AllDirectories)).ShouldThrow<ArgumentException>();
		}

		[Test]
		[Description("Verifies that both IFilesystem.EnumerateFiles and Directory.EnumerateFiles throw identical exceptions")]
		public void TestEnumerateFiles4()
		{
			new Action(() => Directory.EnumerateFiles(null)).ShouldThrow<ArgumentNullException>();
			new Action(() => Wait(_filesystem.EnumerateFiles(null))).ShouldThrow<ArgumentNullException>();
		}

		[Test]
		[Description("Verifies that both IFilesystem.EnumerateFiles and Directory.EnumerateFiles throw identical exceptions")]
		public void TestEnumerateFiles5()
		{
			const string searchPattern = "*.*";
			new Action(() => Directory.EnumerateFiles(null, searchPattern)).ShouldThrow<ArgumentNullException>();
			new Action(() => _filesystem.EnumerateFiles(null, searchPattern)).ShouldThrow<ArgumentNullException>();
		}

		[Test]
		[Description("Verifies that both IFilesystem.EnumerateFiles and Directory.EnumerateFiles throw identical exceptions")]
		public void TestEnumerateFiles6()
		{
			new Action(() => Directory.EnumerateFiles(AssemblyFilePath)).ShouldThrow<IOException>();
			new Action(() => Wait(_filesystem.EnumerateFiles(AssemblyFilePath))).ShouldThrow<IOException>();
		}

		[Test]
		[Description("Verifies that EnumerateDirectories throws the same exception when given invalid paths")]
		public void TestEnumerateDirectories1([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => Directory.EnumerateDirectories(invalidPath)).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.EnumerateDirectories(invalidPath)).ShouldThrow<ArgumentException>();
		}

		[Test]
		[Description("Verifies that EnumerateDirectories throws the same exception when given invalid paths")]
		public void TestEnumerateDirectories2([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => Directory.EnumerateDirectories(invalidPath, "*.*")).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.EnumerateDirectories(invalidPath, "*.*")).ShouldThrow<ArgumentException>();
		}

		[Test]
		[Description("Verifies that EnumerateDirectories throws the same exception when given invalid paths")]
		public void TestEnumerateDirectories3([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => Directory.EnumerateDirectories(invalidPath, "*.*", SearchOption.TopDirectoryOnly)).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.EnumerateDirectories(invalidPath, "*.*", SearchOption.TopDirectoryOnly)).ShouldThrow<ArgumentException>();
		}

		[Test]
		[Description("Verifies that GetFileInfo throws the same exception when given invalid paths")]
		public void TestGetFileInfo([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => new FileInfo(path)).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.GetFileInfo(path)).ShouldThrow<ArgumentException>();
		}

		[Test]
		[Description("Verifies that IsReadOnly throws the same exception when given invalid paths")]
		public void TestIsReadOnly([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => {var unused = new FileInfo(path).IsReadOnly;}).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.IsFileReadOnly(path)).ShouldThrow<ArgumentException>();
		}

		[Test]
		[Description("Verifies that GetDirectoryInfo throws the same exception when given invalid paths")]
		public void TestGetDirectoryInfo([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => new DirectoryInfo(path)).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.GetDirectoryInfo(path)).ShouldThrow<ArgumentException>();
		}

		[Test]
		[Description("Verifies that DeleteDirectory throws the same exception when given invalid paths")]
		public void TestDeleteDirectory1([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => Directory.Delete(invalidPath)).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.DeleteDirectory(invalidPath)).ShouldThrow<ArgumentException>();
		}

		[Test]
		[Description("Verifies that DeleteDirectory throws the same exception when given invalid paths")]
		public void TestDeleteDirectory2([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => Directory.Delete(invalidPath, false)).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.DeleteDirectory(invalidPath, false)).ShouldThrow<ArgumentException>();
		}

		[Test]
		[Description("Verifies that OpenRead throws the same exception when given invalid paths")]
		public void TestOpenRead1([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => File.OpenRead(invalidPath)).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.OpenRead(invalidPath)).ShouldThrow<ArgumentException>();
		}

		[Test]
		[Description("Verifies that CreateDirectory throws the same exception when given invalid paths")]
		public void TestCreateDirectory([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => Directory.CreateDirectory(invalidPath)).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.CreateDirectory(invalidPath)).ShouldThrow<ArgumentException>();
		}

		[Test]
		[Description("Verifies that the filesystem implementation behaves just like Directory.Exists when given invalid paths")]
		public void TestDirectoryExistsInvalidPath([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			Directory.Exists(invalidPath).Should().BeFalse();
			Wait(_filesystem.DirectoryExists(invalidPath)).Should().BeFalse();
		}

		[Test]
		[Description("Verifies that CreateFile throws the same exception when given invalid paths")]
		public void TestCreateFile([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => File.Create(invalidPath)).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.CreateFile(invalidPath)).ShouldThrow<ArgumentException>();
		}

		[Test]
		[Description("Verifies that OpenRead throws the same exception when given invalid paths")]
		public void TestOpenRead([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => File.OpenRead(invalidPath)).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.OpenRead(invalidPath)).ShouldThrow<ArgumentException>();
		}

		[Test]
		[Description("Verifies that OpenWrite throws the same exception when given invalid paths")]
		public void TestOpenWrite([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => File.OpenWrite(invalidPath)).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.OpenWrite(invalidPath)).ShouldThrow<ArgumentException>();
		}

		[Test]
		[Description("Verifies that WriteAllBytes throws the same exception when given invalid paths")]
		public void TestWriteAllBytes([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			var data = new byte[] {42};
			new Action(() => File.WriteAllBytes(invalidPath, data)).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.WriteAllBytes(invalidPath, data)).ShouldThrow<ArgumentException>();
		}

		[Test]
		[Description("Verifies that OpenWrite throws the same exception when given invalid paths")]
		public void TestWrite([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			var data = new MemoryStream();
			new Action(() => _filesystem.Write(invalidPath, data)).ShouldThrow<ArgumentException>();
		}

		[Test]
		[Description("Verifies that DeleteFile throws the same exception when given invalid paths")]
		public void TestDeleteFile([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => File.Delete(invalidPath)).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.DeleteFile(invalidPath)).ShouldThrow<ArgumentException>();
		}

		[Test]
		[Description("Verifies that the filesystem implementation behaves just like File.Exists when given invalid paths")]
		public void TestFileExists([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			File.Exists(invalidPath).Should().BeFalse();
			Wait(_filesystem.FileExists(invalidPath)).Should().BeFalse();
		}

		[Test]
		[Description("Verifies that FileLength throws the same exception when given invalid paths")]
		public void TestFileLength([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => {var unused = new FileInfo(invalidPath).Length;}).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.FileLength(invalidPath)).ShouldThrow<ArgumentException>();
		}
	}
}
