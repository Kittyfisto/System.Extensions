using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace System.Extensions.Test.IO
{
	[TestFixture]
	public abstract class AbstractFilesystemTest
		: AbstractFileTest
	{
		private IFilesystem _filesystem;

		protected abstract IFilesystem Create();

		public IFilesystem Filesystem => _filesystem;

		[SetUp]
		public void Setup()
		{
			_filesystem = Create();
		}

		[Test]
		public void TestRoots1()
		{
			var roots = Await(Filesystem.Roots);
			roots.Should().NotBeNull();
			roots.Should().NotBeEmpty();
			foreach (var root in roots)
			{
				root.Should().NotBeNull();
				root.Name.Should().NotBeNullOrEmpty();
				root.FullName.Should().NotBeNullOrEmpty();
				root.Name.Should().Be(root.FullName);
				root.Parent.Should().BeNull("because root directories don't have a parent");
				root.Root.Should().BeSameAs(root, "because root directories should point to themselves as their own root");
			}
		}

		[Test]
		public void TestTestCurrentDirectory1()
		{
			Filesystem.CurrentDirectory.Should().NotBeNull();
		}

		[Test]
		[Description("Verifies that the Current directory is linked correctly on its path towards its root")]
		public void TestTestCurrent1()
		{
			var directory = Filesystem.Current;
			directory.Should().NotBeNull();
			directory.FullName.Should().Be(Filesystem.CurrentDirectory);
			directory.Root.Should().NotBeNull("because every directory should point towards a root");
			var root = directory.Root;

			while (directory != null)
			{
				directory.Root.Should().BeSameAs(root, "because all directories in a path should point to the same root");
				directory = directory.Parent;
			}
		}

		#region Invalid Paths

		[Test]
		[Description("Verifies that the filesystem implementation behaves just like File.Exists when given invalid paths")]
		public void TestFileExistsInvalidPath([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			File.Exists(invalidPath).Should().BeFalse();
			Await(Filesystem.FileExists(invalidPath)).Should().BeFalse();
		}

		[Test]
		public void TestDirectoryExistsInvalidPath([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			Await(Filesystem.DirectoryExists(invalidPath)).Should().BeFalse();
		}

		[Test]
		public void TestGetDirectoryInfoInvalidPath([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => Filesystem.GetDirectoryInfo(path)).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestGetFileInfoInvalidPath([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => Filesystem.GetFileInfo(path)).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestCreateFileInvalidPath([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => Filesystem.CreateFile(path)).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestOpenReadInvalidPath([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => Filesystem.OpenRead(path)).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestDeleteFileInvalidPath([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => Filesystem.DeleteFile(path)).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestDeleteDirectoryInvalidPath([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => Filesystem.DeleteDirectory(path)).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestEnumerateDirectoriesInvalidPath1([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => Filesystem.EnumerateDirectories(path)).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestEnumerateDirectoriesInvalidPath2([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => Filesystem.EnumerateDirectories(path, "*.*")).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestEnumerateDirectoriesInvalidPath3([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => Filesystem.EnumerateDirectories(path, "*.*", SearchOption.TopDirectoryOnly)).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestEnumerateFilesInvalidPath1([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => Filesystem.EnumerateFiles(path)).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestEnumerateFilesInvalidPath2([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => Filesystem.EnumerateFiles(path, "*.*")).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestEnumerateFilesInvalidPath3([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => Filesystem.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly)).ShouldThrow<ArgumentException>();
		}

		#endregion

		[Test]
		[Description("Verifies that creating a directory works with a relative path")]
		public void TestCreateDirectory1()
		{
			Await(_filesystem.DirectoryExists("Foobar")).Should().BeFalse();
			var directory = Await(_filesystem.CreateDirectory("Foobar"));
			directory.Should().NotBeNull();
			directory.Name.Should().Be("Foobar");
			Await(directory.Exists).Should().BeTrue();
			Await(_filesystem.DirectoryExists("Foobar")).Should().BeTrue();
		}

		[Test]
		[Description("Verifies that creating a directory works with an absolute path")]
		public void TestCreateDirectory2()
		{
			const string directoryName = "Foobar";
			var directoryPath = Path.Combine(_filesystem.CurrentDirectory, directoryName);

			Await(_filesystem.DirectoryExists(directoryPath)).Should().BeFalse();
			var directory = Await(_filesystem.CreateDirectory(directoryPath));
			directory.Should().NotBeNull();
			directory.Name.Should().Be(directoryName);
			Await(directory.Exists).Should().BeTrue();
			Await(_filesystem.DirectoryExists(directoryPath)).Should().BeTrue();
		}

		[Test]
		[Description("Verifies that trying to create a directory a 2nd time is accepted and simply returns a reference to the directory")]
		public void TestCreateDirectory3()
		{
			var expected = Await(_filesystem.CreateDirectory("Foobar"));
			var actual = Await(_filesystem.CreateDirectory("Foobar"));

			actual.Should().NotBeNull();
			actual.FullName.Should().Be(expected.FullName);
		}

		[Test]
		[Description("Verifies that CreateDirectory is able to create a full directory path")]
		public void TestCreateDirectory4()
		{
			var path = Path.Combine(_filesystem.CurrentDirectory, "a", "b", "c");
			Await(_filesystem.CreateDirectory(path));

			Await(_filesystem.DirectoryExists(Path.Combine(_filesystem.CurrentDirectory, "a"))).Should().BeTrue();
			Await(_filesystem.DirectoryExists(Path.Combine(_filesystem.CurrentDirectory, "a", "b"))).Should().BeTrue();
			Await(_filesystem.DirectoryExists(Path.Combine(_filesystem.CurrentDirectory, "a", "b", "c"))).Should().BeTrue();
		}

		[Test]
		public void TestDirectoryCreate1()
		{
			var dir = _filesystem.GetDirectoryInfo("SomeDirectory");
			Await(dir.Exists).Should().BeFalse("because there's no such directory yet");
			Await(dir.Create());
			Await(dir.Exists).Should().BeTrue("because we've just created this directory");
			Await(_filesystem.DirectoryExists("SomeDirectory")).Should().BeTrue();
		}

		[Test]
		[Description("Verifies that deleting a non-existant directory throws")]
		public void TestDeleteDirectory1()
		{
			const string path = "foobar";
			Await(_filesystem.DirectoryExists(path)).Should().BeFalse();
			new Action(() => Await(_filesystem.DeleteDirectory(path))).ShouldThrow<DirectoryNotFoundException>();
		}

		[Test]
		[Description("Verifies that deleting a non-existant directory throws")]
		public void TestDeleteDirectory2()
		{
			const string path = "foo\\bar";
			Await(_filesystem.DirectoryExists(path)).Should().BeFalse();
			new Action(() => Await(_filesystem.DeleteDirectory(path))).ShouldThrow<DirectoryNotFoundException>();
		}

		[Test]
		[Description("Verifies that deleting a previously existing and empty directory works")]
		public void TestDeleteDirectory3()
		{
			const string path = "foobar";
			Await(_filesystem.CreateDirectory(path));
			Await(_filesystem.DirectoryExists(path)).Should().BeTrue();
			Await(_filesystem.DeleteDirectory(path));
			Await(_filesystem.DirectoryExists(path)).Should().BeFalse("because the directory should've been deleted");
		}

		[Test]
		[Description("Verifies that deleting a non-empty directory is not allowed")]
		public void TestDeleteDirectory4()
		{
			const string directory = "foobar";
			string filePath = Path.Combine(directory, "stuff.txt");
			_filesystem.CreateDirectory(directory);
			_filesystem.CreateFile(filePath);
			Await(_filesystem.FileExists(filePath)).Should().BeTrue();

			new Action(() => Await(_filesystem.DeleteDirectory(directory))).ShouldThrow<IOException>();
			Await(_filesystem.DirectoryExists(directory)).Should().BeTrue("because the directory shouldn't have been deleted");
			Await(_filesystem.FileExists(filePath)).Should().BeTrue("because the directory's contents shouldn't have been deleted");
		}

		[Test]
		[Description("Verifies that deleting a non-existant file is allowed and doesn't throw")]
		public void TestDeleteFile1()
		{
			const string filename = "some file.dat";
			Await(_filesystem.FileExists(filename)).Should().BeFalse("because there is no such file");
			Await(_filesystem.DeleteFile(filename));
			Await(_filesystem.FileExists(filename)).Should().BeFalse("because there is still no such file");
		}

		[Test]
		[Description("Verifies that deleting a file from a non-existant folder is not allowed and throws")]
		public void TestDeleteFile2()
		{
			const string filename = "foo\\bar\\file.dat";
			Await(_filesystem.FileExists(filename)).Should().BeFalse("because there is no such file");
			new Action(() => Await(_filesystem.DeleteFile(filename))).ShouldThrow<DirectoryNotFoundException>(
				"because IFilesystem implementations must mimic their .NET counterparts as far as throwing exceptions is concerned");
			Await(_filesystem.FileExists(filename)).Should().BeFalse("because there is still no such file");
		}

		[Test]
		[Description("Verifies that deleting a file actually removes it")]
		public void TestDeleteFile3()
		{
			const string filename = "some file.dat";
			using (Await(_filesystem.CreateFile(filename))) { }
			Await(_filesystem.FileExists(filename)).Should().BeTrue();

			Await(_filesystem.DeleteFile(filename));
			Await(_filesystem.FileExists(filename)).Should().BeFalse("because we've just deleted the file");
			new Action(() => Await(_filesystem.OpenRead(filename))).ShouldThrow<FileNotFoundException>();
		}

		[Test]
		[Description("Verifies that CreateSubdirectory can create single sub directory")]
		public void TestCreateSubdirectory1()
		{
			var directory = _filesystem.Current;
			var child = Await(directory.CreateSubdirectory("SomeStuff"));
			child.Should().NotBeNull();
			child.Name.Should().Be("SomeStuff");
			child.FullName.Should().Be(Path.Combine(_filesystem.CurrentDirectory, "SomeStuff"));

			Await(child.Exists).Should().BeTrue();
			Await(_filesystem.DirectoryExists(child.FullName)).Should().BeTrue();
		}

		[Test]
		[Description("Verifies that CreateSubdirectory can create multiple directories at the same time")]
		public void TestCreateSubdirectory2()
		{
			var directory = _filesystem.Current;
			var child = Await(directory.CreateSubdirectory("Some\\Stuff"));
			child.Should().NotBeNull();
			child.Name.Should().Be("Stuff");
			child.FullName.Should().Be(Path.Combine(_filesystem.CurrentDirectory, "Some\\Stuff"));

			Await(child.Exists).Should().BeTrue();
			Await(_filesystem.DirectoryExists(child.FullName)).Should().BeTrue();
		}

		[Test]
		[Description("Verifies that a created subdirectory can be found via enumeration")]
		public void TestCreateSubdirectory3()
		{
			var directory = _filesystem.Current;
			var child = Await(directory.CreateSubdirectory("Yes"));
			var childDirectories = Await(_filesystem.EnumerateDirectories(directory.FullName));
			childDirectories.Should().NotBeNull();
			childDirectories.Should().HaveCount(1);
			childDirectories[0].Should().Be(child.FullName);
		}

		[Test]
		[Description("Verifies that an empty directory can be enumerated")]
		public void TestEnumerateFiles1()
		{
			Await(_filesystem.EnumerateFiles(_filesystem.CurrentDirectory)).Should().BeEmpty();
		}

		[Test]
		[Description("Verifies that a non-existing directory cannot be enumerated")]
		public void TestEnumerateFiles2()
		{
			new Action(() => Await(_filesystem.EnumerateFiles("daawdw")))
				.ShouldThrow<DirectoryNotFoundException>();
		}

		[Test]
		[Description("Verifies that a newly created file can be found")]
		public void TestEnumerateFiles3()
		{
			Await(_filesystem.EnumerateFiles(_filesystem.CurrentDirectory)).Should().BeEmpty();
			using (Await(_filesystem.CreateFile("a"))) { }
			var files = Await(_filesystem.EnumerateFiles(_filesystem.CurrentDirectory));
			files.Should().HaveCount(1);
			var info = _filesystem.GetFileInfo(files.First());
			Await(info.Exists).Should().BeTrue("because we've just created that file");
		}

		[Test]
		[Description("Verifies that only files matching the search pattern are returned")]
		public void TestEnumerateFiles4()
		{
			Await(_filesystem.EnumerateFiles(_filesystem.CurrentDirectory)).Should().BeEmpty();
			using (Await(_filesystem.CreateFile("a"))) { }
			using (Await(_filesystem.CreateFile("b"))) { }

			Await(_filesystem.EnumerateFiles(_filesystem.CurrentDirectory, "*a")).Should().HaveCount(1);
			Await(_filesystem.EnumerateFiles(_filesystem.CurrentDirectory, "*b")).Should().HaveCount(1);
			Await(_filesystem.EnumerateFiles(_filesystem.CurrentDirectory, "*")).Should().HaveCount(2);
			Await(_filesystem.EnumerateFiles(_filesystem.CurrentDirectory, "*c")).Should().HaveCount(0);
		}

		[Test]
		public void TestFileExists1()
		{
			const string fileName = "stuff.txt";
			Await(_filesystem.FileExists(fileName)).Should().BeFalse("because we haven't created any files yet");
		}

		[Test]
		public void TestFileExists2()
		{
			const string fileName = "foobar\\stuff.txt";
			Await(_filesystem.FileExists(fileName)).Should().BeFalse("because the directory doesn't even exist");
		}

		[Test]
		public void TestCreateFile1()
		{
			const string fileName = "stuff.txt";
			Await(_filesystem.FileExists(fileName)).Should().BeFalse();

			using (var stream = Await(_filesystem.CreateFile(fileName)))
			{
				stream.Should().NotBeNull();
				stream.CanRead.Should().BeTrue("because the file should've been opened for writing");
				stream.CanWrite.Should().BeTrue("because the file should've been opened for reading");
				stream.Position.Should().Be(0, "because the stream should point towards the start of the file");
				stream.Length.Should().Be(0, "because the file should be empty since it was just created");

				Await(_filesystem.FileExists(fileName)).Should().BeTrue("because we've just created that file");
			}
		}

		[Test]
		[Description("Verifies that a previously created file is replaced")]
		public void TestCreateFile2()
		{
			const string fileName = "stuff";
			using (var stream = Await(_filesystem.CreateFile(fileName)))
			{
				stream.WriteByte(128);
			}

			using (var stream = Await(_filesystem.CreateFile(fileName)))
			{
				stream.CanRead.Should().BeTrue("because the file should've been opened for writing");
				stream.CanWrite.Should().BeTrue("because the file should've been opened for reading");
				stream.Position.Should().Be(0, "because the stream should point towards the start of the file");
				stream.Length.Should().Be(0, "because the previously created file should've been replaced with an empty file");
			}
		}

		[Test]
		[Description("Verifies that CreateFile throws when the directory doesn't exist")]
		public void TestCreateFile3()
		{
			const string fileName = "foo\\bar";
			new Action(() => Await(_filesystem.CreateFile(fileName))).ShouldThrow<DirectoryNotFoundException>();
		}

		[Test]
		[Description("Verifies that CreateFile is case insensitive")]
		public void TestCreateFile4()
		{
			const string fileName = "bar";
			using (Await(_filesystem.CreateFile(fileName.ToLower()))) { }
			using (Await(_filesystem.CreateFile(fileName.ToUpper()))) { }

			var files = Await(_filesystem.EnumerateFiles(_filesystem.CurrentDirectory));
			files.Should().HaveCount(1, "because only one file should've been created");
		}

		[Test]
		[Description("Verifies that OpenRead throws when the file doesn't exist")]
		public void TestOpenRead1()
		{
			const string fileName = "stuff";
			new Action(() => Await(_filesystem.OpenRead(fileName))).ShouldThrow<FileNotFoundException>(
				"because there's no such file");
		}

		[Test]
		[Description("Verifies that OpenWrite actually allows writing data to a file")]
		public void TestOpenWrite1()
		{
			const string fileName = "stuff";
			using (var stream = Await(_filesystem.OpenWrite(fileName)))
			{
				stream.WriteByte(128);
			}

			using (var stream = Await(_filesystem.OpenRead(fileName)))
			{
				stream.Length.Should().Be(1);
				stream.Position.Should().Be(0);
				stream.ReadByte().Should().Be(128);
			}
		}

		[Test]
		[Description("Verifies that OpenWrite actually overwrites any previously existing file")]
		public void TestOpenWrite2()
		{
			const string fileName = "stuff";
			using (var stream = Await(_filesystem.OpenWrite(fileName)))
			{
				stream.WriteByte(255);
			}

			using (var stream = Await(_filesystem.OpenWrite(fileName)))
			{
				stream.WriteByte(42);
			}

			using (var stream = Await(_filesystem.OpenRead(fileName)))
			{
				stream.Position.Should().Be(0);
				stream.Length.Should().Be(1);
				stream.ReadByte().Should().Be(42);
			}
		}
	}
}
