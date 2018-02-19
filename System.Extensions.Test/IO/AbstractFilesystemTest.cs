using System.Collections.Generic;
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

		public static IEnumerable<SearchOption> SearchOptions => new[] {SearchOption.AllDirectories, SearchOption.TopDirectoryOnly};

		[SetUp]
		public void Setup()
		{
			_filesystem = Create();
		}

		[Test]
		public void TestRoots1()
		{
			var roots = Wait(Filesystem.Roots);
			roots.Should().NotBeNull();
			roots.Should().NotBeEmpty();
			foreach (var root in roots)
			{
				root.Should().NotBeNull();
				root.Name.Should().NotBeNullOrEmpty();
				root.FullName.Should().NotBeNullOrEmpty();
				root.Name.Should().Be(root.FullName);
				root.Parent.Should().BeNull("because root directories don't have a parent");
				root.Root.Should().Be(root, "because root directories should point to themselves as their own root");
			}
		}

		[Test]
		public void TestDirectoryToString()
		{
			var path = Path.Combine(_filesystem.CurrentDirectory, "Foo", "Bar");
			var directory = _filesystem.GetDirectoryInfo(path);
			directory.ToString().Should().Be("{" + path + "}");
		}
		
		[Test]
		public void TestFileToString()
		{
			var path = Path.Combine(_filesystem.CurrentDirectory, "Foo", "Bar");
			var directory = _filesystem.GetFileInfo(path);
			directory.ToString().Should().Be("{" + path + "}");
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
				directory.Root.Should().Be(root, "because all directories in a path should point to the same root");
				directory = directory.Parent;
			}
		}

		#region Invalid Paths

		[Test]
		[Description("Verifies that the filesystem implementation behaves just like File.Exists when given invalid paths")]
		public void TestFileExistsInvalidPath([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			File.Exists(invalidPath).Should().BeFalse();
			Wait(Filesystem.FileExists(invalidPath)).Should().BeFalse();
		}

		[Test]
		public void TestDirectoryExistsInvalidPath([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			Wait(Filesystem.DirectoryExists(invalidPath)).Should().BeFalse();
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
			Wait(_filesystem.DirectoryExists("Foobar")).Should().BeFalse();
			var directory = Wait(_filesystem.CreateDirectory("Foobar"));
			directory.Should().NotBeNull();
			directory.Name.Should().Be("Foobar");
			Wait(directory.Exists).Should().BeTrue();
			Wait(_filesystem.DirectoryExists("Foobar")).Should().BeTrue();
		}

		[Test]
		[Description("Verifies that creating a directory works with an absolute path")]
		public void TestCreateDirectory2()
		{
			const string directoryName = "Foobar";
			var directoryPath = Path.Combine(_filesystem.CurrentDirectory, directoryName);

			Wait(_filesystem.DirectoryExists(directoryPath)).Should().BeFalse();
			var directory = Wait(_filesystem.CreateDirectory(directoryPath));
			directory.Should().NotBeNull();
			directory.Name.Should().Be(directoryName);
			Wait(directory.Exists).Should().BeTrue();
			Wait(_filesystem.DirectoryExists(directoryPath)).Should().BeTrue();
		}

		[Test]
		[Description("Verifies that trying to create a directory a 2nd time is accepted and simply returns a reference to the directory")]
		public void TestCreateDirectory3()
		{
			var expected = Wait(_filesystem.CreateDirectory("Foobar"));
			var actual = Wait(_filesystem.CreateDirectory("Foobar"));

			actual.Should().NotBeNull();
			actual.FullName.Should().Be(expected.FullName);
		}

		[Test]
		[Description("Verifies that CreateDirectory is able to create a full directory path")]
		public void TestCreateDirectory4()
		{
			var path = Path.Combine(_filesystem.CurrentDirectory, "a", "b", "c");
			Wait(_filesystem.CreateDirectory(path));

			Wait(_filesystem.DirectoryExists(Path.Combine(_filesystem.CurrentDirectory, "a"))).Should().BeTrue();
			Wait(_filesystem.DirectoryExists(Path.Combine(_filesystem.CurrentDirectory, "a", "b"))).Should().BeTrue();
			Wait(_filesystem.DirectoryExists(Path.Combine(_filesystem.CurrentDirectory, "a", "b", "c"))).Should().BeTrue();
		}

		[Test]
		[SetCulture("en-US")]
		[Description("Verifies that creating a directory for a non-existant root doesn't work")]
		public void TestCreateDirectory5()
		{
			const string directory = "Z:\\foo\\bar";
			new Action(() => Wait(_filesystem.CreateDirectory(directory)))
				.ShouldThrow<DirectoryNotFoundException>()
				.WithMessage("Could not find a part of the path 'Z:\\foo\\bar'.");
		}

		[Test]
		public void TestDirectoryCreate1()
		{
			var dir = _filesystem.GetDirectoryInfo("SomeDirectory");
			Wait(dir.Exists).Should().BeFalse("because there's no such directory yet");
			Wait(dir.Create());
			Wait(dir.Exists).Should().BeTrue("because we've just created this directory");
			Wait(_filesystem.DirectoryExists("SomeDirectory")).Should().BeTrue();
		}

		[Test]
		[Description("Verifies that deleting a non-existant directory throws")]
		public void TestDeleteDirectory1()
		{
			const string path = "foobar";
			Wait(_filesystem.DirectoryExists(path)).Should().BeFalse();
			new Action(() => Wait(_filesystem.DeleteDirectory(path))).ShouldThrow<DirectoryNotFoundException>();
		}

		[Test]
		[Description("Verifies that deleting a non-existant directory throws")]
		public void TestDeleteDirectory2()
		{
			const string path = "foo\\bar";
			Wait(_filesystem.DirectoryExists(path)).Should().BeFalse();
			new Action(() => Wait(_filesystem.DeleteDirectory(path))).ShouldThrow<DirectoryNotFoundException>();
		}

		[Test]
		[Description("Verifies that deleting a previously existing and empty directory works")]
		public void TestDeleteDirectory3()
		{
			const string path = "foobar";
			Wait(_filesystem.CreateDirectory(path));
			Wait(_filesystem.DirectoryExists(path)).Should().BeTrue();
			Wait(_filesystem.DeleteDirectory(path));
			Wait(_filesystem.DirectoryExists(path)).Should().BeFalse("because the directory should've been deleted");
		}

		[Test]
		[Description("Verifies that deleting a non-empty directory is not allowed")]
		public void TestDeleteDirectory4()
		{
			const string directory = "foobar";
			string filePath = Path.Combine(directory, "stuff.txt");
			_filesystem.CreateDirectory(directory);
			_filesystem.CreateFile(filePath);
			Wait(_filesystem.FileExists(filePath)).Should().BeTrue();

			new Action(() => Wait(_filesystem.DeleteDirectory(directory))).ShouldThrow<IOException>();
			new Action(() => Wait(_filesystem.GetDirectoryInfo(directory).Delete())).ShouldThrow<IOException>();

			Wait(_filesystem.DirectoryExists(directory)).Should().BeTrue("because the directory shouldn't have been deleted");
			Wait(_filesystem.FileExists(filePath)).Should().BeTrue("because the directory's contents shouldn't have been deleted");
		}

		[Test]
		[Description("Verifies that deleting a directory including subdirectories is possible")]
		public void TestDeleteyDirectory5()
		{
			const string directory = "A";
			_filesystem.CreateDirectory(directory);
			var subDirectory = Path.Combine(directory, "B");
			_filesystem.CreateDirectory(subDirectory);

			var directoryInfo = _filesystem.GetDirectoryInfo(directory);
			var subDirectoryInfo = _filesystem.GetDirectoryInfo(subDirectory);

			Wait(directoryInfo.Exists).Should().BeTrue();
			Wait(subDirectoryInfo.Exists).Should().BeTrue();

			Wait(_filesystem.DeleteDirectory(directory, true));
			Wait(directoryInfo.Exists).Should().BeFalse();
			Wait(subDirectoryInfo.Exists).Should().BeFalse();
		}

		[Test]
		public void TestGetDirectoryCaseInsensitive()
		{
			var actual = Wait(_filesystem.CreateDirectory("FoO"));

			const string reason = "because we should retrieve an equal IDirectoryInfo object, regardless of the case";
			_filesystem.GetDirectoryInfo("foo").Should().Be(actual, reason);
			_filesystem.GetDirectoryInfo("FOO").Should().Be(actual, reason);
			_filesystem.GetDirectoryInfo("fOo").Should().Be(actual, reason);
		}
		
		[Test]
		public void TestGetFileCaseInsensitive()
		{
			var actual = _filesystem.GetFileInfo("FoO");
			using (Wait(_filesystem.CreateFile("FoO")))
			{ }

			const string reason = "because we should retrieve an equal IDirectoryInfo object, regardless of the case";
			_filesystem.GetFileInfo("foo").Should().Be(actual, reason);
			_filesystem.GetFileInfo("FOO").Should().Be(actual, reason);
			_filesystem.GetFileInfo("fOo").Should().Be(actual, reason);
		}

		[Test]
		[Description("Verifies that deleting a non-existant file is allowed and doesn't throw")]
		public void TestDeleteFile1()
		{
			const string filename = "some file.dat";
			Wait(_filesystem.FileExists(filename)).Should().BeFalse("because there is no such file");
			Wait(_filesystem.DeleteFile(filename));
			Wait(_filesystem.FileExists(filename)).Should().BeFalse("because there is still no such file");
		}

		[Test]
		[Description("Verifies that deleting a file from a non-existant folder is not allowed and throws")]
		public void TestDeleteFile2()
		{
			const string filename = "foo\\bar\\file.dat";
			Wait(_filesystem.FileExists(filename)).Should().BeFalse("because there is no such file");
			new Action(() => Wait(_filesystem.DeleteFile(filename))).ShouldThrow<DirectoryNotFoundException>(
				"because IFilesystem implementations must mimic their .NET counterparts as far as throwing exceptions is concerned");
			Wait(_filesystem.FileExists(filename)).Should().BeFalse("because there is still no such file");
		}

		[Test]
		[Description("Verifies that deleting a file actually removes it")]
		public void TestDeleteFile3()
		{
			const string filename = "some file.dat";
			using (Wait(_filesystem.CreateFile(filename))) { }
			Wait(_filesystem.FileExists(filename)).Should().BeTrue();

			Wait(_filesystem.DeleteFile(filename));
			Wait(_filesystem.FileExists(filename)).Should().BeFalse("because we've just deleted the file");
			new Action(() => Wait(_filesystem.OpenRead(filename))).ShouldThrow<FileNotFoundException>();
		}

		[Test]
		[Description("Verifies that CreateSubdirectory can create single sub directory")]
		public void TestCreateSubdirectory1()
		{
			var directory = _filesystem.Current;
			var child = Wait(directory.CreateSubdirectory("SomeStuff"));
			child.Should().NotBeNull();
			child.Name.Should().Be("SomeStuff");
			child.FullName.Should().Be(Path.Combine(_filesystem.CurrentDirectory, "SomeStuff"));

			Wait(child.Exists).Should().BeTrue();
			Wait(_filesystem.DirectoryExists(child.FullName)).Should().BeTrue();
		}

		[Test]
		[Description("Verifies that CreateSubdirectory can create multiple directories at the same time")]
		public void TestCreateSubdirectory2()
		{
			var directory = _filesystem.Current;
			var child = Wait(directory.CreateSubdirectory("Some\\Stuff"));
			child.Should().NotBeNull();
			child.Name.Should().Be("Stuff");
			child.FullName.Should().Be(Path.Combine(_filesystem.CurrentDirectory, "Some\\Stuff"));

			Wait(child.Exists).Should().BeTrue();
			Wait(_filesystem.DirectoryExists(child.FullName)).Should().BeTrue();
		}

		[Test]
		[Description("Verifies that a created subdirectory can be found via enumeration")]
		public void TestCreateSubdirectory3()
		{
			var directory = _filesystem.Current;
			var child = Wait(directory.CreateSubdirectory("Yes"));
			var childDirectories = Wait(_filesystem.EnumerateDirectories(directory.FullName));
			childDirectories.Should().NotBeNull();
			childDirectories.Should().HaveCount(1);
			childDirectories[0].Should().Be(child.FullName);
		}

		[Test]
		[Description("")]
		public void TestEnumerateDirectories1()
		{
			var path = _filesystem.CurrentDirectory;
			Wait(_filesystem.EnumerateDirectories(path)).Should().BeEmpty("because we haven't created any additional directories");
		}

		[Test]
		[Description("")]
		public void TestEnumerateDirectories2()
		{
			var path = _filesystem.CurrentDirectory;
			Wait(_filesystem.EnumerateDirectories(path, "*")).Should().BeEmpty("because we haven't created any additional directories");
		}

		[Test]
		[Description("")]
		public void TestEnumerateDirectories3([ValueSource(nameof(SearchOptions))] SearchOption searchOption)
		{
			var path = _filesystem.CurrentDirectory;
			Wait(_filesystem.EnumerateDirectories(path, "*", searchOption)).Should().BeEmpty("because we haven't created any additional directories");
		}

		[Test]
		[Description("Verifies that an empty directory can be enumerated")]
		public void TestEnumerateFiles1()
		{
			Wait(_filesystem.EnumerateFiles(_filesystem.CurrentDirectory)).Should().BeEmpty();
		}
		
		[Test]
		[Description("Verifies that an empty directory can be enumerated")]
		public void TestEnumerateFiles2()
		{
			Wait(_filesystem.Current.EnumerateFiles()).Should().BeEmpty();
		}

		[Test]
		[Description("Verifies that a non-existing directory cannot be enumerated")]
		public void TestEnumerateFiles3()
		{
			new Action(() => Wait(_filesystem.EnumerateFiles("daawdw"))).ShouldThrow<DirectoryNotFoundException>();
		}
		
		[Test]
		[Description("Verifies that a non-existing directory cannot be enumerated")]
		public void TestEnumerateFiles4()
		{
			new Action(() => Wait(_filesystem.GetDirectoryInfo("daawdw").EnumerateFiles())).ShouldThrow<DirectoryNotFoundException>();
		}

		[Test]
		[Description("Verifies that a newly created file can be found")]
		public void TestEnumerateFiles5()
		{
			Wait(_filesystem.EnumerateFiles(_filesystem.CurrentDirectory)).Should().BeEmpty();
			using (Wait(_filesystem.CreateFile("a"))) { }
			var files = Wait(_filesystem.EnumerateFiles(_filesystem.CurrentDirectory));
			files.Should().HaveCount(1);
			var info = _filesystem.GetFileInfo(files.First());
			Wait(info.Exists).Should().BeTrue("because we've just created that file");
		}

		[Test]
		[Description("Verifies that only files matching the search pattern are returned")]
		public void TestEnumerateFiles6()
		{
			Wait(_filesystem.EnumerateFiles(_filesystem.CurrentDirectory)).Should().BeEmpty();
			using (Wait(_filesystem.CreateFile("a"))) { }
			using (Wait(_filesystem.CreateFile("b"))) { }

			Wait(_filesystem.EnumerateFiles(_filesystem.CurrentDirectory, "*a")).Should().HaveCount(1);
			Wait(_filesystem.EnumerateFiles(_filesystem.CurrentDirectory, "*b")).Should().HaveCount(1);
			Wait(_filesystem.EnumerateFiles(_filesystem.CurrentDirectory, "*")).Should().HaveCount(2);
			Wait(_filesystem.EnumerateFiles(_filesystem.CurrentDirectory, "*c")).Should().HaveCount(0);
		}
		
		[Test]
		[Description("Verifies that only files matching the search pattern are returned")]
		public void TestEnumerateFiles7()
		{
			Wait(_filesystem.EnumerateFiles(_filesystem.CurrentDirectory)).Should().BeEmpty();
			using (Wait(_filesystem.CreateFile("a"))) { }
			using (Wait(_filesystem.CreateFile("b"))) { }

			var directory = _filesystem.Current;
			Wait(directory.EnumerateFiles("*a")).Should().HaveCount(1);
			Wait(directory.EnumerateFiles("*b")).Should().HaveCount(1);
			Wait(directory.EnumerateFiles("*")).Should().HaveCount(2);
			Wait(directory.EnumerateFiles("*c")).Should().HaveCount(0);
		}

		[Test]
		public void TestEnumerateFilesAllDirectories1()
		{
			_filesystem.CreateDirectory("A");
			_filesystem.CreateDirectory("B");
			_filesystem.CreateFile("A\\a.txt");
			_filesystem.CreateFile("B\\b.txt");

			var files = Wait(_filesystem.EnumerateFiles(_filesystem.CurrentDirectory, "*", SearchOption.AllDirectories));
			files.Should().HaveCount(2);
			files.Should().BeEquivalentTo(new object[]
			{
				Path.Combine(_filesystem.CurrentDirectory, "A\\a.txt"),
				Path.Combine(_filesystem.CurrentDirectory, "B\\b.txt")
			});
		}

		[Test]
		public void TestEnumerateFilesAllDirectories2()
		{
			_filesystem.CreateDirectory("A");
			_filesystem.CreateDirectory("B");
			_filesystem.CreateFile("A\\a.txt");
			_filesystem.CreateFile("B\\b.txt");

			var directory = _filesystem.Current;
			var files = Wait(directory.EnumerateFiles("*", SearchOption.AllDirectories));
			files.Should().HaveCount(2);
			var file = files.FirstOrDefault(x => x.Name.EndsWith("a.txt"));
			file.Should().NotBeNull();
			file.FullPath.Should().Be(Path.Combine(_filesystem.CurrentDirectory, "A\\a.txt"));
			file = files.FirstOrDefault(x => x.Name.EndsWith("b.txt"));
			file.Should().NotBeNull();
			file.FullPath.Should().Be(Path.Combine(_filesystem.CurrentDirectory, "B\\b.txt"));
		}

		[Test]
		public void TestEnumerateFilesAllDirectories3()
		{
			_filesystem.CreateDirectory("A");
			_filesystem.CreateDirectory("B");
			_filesystem.CreateFile("A\\a.txt");
			_filesystem.CreateFile("B\\b.txt");

			var files = Wait(_filesystem.EnumerateFiles("B", "*", SearchOption.AllDirectories));
			files.Should().HaveCount(1);
			files.Should().BeEquivalentTo(new object[]
			{
				Path.Combine(_filesystem.CurrentDirectory, "B\\b.txt")
			});
		}
		
		[Test]
		public void TestEnumerateFilesAllDirectories4()
		{
			_filesystem.CreateDirectory("A");
			_filesystem.CreateDirectory("B");
			_filesystem.CreateFile("A\\a.txt");
			_filesystem.CreateFile("B\\b.txt");
			
			var directory = _filesystem.GetDirectoryInfo("B");
			var files = Wait(directory.EnumerateFiles("*", SearchOption.AllDirectories));
			files.Should().HaveCount(1);
			files.First().FullPath.Should().Be(Path.Combine(_filesystem.CurrentDirectory, "B\\b.txt"));
		}

		[Test]
		public void TestFileCreate1()
		{
			var file = _filesystem.GetFileInfo("foo.txt");
			Wait(file.Exists).Should().BeFalse();
			using (var stream = Wait(file.Create()))
			{
				stream.WriteByte(42);
			}
			Wait(file.Exists).Should().BeTrue();
		}

		[Test]
		[Description("Verifies that deleting a non existing file is allowed and essentially a NOP")]
		public void TestFileDelete1()
		{
			var file = _filesystem.GetFileInfo("foo.txt");
			new Action(() => Wait(file.Delete())).ShouldNotThrow();
		}

		[Test]
		[Description("Verifies that deleting a newly created file is possible")]
		public void TestFileDelete2()
		{
			var file = _filesystem.GetFileInfo("foo.txt");
			using (Wait(file.Create()))
			{ }
			Wait(file.Exists).Should().BeTrue();

			Wait(file.Delete());
			Wait(file.Exists).Should().BeFalse();
			Wait(_filesystem.FileExists("foo.txt")).Should().BeFalse();
		}

		[Test]
		public void TestFileExists1()
		{
			const string fileName = "stuff.txt";

			Wait(_filesystem.FileExists(fileName)).Should().BeFalse("because we haven't created any files yet");
		}
		
		[Test]
		public void TestFileExists2()
		{
			const string fileName = "stuff.txt";

			Wait(_filesystem.GetFileInfo(fileName).Exists).Should().BeFalse("because we haven't created any files yet");
		}

		[Test]
		public void TestFileExists3()
		{
			var fileName = Path.Combine(_filesystem.CurrentDirectory, "stuff.txt");

			Wait(_filesystem.GetDirectoryInfo(_filesystem.CurrentDirectory).FileExists(fileName)).Should().BeFalse("because we haven't created any files yet");
		}

		[Test]
		public void TestFileExists4()
		{
			const string fileName = "foobar\\stuff.txt";
			Wait(_filesystem.FileExists(fileName)).Should().BeFalse("because the directory doesn't even exist");
		}

		[Test]
		public void TestCreateFile1()
		{
			const string fileName = "stuff.txt";
			Wait(_filesystem.FileExists(fileName)).Should().BeFalse();

			using (var stream = Wait(_filesystem.CreateFile(fileName)))
			{
				stream.Should().NotBeNull();
				stream.CanRead.Should().BeTrue("because the file should've been opened for writing");
				stream.CanWrite.Should().BeTrue("because the file should've been opened for reading");
				stream.Position.Should().Be(0, "because the stream should point towards the start of the file");
				stream.Length.Should().Be(0, "because the file should be empty since it was just created");

				Wait(_filesystem.FileExists(fileName)).Should().BeTrue("because we've just created that file");
			}
		}

		[Test]
		[Description("Verifies that a previously created file is replaced")]
		public void TestCreateFile2()
		{
			const string fileName = "stuff";
			using (var stream = Wait(_filesystem.CreateFile(fileName)))
			{
				stream.WriteByte(128);
			}

			using (var stream = Wait(_filesystem.CreateFile(fileName)))
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
			new Action(() => Wait(_filesystem.CreateFile(fileName))).ShouldThrow<DirectoryNotFoundException>();
		}

		[Test]
		[Description("Verifies that CreateFile is case insensitive")]
		public void TestCreateFile4()
		{
			const string fileName = "bar";
			using (Wait(_filesystem.CreateFile(fileName.ToLower()))) { }
			using (Wait(_filesystem.CreateFile(fileName.ToUpper()))) { }

			var files = Wait(_filesystem.EnumerateFiles(_filesystem.CurrentDirectory));
			files.Should().HaveCount(1, "because only one file should've been created");
		}

		[Test]
		[Description("Verifies that OpenRead throws when the file doesn't exist")]
		public void TestOpenRead1()
		{
			const string fileName = "stuff";
			new Action(() => Wait(_filesystem.OpenRead(fileName))).ShouldThrow<FileNotFoundException>(
				"because there's no such file");
		}

		[Test]
		[Description("Verifies that OpenWrite actually allows writing data to a file")]
		public void TestOpenWrite1()
		{
			const string fileName = "stuff";
			using (var stream = Wait(_filesystem.OpenWrite(fileName)))
			{
				stream.WriteByte(128);
			}

			using (var stream = Wait(_filesystem.OpenRead(fileName)))
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
			using (var stream = Wait(_filesystem.OpenWrite(fileName)))
			{
				stream.WriteByte(255);
			}

			using (var stream = Wait(_filesystem.OpenWrite(fileName)))
			{
				stream.WriteByte(42);
			}

			using (var stream = Wait(_filesystem.OpenRead(fileName)))
			{
				stream.Position.Should().Be(0);
				stream.Length.Should().Be(1);
				stream.ReadByte().Should().Be(42);
			}
		}

		[Test]
		[Description("Verifies that the length of a newly written file is 0")]
		public void TestFileLength1()
		{
			const string fileName = "stuff";
			using (var stream = Wait(_filesystem.OpenWrite(fileName)))
			{
			}

			const string reason = "because we've written nothing to the file so far";
			Wait(_filesystem.FileLength(fileName)).Should().Be(0, reason);
			Wait(_filesystem.GetFileInfo(fileName).Length).Should().Be(0, reason);
		}

		[Test]
		[Description("Verifies that writing data to the file updates its length")]
		public void TestFileLength2()
		{
			const string fileName = "stuff";
			using (var stream = Wait(_filesystem.OpenWrite(fileName)))
			{
				stream.Write(new byte[42], 0, 42);
			}

			const string reason = "because we've written 42 bytes to the file";
			Wait(_filesystem.FileLength(fileName)).Should().Be(42, reason);
			Wait(_filesystem.GetFileInfo(fileName).Length).Should().Be(42, reason);
		}

		[Test]
		[Ignore("Not yet implemented")]
		[Description("Verifies that writing data to the file updates its length")]
		public void TestFileLength3()
		{
			const string fileName = "stuff";
			using (var stream = Wait(_filesystem.OpenWrite(fileName)))
			{
				stream.Write(new byte[42], 0, 42);
			}

			var fileInfo = _filesystem.GetFileInfo(fileName);
			Wait(fileInfo.Delete());
			new Action(() => Wait(fileInfo.Length))
				.ShouldThrow<AggregateException>()
				.WithInnerException<FileNotFoundException>();
			new Action(() => Wait(_filesystem.FileLength(fileName)))
				.ShouldThrow<AggregateException>()
				.WithInnerException<FileNotFoundException>();
		}
	}
}
