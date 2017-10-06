using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace System.Extensions.Test.IO
{
	[TestFixture]
	public class FilesystemTest
		: AbstractFilesystemTest
	{
		private SerialTaskScheduler _scheduler;
		private string _testclassPath;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			_testclassPath = Path.Combine(Path.GetTempPath(), "FilesystemTest");
			_scheduler = new SerialTaskScheduler();
		}

		[SetUp]
		public new void Setup()
		{
			var testDirectory = Path.Combine(_testclassPath, TestContext.CurrentContext.Test.ID);
			if (Directory.Exists(testDirectory))
				Directory.Delete(testDirectory, true);
			Directory.CreateDirectory(testDirectory);
			Directory.SetCurrentDirectory(testDirectory);
			Console.WriteLine("Directory: {0}", testDirectory);
		}

		[OneTimeTearDown]
		public void OneTimeTeardown()
		{
			_scheduler.Dispose();
		}

		[Test]
		public void TestRoots2()
		{
			var actualRoots = DriveInfo.GetDrives().Select(x => new DirectoryInfo(x.Name)).ToList();
			var roots = Await(Filesystem.Roots);

			roots.Should().HaveCount(actualRoots.Count);
			foreach (var root in roots)
			{
				var actual = actualRoots.FirstOrDefault(x => Equals(x.Name, root.Name));
				actual.Should().NotBeNull();
				root.FullName.Should().Be(actual.FullName);
			}
		}

		[Test]
		public void TestFileExists2()
		{
			Await(Filesystem.FileExists("wdawaddwawadoknfawonafw"))
				.Should().BeFalse("because the file doesn't exist");
		}

		[Test]
		public void TestFileExists3()
		{
			Await(Filesystem.FileExists(AssemblyFilePath))
				.Should().BeTrue("because that assembly most certainly exists");
		}

		[Test]
		public void TestDirectoryExists1()
		{
			Await(Filesystem.DirectoryExists("dawwadwadadwawd"))
				.Should().BeFalse("because the directory doesn't exist");
		}

		[Test]
		public void TestDirectoryExists2()
		{
			Await(Filesystem.DirectoryExists(AssemblyDirectory))
				.Should().BeTrue("because that directory most certainly exists");
		}

		[Test]
		public void TestEnumerateFiles4()
		{
			var expected = Directory.EnumerateFiles(AssemblyDirectory).ToList();
			var actual = Await(Filesystem.EnumerateFiles(AssemblyDirectory));

			Console.WriteLine("Found {0} files", actual.Count);
			actual.Should().NotBeNull();
			actual.Should().BeEquivalentTo(expected);
		}

		[Test]
		public void TestEnumerateFiles5()
		{
			const string filter = "*.pdb";
			var expected = Directory.EnumerateFiles(AssemblyDirectory, filter).ToList();
			var actual = Await(Filesystem.EnumerateFiles(AssemblyDirectory, filter));

			Console.WriteLine("Found {0} files", actual.Count);
			actual.Should().NotBeNull();
			actual.Should().BeEquivalentTo(expected);
		}

		[Test]
		public void TestGetFileInfo1()
		{
			var expected = new FileInfo(AssemblyFilePath);
			var actual = Filesystem.GetFileInfo(AssemblyFilePath);
			actual.Should().NotBeNull();
			actual.Name.Should().Be(expected.Name);
			actual.FullPath.Should().Be(AssemblyFilePath);
			Await(actual.Length).Should().Be(expected.Length, "because both methods should find the same file size");
			Await(actual.Exists).Should().BeTrue("because the file most certainly exists");
			Await(actual.IsReadOnly).Should().Be(expected.IsReadOnly, "because both methods should find the same attribute");
		}

		[Test]
		public void TestGetFileInfo3()
		{
			var expected = new FileInfo(AssemblyFilePath);
			var actual = Await(Filesystem.GetFileInfo(AssemblyFilePath).Capture());
			actual.Should().NotBeNull();
			actual.Name.Should().Be(expected.Name);
			actual.FullPath.Should().Be(AssemblyFilePath);
			actual.Length.Should().Be(expected.Length, "because both methods should find the same file size");
			actual.Exists.Should().BeTrue("because the file most certainly exists");
			actual.IsReadOnly.Should().Be(expected.IsReadOnly, "because both methods should find the same attribute");
			actual.Directory.Should().NotBeNull();
			actual.DirectoryName.Should().Be(expected.DirectoryName);
		}

		[Test]
		public void TestGetDirectoryInfo1()
		{
			var info = Filesystem.GetDirectoryInfo(AssemblyDirectory);

			info.Should().NotBeNull();
			info.Name.Should().Be(new DirectoryInfo(AssemblyDirectory).Name);
			info.FullName.Should().Be(AssemblyDirectory);
			Await(info.Exists).Should().BeTrue("because the folder most certainly exists");
		}

		[Test]
		public void TestGetDirectoryInfo2()
		{
			var info = Filesystem.GetDirectoryInfo(AssemblyDirectory);
			var expected = Await(Filesystem.EnumerateFiles(AssemblyDirectory));
			var actual = Await(info.EnumerateFiles());

			foreach (var fileInfo in actual)
			{
				fileInfo.Should().NotBeNull();
				expected.Should().Contain(fileInfo.FullPath);
				Await(fileInfo.Exists).Should().BeTrue();
			}
		}

		[Test]
		public void TestGetDirectoryInfo3()
		{
			var info = Filesystem.GetDirectoryInfo(AssemblyDirectory);
			var expected = Await(Filesystem.EnumerateFiles(AssemblyDirectory, "*.dll"));
			var actual = Await(info.EnumerateFiles("*.dll"));

			foreach (var fileInfo in actual)
			{
				fileInfo.Should().NotBeNull();
				expected.Should().Contain(fileInfo.FullPath);
				Await(fileInfo.Exists).Should().BeTrue();
			}
		}

		[Test]
		public void TestGetDirectoryInfo4([Values(SearchOption.AllDirectories, SearchOption.TopDirectoryOnly)] SearchOption searchOption)
		{
			var info = Filesystem.GetDirectoryInfo(AssemblyDirectory);
			var expected = Await(Filesystem.EnumerateFiles(AssemblyDirectory, "*.dll", searchOption));
			var actual = Await(info.EnumerateFiles("*.dll", searchOption));

			foreach (var fileInfo in actual)
			{
				fileInfo.Should().NotBeNull();
				expected.Should().Contain(fileInfo.FullPath);
				Await(fileInfo.Exists).Should().BeTrue();
			}
		}

		[Test]
		public void TestGetDirectoryInfo5()
		{
			var actual = new DirectoryInfo(AssemblyDirectory);
			var info = Await(Filesystem.GetDirectoryInfo(AssemblyDirectory).Capture());
			info.Name.Should().Be(actual.Name);
			info.FullName.Should().Be(actual.FullName);
			info.Exists.Should().Be(actual.Exists);
			info.Parent.Should().NotBeNull();
			info.Root.Should().NotBeNull();
		}

		[Test]
		[Description("Verifies that a directory snapshot is linked correctly")]
		public void TestGetDirectoryInfo6()
		{
			var info = Await(Filesystem.GetDirectoryInfo(AssemblyDirectory).Capture());
			var root = info.Root;

			var dir = info;
			while (dir != null)
			{
				dir.Root.Should().BeSameAs(root, "because all directories should point to the same root object");
				dir = dir.Parent;
			}
		}

		[Test]
		[Ignore("Broke it again")]
		[Description("Verifies that name and fullname behave identical to their DirectoryInfo counterparts")]
		public void TestGetDirectoryInfo7()
		{
			var path1 = AssemblyDirectory;
			var path2 = AssemblyDirectory + "\\";

			var expectedInfo1 = new DirectoryInfo(path1);
			var actualInfo1 = Filesystem.GetDirectoryInfo(path1);
			actualInfo1.Name.Should().Be(expectedInfo1.Name);
			actualInfo1.FullName.Should().Be(expectedInfo1.FullName);

			var expectedInfo2 = new DirectoryInfo(path2);
			var actualInfo2 = Filesystem.GetDirectoryInfo(path2);
			actualInfo2.Name.Should().Be(expectedInfo2.Name);
			actualInfo2.FullName.Should().Be(expectedInfo2.FullName);
		}

		[Test]
		[Description("Verifies that Create() actually creates a directory on the filesystem")]
		public void TestCreateDirectory10()
		{
			const string directoryName = "Stuff";
			var actualDirectory = Filesystem.GetDirectoryInfo(directoryName);
			Directory.Exists(directoryName).Should().BeFalse("because the directory does not exist yet");

			Await(actualDirectory.Create());
			Directory.Exists(directoryName).Should().BeTrue("because we've just created this directory");
		}

		[Test]
		[Description("Verifies that Create() actually creates a file on the filesystem")]
		public void TestCreateFile10()
		{
			const string fileName = "Stuff";
			var file = Filesystem.GetFileInfo(fileName);
			File.Exists(fileName).Should().BeFalse("because the file does not exist yet");

			using (Await(file.Create()))
			{
				File.Exists(fileName).Should().BeTrue("because we've just created this file");
			}
		}

		[Test]
		[Description("Verifies that Create() actually creates a file on the filesystem")]
		public void TestCreateFile11()
		{
			const string fileName = "Stuff";
			var file = Filesystem.GetFileInfo(fileName);

			using (var stream = Await(file.Create()))
			{
				stream.WriteByte(42);
			}

			File.ReadAllBytes(fileName).Should().Equal(new object[] {(byte) 42});
		}

		protected override IFilesystem Create()
		{
			return new Filesystem(_scheduler);
		}
	}
}