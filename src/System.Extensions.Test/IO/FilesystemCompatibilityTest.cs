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
		private Filesystem _filesystem;
		private DefaultTaskScheduler _taskScheduler;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			_taskScheduler = new DefaultTaskScheduler();
			_filesystem = new Filesystem(_taskScheduler);
		}

		[OneTimeTearDown]
		public void OneTimeTeardown()
		{
			_taskScheduler.Dispose();
		}

		[Test]
		[Description("Verifies that EnumerateFiles throws the same exception when given invalid paths")]
		public void TestEnumerateFiles1([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => Directory.EnumerateFiles(invalidPath)).Should().Throw<ArgumentException>();
			new Action(() => _filesystem.EnumerateFiles(invalidPath)).Should().Throw<ArgumentException>();
		}

		[Test]
		[Description("Verifies that EnumerateFiles throws the same exception when given invalid paths")]
		public void TestEnumerateFiles2([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => Directory.EnumerateFiles(invalidPath, "*.*")).Should().Throw<ArgumentException>();
			new Action(() => _filesystem.EnumerateFiles(invalidPath, "*.*")).Should().Throw<ArgumentException>();
		}

		[Test]
		[Description("Verifies that EnumerateFiles throws the same exception when given invalid paths")]
		public void TestEnumerateFiles3([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => Directory.EnumerateFiles(invalidPath, "*.*", SearchOption.AllDirectories)).Should().Throw<ArgumentException>();
			new Action(() => _filesystem.EnumerateFiles(invalidPath, "*.*", SearchOption.AllDirectories)).Should().Throw<ArgumentException>();
		}

		[Test]
		[Description("Verifies that both IFilesystem.EnumerateFiles and Directory.EnumerateFiles throw identical exceptions")]
		public void TestEnumerateFiles4()
		{
			new Action(() => Directory.EnumerateFiles(null)).Should().Throw<ArgumentNullException>();
			new Action(() => _filesystem.EnumerateFiles(null)).Should().Throw<ArgumentNullException>();
		}

		[Test]
		[Description("Verifies that both IFilesystem.EnumerateFiles and Directory.EnumerateFiles throw identical exceptions")]
		public void TestEnumerateFiles5()
		{
			const string searchPattern = "*.*";
			new Action(() => Directory.EnumerateFiles(null, searchPattern)).Should().Throw<ArgumentNullException>();
			new Action(() => _filesystem.EnumerateFiles(null, searchPattern)).Should().Throw<ArgumentNullException>();
		}

		[Test]
		[Description("Verifies that both IFilesystem.EnumerateFiles and Directory.EnumerateFiles throw identical exceptions")]
		public void TestEnumerateFiles6()
		{
			new Action(() => Directory.EnumerateFiles(AssemblyFilePath)).Should().Throw<IOException>();
			new Action(() => _filesystem.EnumerateFiles(AssemblyFilePath)).Should().Throw<IOException>();
		}

		[Test]
		[Description("Verifies that EnumerateDirectories throws the same exception when given invalid paths")]
		public void TestEnumerateDirectories1([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => Directory.EnumerateDirectories(invalidPath)).Should().Throw<ArgumentException>();
			new Action(() => _filesystem.EnumerateDirectories(invalidPath)).Should().Throw<ArgumentException>();
		}

		[Test]
		[Description("Verifies that EnumerateDirectories throws the same exception when given invalid paths")]
		public void TestEnumerateDirectories2([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => Directory.EnumerateDirectories(invalidPath, "*.*")).Should().Throw<ArgumentException>();
			new Action(() => _filesystem.EnumerateDirectories(invalidPath, "*.*")).Should().Throw<ArgumentException>();
		}

		[Test]
		[Description("Verifies that EnumerateDirectories throws the same exception when given invalid paths")]
		public void TestEnumerateDirectories3([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => Directory.EnumerateDirectories(invalidPath, "*.*", SearchOption.TopDirectoryOnly)).Should().Throw<ArgumentException>();
			new Action(() => _filesystem.EnumerateDirectories(invalidPath, "*.*", SearchOption.TopDirectoryOnly)).Should().Throw<ArgumentException>();
		}

		[Test]
		[Description("Verifies that GetFileInfo throws the same exception when given invalid paths")]
		public void TestGetFileInfo([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => new FileInfo(path)).Should().Throw<ArgumentException>();
			new Action(() => _filesystem.GetFileInfo(path)).Should().Throw<ArgumentException>();
		}

		[Test]
		[Description("Verifies that IsReadOnly throws the same exception when given invalid paths")]
		public void TestIsReadOnly([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => {var unused = new FileInfo(path).IsReadOnly;}).Should().Throw<ArgumentException>();
			new Action(() => _filesystem.IsFileReadOnly(path)).Should().Throw<ArgumentException>();
		}

		[Test]
		[Description("Verifies that GetDirectoryInfo throws the same exception when given invalid paths")]
		public void TestGetDirectoryInfo([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => new DirectoryInfo(path)).Should().Throw<ArgumentException>();
			new Action(() => _filesystem.GetDirectoryInfo(path)).Should().Throw<ArgumentException>();
		}

		[Test]
		[Description("Verifies that DeleteDirectory throws the same exception when given invalid paths")]
		public void TestDeleteDirectory1([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => Directory.Delete(invalidPath)).Should().Throw<ArgumentException>();
			new Action(() => _filesystem.DeleteDirectory(invalidPath)).Should().Throw<ArgumentException>();
		}

		[Test]
		[Description("Verifies that DeleteDirectory throws the same exception when given invalid paths")]
		public void TestDeleteDirectory2([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => Directory.Delete(invalidPath, false)).Should().Throw<ArgumentException>();
			new Action(() => _filesystem.DeleteDirectory(invalidPath, false)).Should().Throw<ArgumentException>();
		}

		[Test]
		[Description("Verifies that OpenRead throws the same exception when given invalid paths")]
		public void TestOpenRead1([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => File.OpenRead(invalidPath)).Should().Throw<ArgumentException>();
			new Action(() => _filesystem.OpenRead(invalidPath)).Should().Throw<ArgumentException>();
		}

		[Test]
		[Description("Verifies that CreateDirectory throws the same exception when given invalid paths")]
		public void TestCreateDirectory([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => Directory.CreateDirectory(invalidPath)).Should().Throw<ArgumentException>();
			new Action(() => _filesystem.CreateDirectory(invalidPath)).Should().Throw<ArgumentException>();
		}

		[Test]
		[Description("Verifies that the filesystem implementation behaves just like Directory.Exists when given invalid paths")]
		public void TestDirectoryExistsInvalidPath([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			Directory.Exists(invalidPath).Should().BeFalse();
			_filesystem.DirectoryExists(invalidPath).Should().BeFalse();
		}

		[Test]
		[Description("Verifies that CreateFile throws the same exception when given invalid paths")]
		public void TestCreateFile([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => File.Create(invalidPath)).Should().Throw<ArgumentException>();
			new Action(() => _filesystem.CreateFile(invalidPath)).Should().Throw<ArgumentException>();
		}

		[Test]
		[Description("Verifies that OpenRead throws the same exception when given invalid paths")]
		public void TestOpenRead([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => File.OpenRead(invalidPath)).Should().Throw<ArgumentException>();
			new Action(() => _filesystem.OpenRead(invalidPath)).Should().Throw<ArgumentException>();
		}

		[Test]
		[Description("Verifies that OpenWrite throws the same exception when given invalid paths")]
		public void TestOpenWrite([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => File.OpenWrite(invalidPath)).Should().Throw<ArgumentException>();
			new Action(() => _filesystem.OpenWrite(invalidPath)).Should().Throw<ArgumentException>();
		}

		[Test]
		[Description("Verifies that WriteAllBytes throws the same exception when given invalid paths")]
		public void TestWriteAllBytes([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			var data = new byte[] {42};
			new Action(() => File.WriteAllBytes(invalidPath, data)).Should().Throw<ArgumentException>();
			new Action(() => _filesystem.WriteAllBytes(invalidPath, data)).Should().Throw<ArgumentException>();
		}

		[Test]
		[Description("Verifies that OpenWrite throws the same exception when given invalid paths")]
		public void TestWrite([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			var data = new MemoryStream();
			new Action(() => _filesystem.Write(invalidPath, data)).Should().Throw<ArgumentException>();
		}

		[Test]
		[Description("Verifies that DeleteFile throws the same exception when given invalid paths")]
		public void TestDeleteFile([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => File.Delete(invalidPath)).Should().Throw<ArgumentException>();
			new Action(() => _filesystem.DeleteFile(invalidPath)).Should().Throw<ArgumentException>();
		}

		[Test]
		[Description("Verifies that the filesystem implementation behaves just like File.Exists when given invalid paths")]
		public void TestFileExists([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			File.Exists(invalidPath).Should().BeFalse();
			_filesystem.FileExists(invalidPath).Should().BeFalse();
		}

		[Test]
		[Description("Verifies that FileLength throws the same exception when given invalid paths")]
		public void TestFileLength([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => {var unused = new FileInfo(invalidPath).Length;}).Should().Throw<ArgumentException>();
			new Action(() => _filesystem.FileLength(invalidPath)).Should().Throw<ArgumentException>();
		}
	}
}
