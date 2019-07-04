using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
			var roots = Filesystem.Roots;
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
		[Description("Verifies that the filesystem implementation behaves just like File.Write when given invalid paths")]
		public void TestWriteAllBytesInvalidPath([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			new Action(() => File.WriteAllBytes(invalidPath, new byte[0])).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.WriteAllBytes(invalidPath, new byte[0])).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.Write(invalidPath, new MemoryStream())).ShouldThrow<ArgumentException>();
		}

		[Test]
		[Description("Verifies that the filesystem implementation behaves just like File.Exists when given invalid paths")]
		public void TestFileExistsInvalidPath([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			File.Exists(invalidPath).Should().BeFalse();
			_filesystem.FileExists(invalidPath).Should().BeFalse();
		}

		[Test]
		public void TestDirectoryExistsInvalidPath([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			_filesystem.DirectoryExists(invalidPath).Should().BeFalse();
		}

		[Test]
		public void TestGetDirectoryInfoInvalidPath([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => _filesystem.GetDirectoryInfo(path)).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestGetFileInfoInvalidPath([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => _filesystem.GetFileInfo(path)).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestCreateFileInvalidPath([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => _filesystem.CreateFile(path)).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestOpenReadInvalidPath([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => _filesystem.OpenRead(path)).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestDeleteFileInvalidPath([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => _filesystem.DeleteFile(path)).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestDeleteDirectoryInvalidPath([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => _filesystem.DeleteDirectory(path)).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestEnumerateDirectoriesInvalidPath1([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => _filesystem.EnumerateDirectories(path)).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestEnumerateDirectoriesInvalidPath2([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => _filesystem.EnumerateDirectories(path, "*.*")).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestEnumerateDirectoriesInvalidPath3([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => _filesystem.EnumerateDirectories(path, "*.*", SearchOption.TopDirectoryOnly)).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestEnumerateFilesInvalidPath1([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => _filesystem.EnumerateFiles(path)).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestEnumerateFilesInvalidPath2([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => _filesystem.EnumerateFiles(path, "*.*")).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestEnumerateFilesInvalidPath3([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => _filesystem.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly)).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestCopyFileInvalidPath1([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => File.Copy(path, "foo.dat")).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.CopyFile(path, "foo.dat")).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestCopyFileInvalidPath2([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => File.Copy("foo.dat", path)).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.CopyFile("foo.dat", path)).ShouldThrow<ArgumentException>();
		}

		#endregion

		[Test]
		[Description("Verifies that creating a directory works with a relative path")]
		public void TestCreateDirectory1()
		{
			_filesystem.DirectoryExists("Foobar").Should().BeFalse();
			var directory = _filesystem.CreateDirectory("Foobar");
			directory.Should().NotBeNull();
			directory.Name.Should().Be("Foobar");
			directory.Exists.Should().BeTrue();
			_filesystem.DirectoryExists("Foobar").Should().BeTrue();
		}

		[Test]
		[Description("Verifies that creating a directory works with an absolute path")]
		public void TestCreateDirectory2()
		{
			const string directoryName = "Foobar";
			var directoryPath = Path.Combine(_filesystem.CurrentDirectory, directoryName);

			_filesystem.DirectoryExists(directoryPath).Should().BeFalse();
			var directory = _filesystem.CreateDirectory(directoryPath);
			directory.Should().NotBeNull();
			directory.Name.Should().Be(directoryName);
			directory.Exists.Should().BeTrue();
			_filesystem.DirectoryExists(directoryPath).Should().BeTrue();
		}

		[Test]
		[Description("Verifies that trying to create a directory a 2nd time is accepted and simply returns a reference to the directory")]
		public void TestCreateDirectory3()
		{
			var expected = _filesystem.CreateDirectory("Foobar");
			var actual = _filesystem.CreateDirectory("Foobar");

			actual.Should().NotBeNull();
			actual.FullName.Should().Be(expected.FullName);
		}

		[Test]
		[Description("Verifies that CreateDirectory is able to create a full directory path")]
		public void TestCreateDirectory4()
		{
			var path = Path.Combine(_filesystem.CurrentDirectory, "a", "b", "c");
			_filesystem.CreateDirectory(path);

			_filesystem.DirectoryExists(Path.Combine(_filesystem.CurrentDirectory, "a")).Should().BeTrue();
			_filesystem.DirectoryExists(Path.Combine(_filesystem.CurrentDirectory, "a", "b")).Should().BeTrue();
			_filesystem.DirectoryExists(Path.Combine(_filesystem.CurrentDirectory, "a", "b", "c")).Should().BeTrue();
		}

		[Test]
		[SetCulture("en-US")]
		[Description("Verifies that creating a directory for a non-existant root doesn't work")]
		public void TestCreateDirectory5()
		{
			const string directory = "Z:\\foo\\bar";
			new Action(() => _filesystem.CreateDirectory(directory))
				.ShouldThrow<DirectoryNotFoundException>()
				.WithMessage("Could not find a part of the path 'Z:\\foo\\bar'.");
		}

		[Test]
		public void TestDirectoryCreate1()
		{
			var dir = _filesystem.GetDirectoryInfo("SomeDirectory");
			dir.Exists.Should().BeFalse("because there's no such directory yet");
			dir.Create();
			dir.Exists.Should().BeTrue("because we've just created this directory");
			_filesystem.DirectoryExists("SomeDirectory").Should().BeTrue();
		}

		[Test]
		[Description("Verifies that deleting a non-existant directory throws")]
		public void TestDeleteDirectory1()
		{
			const string path = "foobar";
			_filesystem.DirectoryExists(path).Should().BeFalse();
			new Action(() => _filesystem.DeleteDirectory(path)).ShouldThrow<DirectoryNotFoundException>();
		}

		[Test]
		[Description("Verifies that deleting a non-existant directory throws")]
		public void TestDeleteDirectory2()
		{
			const string path = "foo\\bar";
			_filesystem.DirectoryExists(path).Should().BeFalse();
			new Action(() => _filesystem.DeleteDirectory(path)).ShouldThrow<DirectoryNotFoundException>();
		}

		[Test]
		[Description("Verifies that deleting a previously existing and empty directory works")]
		public void TestDeleteDirectory3()
		{
			const string path = "foobar";
			_filesystem.CreateDirectory(path);
			_filesystem.DirectoryExists(path).Should().BeTrue();
			_filesystem.DeleteDirectory(path);
			_filesystem.DirectoryExists(path).Should().BeFalse("because the directory should've been deleted");
		}

		[Test]
		[Description("Verifies that deleting a non-empty directory is not allowed")]
		public void TestDeleteDirectory4()
		{
			const string directory = "foobar";
			string filePath = Path.Combine(directory, "stuff.txt");
			_filesystem.CreateDirectory(directory);
			_filesystem.CreateFile(filePath);
			_filesystem.FileExists(filePath).Should().BeTrue();

			new Action(() => _filesystem.DeleteDirectory(directory)).ShouldThrow<IOException>("because deleting a non-empty directory is not allowed");
			new Action(() => _filesystem.GetDirectoryInfo(directory).Delete()).ShouldThrow<IOException>();

			_filesystem.DirectoryExists(directory).Should().BeTrue("because the directory shouldn't have been deleted");
			_filesystem.FileExists(filePath).Should().BeTrue("because the directory's contents shouldn't have been deleted");
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

			directoryInfo.Exists.Should().BeTrue();
			subDirectoryInfo.Exists.Should().BeTrue();

			_filesystem.DeleteDirectory(directory, true);
			directoryInfo.Exists.Should().BeFalse();
			subDirectoryInfo.Exists.Should().BeFalse();
		}

		[Test]
		public void TestGetDirectoryCaseInsensitive()
		{
			var actual = _filesystem.CreateDirectory("FoO");

			const string reason = "because we should retrieve an equal IDirectoryInfo object, regardless of the case";
			_filesystem.GetDirectoryInfo("foo").Should().Be(actual, reason);
			_filesystem.GetDirectoryInfo("FOO").Should().Be(actual, reason);
			_filesystem.GetDirectoryInfo("fOo").Should().Be(actual, reason);
		}
		
		[Test]
		public void TestGetFileCaseInsensitive()
		{
			var actual = _filesystem.GetFileInfo("FoO");
			using (_filesystem.CreateFile("FoO"))
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
			_filesystem.FileExists(filename).Should().BeFalse("because there is no such file");
			_filesystem.DeleteFile(filename);
			_filesystem.FileExists(filename).Should().BeFalse("because there is still no such file");
		}

		[Test]
		[Description("Verifies that deleting a file from a non-existant folder is not allowed and throws")]
		public void TestDeleteFile2()
		{
			const string filename = "foo\\bar\\file.dat";
			_filesystem.FileExists(filename).Should().BeFalse("because there is no such file");
			new Action(() => _filesystem.DeleteFile(filename)).ShouldThrow<DirectoryNotFoundException>(
				"because IFilesystem implementations must mimic their .NET counterparts as far as throwing exceptions is concerned");
			_filesystem.FileExists(filename).Should().BeFalse("because there is still no such file");
		}

		[Test]
		[Description("Verifies that deleting a file actually removes it")]
		public void TestDeleteFile3()
		{
			const string filename = "some file.dat";
			using (_filesystem.CreateFile(filename)) { }
			_filesystem.FileExists(filename).Should().BeTrue();

			_filesystem.DeleteFile(filename);
			_filesystem.FileExists(filename).Should().BeFalse("because we've just deleted the file");
			new Action(() => _filesystem.OpenRead(filename)).ShouldThrow<FileNotFoundException>();
		}

		[Test]
		[Description("Verifies that CreateSubdirectory can create single sub directory")]
		public void TestCreateSubdirectory1()
		{
			var directory = _filesystem.Current;
			var child = directory.CreateSubdirectory("SomeStuff");
			child.Should().NotBeNull();
			child.Name.Should().Be("SomeStuff");
			child.FullName.Should().Be(Path.Combine(_filesystem.CurrentDirectory, "SomeStuff"));

			child.Exists.Should().BeTrue();
			_filesystem.DirectoryExists(child.FullName).Should().BeTrue();
		}

		[Test]
		[Description("Verifies that CreateSubdirectory can create multiple directories at the same time")]
		public void TestCreateSubdirectory2()
		{
			var directory = _filesystem.Current;
			var child = directory.CreateSubdirectory("Some\\Stuff");
			child.Should().NotBeNull();
			child.Name.Should().Be("Stuff");
			child.FullName.Should().Be(Path.Combine(_filesystem.CurrentDirectory, "Some\\Stuff"));

			child.Exists.Should().BeTrue();
			_filesystem.DirectoryExists(child.FullName).Should().BeTrue();
		}

		[Test]
		[Description("Verifies that a created subdirectory can be found via enumeration")]
		public void TestCreateSubdirectory3()
		{
			var directory = _filesystem.Current;
			var child = directory.CreateSubdirectory("Yes");
			var childDirectories = _filesystem.EnumerateDirectories(directory.FullName);
			childDirectories.Should().NotBeNull();
			childDirectories.Should().HaveCount(1);
			childDirectories[0].Should().Be(child.FullName);
		}

		[Test]
		[Description("")]
		public void TestEnumerateDirectories1()
		{
			var path = _filesystem.CurrentDirectory;
			_filesystem.EnumerateDirectories(path).Should().BeEmpty("because we haven't created any additional directories");
		}

		[Test]
		[Description("")]
		public void TestEnumerateDirectories2()
		{
			var path = _filesystem.CurrentDirectory;
			_filesystem.EnumerateDirectories(path, "*").Should().BeEmpty("because we haven't created any additional directories");
		}

		[Test]
		[Description("")]
		public void TestEnumerateDirectories3([ValueSource(nameof(SearchOptions))] SearchOption searchOption)
		{
			var path = _filesystem.CurrentDirectory;
			_filesystem.EnumerateDirectories(path, "*", searchOption).Should().BeEmpty("because we haven't created any additional directories");
		}

		[Test]
		[Description("Verifies that an empty directory can be enumerated")]
		public void TestEnumerateFiles1()
		{
			_filesystem.EnumerateFiles(_filesystem.CurrentDirectory).Should().BeEmpty();
		}
		
		[Test]
		[Description("Verifies that an empty directory can be enumerated")]
		public void TestEnumerateFiles2()
		{
			_filesystem.Current.EnumerateFiles().Should().BeEmpty();
		}

		[Test]
		[Description("Verifies that a non-existing directory cannot be enumerated")]
		public void TestEnumerateFiles3()
		{
			new Action(() => _filesystem.EnumerateFiles("daawdw")).ShouldThrow<DirectoryNotFoundException>();
		}
		
		[Test]
		[Description("Verifies that a non-existing directory cannot be enumerated")]
		public void TestEnumerateFiles4()
		{
			new Action(() => _filesystem.GetDirectoryInfo("daawdw").EnumerateFiles()).ShouldThrow<DirectoryNotFoundException>();
		}

		[Test]
		[Description("Verifies that a newly created file can be found")]
		public void TestEnumerateFiles5()
		{
			_filesystem.EnumerateFiles(_filesystem.CurrentDirectory).Should().BeEmpty();
			using (_filesystem.CreateFile("a")) { }
			var files = _filesystem.EnumerateFiles(_filesystem.CurrentDirectory);
			files.Should().HaveCount(1);
			var info = _filesystem.GetFileInfo(files.First());
			info.Exists.Should().BeTrue("because we've just created that file");
		}

		[Test]
		[Description("Verifies that only files matching the search pattern are returned")]
		public void TestEnumerateFiles6()
		{
			_filesystem.EnumerateFiles(_filesystem.CurrentDirectory).Should().BeEmpty();
			using (_filesystem.CreateFile("a")) { }
			using (_filesystem.CreateFile("b")) { }

			_filesystem.EnumerateFiles(_filesystem.CurrentDirectory, "*a").Should().HaveCount(1);
			_filesystem.EnumerateFiles(_filesystem.CurrentDirectory, "*b").Should().HaveCount(1);
			_filesystem.EnumerateFiles(_filesystem.CurrentDirectory, "*").Should().HaveCount(2);
			_filesystem.EnumerateFiles(_filesystem.CurrentDirectory, "*c").Should().HaveCount(0);
		}
		
		[Test]
		[Description("Verifies that only files matching the search pattern are returned")]
		public void TestEnumerateFiles7()
		{
			_filesystem.EnumerateFiles(_filesystem.CurrentDirectory).Should().BeEmpty();
			using (_filesystem.CreateFile("a")) { }
			using (_filesystem.CreateFile("b")) { }

			var directory = _filesystem.Current;
			directory.EnumerateFiles("*a").Should().HaveCount(1);
			directory.EnumerateFiles("*b").Should().HaveCount(1);
			directory.EnumerateFiles("*").Should().HaveCount(2);
			directory.EnumerateFiles("*c").Should().HaveCount(0);
		}

		[Test]
		public void TestEnumerateFilesAllDirectories1()
		{
			_filesystem.CreateDirectory("A");
			_filesystem.CreateDirectory("B");
			_filesystem.CreateFile("A\\a.txt");
			_filesystem.CreateFile("B\\b.txt");

			var files = _filesystem.EnumerateFiles(_filesystem.CurrentDirectory, "*", SearchOption.AllDirectories);
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
			var files = directory.EnumerateFiles("*", SearchOption.AllDirectories);
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

			var files = _filesystem.EnumerateFiles("B", "*", SearchOption.AllDirectories);
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
			var files = directory.EnumerateFiles("*", SearchOption.AllDirectories);
			files.Should().HaveCount(1);
			files.First().FullPath.Should().Be(Path.Combine(_filesystem.CurrentDirectory, "B\\b.txt"));
		}

		[Test]
		public void TestFileCreate1()
		{
			var file = _filesystem.GetFileInfo("foo.txt");
			file.Exists.Should().BeFalse();
			using (var stream = file.Create())
			{
				stream.WriteByte(42);
			}
			file.Exists.Should().BeTrue();
		}

		[Test]
		[Description("Verifies that deleting a non existing file is allowed and essentially a NOP")]
		public void TestFileDelete1()
		{
			var file = _filesystem.GetFileInfo("foo.txt");
			new Action(() => file.Delete()).ShouldNotThrow();
		}

		[Test]
		[Description("Verifies that deleting a newly created file is possible")]
		public void TestFileDelete2()
		{
			var file = _filesystem.GetFileInfo("foo.txt");
			using (file.Create())
			{ }
			file.Exists.Should().BeTrue();

			file.Delete();
			file.Exists.Should().BeFalse();
			_filesystem.FileExists("foo.txt").Should().BeFalse();
		}

		[Test]
		public void TestFileExists1()
		{
			const string fileName = "stuff.txt";

			_filesystem.FileExists(fileName).Should().BeFalse("because we haven't created any files yet");
		}
		
		[Test]
		public void TestFileExists2()
		{
			const string fileName = "stuff.txt";

			_filesystem.GetFileInfo(fileName).Exists.Should().BeFalse("because we haven't created any files yet");
		}

		[Test]
		public void TestFileExists3()
		{
			var fileName = Path.Combine(_filesystem.CurrentDirectory, "stuff.txt");

			_filesystem.GetDirectoryInfo(_filesystem.CurrentDirectory).FileExists(fileName).Should().BeFalse("because we haven't created any files yet");
		}

		[Test]
		public void TestFileExists4()
		{
			const string fileName = "foobar\\stuff.txt";
			_filesystem.FileExists(fileName).Should().BeFalse("because the directory doesn't even exist");
		}
		
		[Test]
		[Description("Verifies that copying an empty file is allowed")]
		public void TestCopyFile1()
		{
			_filesystem.WriteAllBytes("a.dat", new byte[0]);
			_filesystem.FileExists("b.txt").Should().BeFalse();

			_filesystem.CopyFile("a.dat", "b.txt");
			_filesystem.ReadAllBytes("b.txt").Should().BeEmpty();
			_filesystem.FileExists("b.txt").Should().BeTrue("because now that the file has been copied, it should exist");
		}

		[Test]
		[Description("Verifies that copying a file copies all of its data")]
		public void TestCopyFile2()
		{
			_filesystem.WriteAllBytes("a.dat", new byte[] {4, 3, 2, 1});
			_filesystem.CreateDirectory("foo");
			_filesystem.FileExists("foo\\b.blub").Should().BeFalse();

			_filesystem.CopyFile("a.dat", "foo\\b.blub");
			_filesystem.ReadAllBytes("foo\\b.blub").Should().Equal(new byte[] {4, 3, 2, 1});
			_filesystem.FileExists("foo\\b.blub").Should().BeTrue("because now that the file has been copied, it should exist");
		}

		[Test]
		[Description("Verifies that copying over an existing file is not allowed")]
		public void TestCopyFile3()
		{
			_filesystem.WriteAllBytes("a.dat", new byte[] {4, 3, 2, 1});
			_filesystem.WriteAllBytes("b.dat", new byte[] {1, 2, 3});
			new Action(() => _filesystem.CopyFile("a.dat", "b.dat")).ShouldThrow<IOException>();
			_filesystem.ReadAllBytes("b.dat").Should().Equal(new byte[] {1, 2, 3}, "because the previous data should not have been overwritten");
		}

		[Test]
		[Description("Verifies that copying from a non-existing directory is not allowed")]
		public void TestCopyFile4()
		{
			new Action(() => _filesystem.CopyFile("foo\\a.dat", "b.dat")).ShouldThrow<DirectoryNotFoundException>();
		}
		
		[Test]
		[Description("Verifies that copying to a non-existing directory is not allowed")]
		public void TestCopyFile5()
		{
			using (_filesystem.CreateFile("a.dat"))
			{ }
			new Action(() => _filesystem.CopyFile("a.dat", "blub\\b.dat")).ShouldThrow<DirectoryNotFoundException>();
		}

		[Test]
		[Description("Verifies that copying from a non-existing file is not allowed")]
		public void TestCopyFile6()
		{
			new Action(() => _filesystem.CopyFile("a.dat", "b.dat")).ShouldThrow<FileNotFoundException>();
		}

		[Test]
		public void TestCreateFile1()
		{
			const string fileName = "stuff.txt";
			_filesystem.FileExists(fileName).Should().BeFalse();

			using (var stream = _filesystem.CreateFile(fileName))
			{
				stream.Should().NotBeNull();
				stream.CanRead.Should().BeTrue("because the file should've been opened for writing");
				stream.CanWrite.Should().BeTrue("because the file should've been opened for reading");
				stream.Position.Should().Be(0, "because the stream should point towards the start of the file");
				stream.Length.Should().Be(0, "because the file should be empty since it was just created");

				_filesystem.FileExists(fileName).Should().BeTrue("because we've just created that file");
			}
		}

		[Test]
		[Description("Verifies that a previously created file is replaced")]
		public void TestCreateFile2()
		{
			const string fileName = "stuff";
			using (var stream = _filesystem.CreateFile(fileName))
			{
				stream.WriteByte(128);
			}

			using (var stream = _filesystem.CreateFile(fileName))
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
			new Action(() => _filesystem.CreateFile(fileName)).ShouldThrow<DirectoryNotFoundException>();
		}

		[Test]
		[Description("Verifies that CreateFile is case insensitive")]
		public void TestCreateFile4()
		{
			const string fileName = "bar";
			using (_filesystem.CreateFile(fileName.ToLower())) { }
			using (_filesystem.CreateFile(fileName.ToUpper())) { }

			var files = _filesystem.EnumerateFiles(_filesystem.CurrentDirectory);
			files.Should().HaveCount(1, "because only one file should've been created");
		}

		[Test]
		[Description("Verifies that OpenRead throws when the file doesn't exist")]
		public void TestOpenRead1()
		{
			const string fileName = "stuff";
			new Action(() => _filesystem.OpenRead(fileName)).ShouldThrow<FileNotFoundException>(
				"because there's no such file");
		}

		[Test]
		[Description("Verifies that OpenWrite actually allows writing data to a file")]
		public void TestOpenWrite1()
		{
			const string fileName = "stuff";
			using (var stream = _filesystem.OpenWrite(fileName))
			{
				stream.WriteByte(128);
			}

			using (var stream = _filesystem.OpenRead(fileName))
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
			using (var stream = _filesystem.OpenWrite(fileName))
			{
				stream.WriteByte(255);
			}

			using (var stream = _filesystem.OpenWrite(fileName))
			{
				stream.WriteByte(42);
			}

			using (var stream = _filesystem.OpenRead(fileName))
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
			using (var stream = _filesystem.OpenWrite(fileName))
			{
			}

			const string reason = "because we've written nothing to the file so far";
			_filesystem.FileLength(fileName).Should().Be(0, reason);
			_filesystem.GetFileInfo(fileName).Length.Should().Be(0, reason);
		}

		[Test]
		[Description("Verifies that writing data to the file updates its length")]
		public void TestFileLength2()
		{
			const string fileName = "stuff";
			using (var stream = _filesystem.OpenWrite(fileName))
			{
				stream.Write(new byte[42], 0, 42);
			}

			const string reason = "because we've written 42 bytes to the file";
			_filesystem.FileLength(fileName).Should().Be(42, reason);
			_filesystem.GetFileInfo(fileName).Length.Should().Be(42, reason);
		}

		[Test]
		[Description("Verifies that writing data to the file updates its length")]
		public void TestFileLength3()
		{
			const string fileName = "stuff";
			using (var stream = _filesystem.OpenWrite(fileName))
			{
				stream.Write(new byte[42], 0, 42);
			}

			var fileInfo = _filesystem.GetFileInfo(fileName);
			fileInfo.Delete();
			new Action(() =>
				{
					var unused = fileInfo.Length;
				})
				.ShouldThrow<FileNotFoundException>();
			new Action(() => _filesystem.FileLength(fileName))
				.ShouldThrow<FileNotFoundException>();
		}

		[Test]
		public void TestWrite1()
		{
			new Action(() => _filesystem.Write("foo.dat", null))
				.ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void TestWrite2()
		{
			var data = new byte[] {1, 2, 3, 4};
			var stream = new MemoryStream(data);
			_filesystem.Write("foo.dat", stream);
			_filesystem.ReadAllBytes("foo.dat").Should().Equal(data);
		}

		[Test]
		[Description("Verifies that Write overwrites previous content")]
		public void TestWrite3()
		{
			_filesystem.Write("foo.dat", new MemoryStream(new byte[] {1, 2, 3, 4}));
			_filesystem.Write("foo.dat", new MemoryStream(new byte[] {2, 3, 4}));
			_filesystem.Write("foo.dat", new MemoryStream(new byte[] {3, 4}));
			_filesystem.Write("foo.dat", new MemoryStream(new byte[] {4}));
			_filesystem.ReadAllBytes("foo.dat").Should().Equal(new byte[] {4}, "because every Write() operation should overwrite previous content");
		}

		[Test]
		public void TestWriteAllBytes1()
		{
			new Action(() => _filesystem.WriteAllBytes("foo.dat", null))
				.ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void TestWriteAllBytes2()
		{
			var data = new byte[] {1, 2, 3, 4};
			_filesystem.WriteAllBytes("foo.dat", data);
			_filesystem.ReadAllBytes("foo.dat").Should().Equal(data);
		}

		[Test]
		[Description("Verifies that Write overwrites previous content, but keeps its filesize")]
		public void TestWriteAllBytes3()
		{
			_filesystem.WriteAllBytes("foo.dat", new byte[] {1, 2, 3, 4});
			_filesystem.WriteAllBytes("foo.dat", new byte[] {2, 3, 4});
			_filesystem.WriteAllBytes("foo.dat", new byte[] {3, 4});
			_filesystem.WriteAllBytes("foo.dat", new byte[] {4});
			_filesystem.ReadAllBytes("foo.dat").Should().Equal(new byte[] {4}, "because every WriteAllBytes() operation should overwrite previous content");
		}

		[Test]
		public void TestWriteAllText()
		{
			_filesystem.WriteAllText("hello.txt", "No, you're breathtaking!");
			_filesystem.ReadAllText("hello.txt").Should().Be("No, you're breathtaking!");
		}

		[Test]
		public void TestWriteAllTextUtf32()
		{
			_filesystem.WriteAllText("hello.txt", "No, you're breathtaking!", Encoding.UTF32);
			_filesystem.ReadAllText("hello.txt", Encoding.UTF32).Should().Be("No, you're breathtaking!");
		}

		[Test]
		public void TestReadAllText()
		{
			_filesystem.WriteAllBytes("foo.dat", Encoding.UTF8.GetBytes("The lazy brown..."));
			_filesystem.ReadAllText("foo.dat").Should().Be("The lazy brown...");
		}

		[Test]
		public void TestReadAllTextUTF7()
		{
			_filesystem.WriteAllBytes("foo.dat", Encoding.UTF7.GetBytes("The lazy brown..."));
			_filesystem.ReadAllText("foo.dat", Encoding.UTF7).Should().Be("The lazy brown...");
		}

		[Test]
		public void TestReadAllTextUTF32()
		{
			_filesystem.WriteAllBytes("foo.dat", Encoding.UTF32.GetBytes("The lazy brown..."));
			_filesystem.ReadAllText("foo.dat", Encoding.UTF32).Should().Be("The lazy brown...");
		}

		[Test]
		public void TestReadAllTextFileDoesNotExist()
		{
			new Action(() => _filesystem.ReadAllText("foo.dat"))
				.ShouldThrow<FileNotFoundException>();
		}

		[Test]
		public void TestReadAllLines()
		{
			_filesystem.WriteAllBytes("foo.dat", Encoding.UTF8.GetBytes("a\r\nb\r\nc"));
			_filesystem.ReadAllLines("foo.dat").Should().Equal(new[]{"a", "b", "c"});
		}

		[Test]
		public void TestReadAllLinesFileDoesNotExist()
		{
			new Action(() => _filesystem.ReadAllLines("foo.dat"))
				.ShouldThrow<FileNotFoundException>();
		}

		[Test]
		public void TestWatchNonExistantDirectory()
		{
			using (var watcher = _filesystem.Watchdog.StartDirectoryWatch("NonExistantFolder"))
			{
				watcher.Should().NotBeNull();
				watcher.Files.Should().BeEmpty("because the watched folder doesn't exist");
			}
		}

		[Test]
		public void TestWatchEmptyDirectory()
		{
			_filesystem.CreateDirectory("SomeFolder");
			using (var watcher = _filesystem.Watchdog.StartDirectoryWatch("SomeFolder"))
			{
				watcher.Files.Should().BeEmpty();
			}
		}

		[Test]
		public void TestWatchNonEmptyDirectory()
		{
			_filesystem.CreateDirectory("SomeFolder");
			_filesystem.WriteAllBytes("SomeFolder\\a.txt", new byte[123]);
			using (var watcher = _filesystem.Watchdog.StartDirectoryWatch("SomeFolder"))
			{
				watcher.Property(x => x.Files).ShouldEventually().HaveCount(1);

				var file = watcher.Files.First();
				file.Should().NotBeNull();
				file.Name.Should().EndWith("a.txt");
			}
		}

		[Test]
		public void TestWatchCreateDeleteCreateDirectory()
		{
			_filesystem.CreateDirectory("SomeFolder");
			_filesystem.WriteAllBytes("SomeFolder\\a.txt", new byte[123]);

			using (var watcher = _filesystem.Watchdog.StartDirectoryWatch("SomeFolder"))
			{
				watcher.Property(x => x.Files).ShouldEventually().HaveCount(1);

				_filesystem.DeleteFile("SomeFolder\\a.txt");
				_filesystem.DeleteDirectory("SomeFolder");
				watcher.Property(x => x.Files).ShouldEventually().HaveCount(0);

				_filesystem.CreateDirectory("SomeFolder");
				_filesystem.WriteAllBytes("SomeFolder\\a.txt", new byte[123]);
				watcher.Property(x => x.Files).ShouldEventually().HaveCount(1);
			}
		}

		[Test]
		public void TestWatchWriteAllBytes()
		{
			_filesystem.CreateDirectory("SomeFolder");
			using (var watcher = _filesystem.Watchdog.StartDirectoryWatch("SomeFolder"))
			{
				watcher.MonitorEvents();
				watcher.Property(x => x.Files).ShouldEventually().BeEmpty();

				_filesystem.WriteAllBytes("SomeFolder\\a.txt", new byte[123]);
				watcher.Property(x => x.Files).ShouldEventually().HaveCount(1);
				watcher.Files.Should().HaveCount(1);
				var file = watcher.Files.First();
				file.Should().NotBeNull();
				file.Name.Should().EndWith("a.txt");

				watcher.ShouldRaise(nameof(watcher.Changed));
			}
		}

		[Test]
		public void TestWatchCreateFile()
		{
			_filesystem.CreateDirectory("SomeFolder");
			using (var watcher = _filesystem.Watchdog.StartDirectoryWatch("SomeFolder"))
			{
				watcher.MonitorEvents();
				watcher.Property(x => x.Files).ShouldEventually().BeEmpty();

				using (_filesystem.CreateFile("SomeFolder\\b.txt"))
				{}

				watcher.Property(x => x.Files).ShouldEventually().HaveCount(1);
				watcher.Files.Should().HaveCount(1);
				var file = watcher.Files.First();
				file.Should().NotBeNull();
				file.Name.Should().EndWith("b.txt");

				watcher.ShouldRaise(nameof(watcher.Changed));
			}
		}

		[Test]
		public void TestWatchOpenWriteFile()
		{
			_filesystem.CreateDirectory("SomeFolder");
			using (var watcher = _filesystem.Watchdog.StartDirectoryWatch("SomeFolder"))
			{
				watcher.MonitorEvents();
				watcher.Property(x => x.Files).ShouldEventually().BeEmpty();

				using (_filesystem.OpenWrite("SomeFolder\\b.txt"))
				{}

				watcher.Property(x => x.Files).ShouldEventually().HaveCount(1);
				watcher.Files.Should().HaveCount(1);
				var file = watcher.Files.First();
				file.Should().NotBeNull();
				file.Name.Should().EndWith("b.txt");

				watcher.ShouldRaise(nameof(watcher.Changed));
			}
		}

		[Test]
		public void TestWatchAddIrrelevantFile()
		{
			_filesystem.CreateDirectory("SomeFolder");
			using (var watcher = _filesystem.Watchdog.StartDirectoryWatch("SomeFolder"))
			{
				watcher.MonitorEvents();
				watcher.Files.Should().BeEmpty();

				_filesystem.WriteAllBytes("a.txt", new byte[123]);
				watcher.Files.Should().BeEmpty();
				watcher.ShouldNotRaise(nameof(watcher.Changed));
			}
		}

		[Test]
		public void TestWatchDeleteFile()
		{
			_filesystem.CreateDirectory("SomeFolder");
			_filesystem.WriteAllBytes("SomeFolder\\a.txt", new byte[123]);
			using (var watcher = _filesystem.Watchdog.StartDirectoryWatch("SomeFolder"))
			{
				watcher.MonitorEvents();
				watcher.Property(x => x.Files).ShouldEventually().HaveCount(1);

				_filesystem.DeleteFile("SomeFolder\\a.txt");
				watcher.Property(x => x.Files).ShouldEventually().BeEmpty();

				watcher.ShouldRaise(nameof(watcher.Changed));
			}
		}

		[Test]
		public void TestWatchTopLevelOnly()
		{
			_filesystem.CreateDirectory("SomeFolder");
			using (var watcher = _filesystem.Watchdog.StartDirectoryWatch("SomeFolder"))
			{
				_filesystem.CreateDirectory("SomeFolder\\Blub");
				watcher.Property(x => x.Files).ShouldEventually().BeEmpty();

				_filesystem.WriteAllBytes("SomeFolder\\Blub\\b.txt", new byte[0]);
				watcher.Property(x => x.Files).ShouldEventually().BeEmpty();

				_filesystem.WriteAllBytes("SomeFolder\\a.txt", new byte[123]);
				watcher.Property(x => x.Files).ShouldEventually().HaveCount(1);
			}
		}

		[Test]
		public void TestWatchAllDirectories()
		{
			_filesystem.CreateDirectory("SomeFolder");
			_filesystem.WriteAllBytes("SomeFolder\\a.txt", new byte[123]);
			_filesystem.CreateDirectory("SomeFolder\\Blub");
			_filesystem.WriteAllBytes("SomeFolder\\Blub\\b.txt", new byte[0]);

			using (var watcher = _filesystem.Watchdog.StartDirectoryWatch("SomeFolder", null, SearchOption.AllDirectories))
			{
				watcher.Property(x => x.Files).ShouldEventually().HaveCount(2);
				watcher.Files.Should().Contain(x => x.FullPath.EndsWith("a.txt"));
				watcher.Files.Should().Contain(x => x.FullPath.EndsWith("b.txt"));
			}
		}

		[Test]
		public void TestWatchWithSearchPattern1()
		{
			_filesystem.CreateDirectory("SomeFolder");
			_filesystem.WriteAllBytes("SomeFolder\\a", new byte[0]);
			_filesystem.WriteAllBytes("SomeFolder\\b", new byte[0]);

			using (var watcher = _filesystem.Watchdog.StartDirectoryWatch("SomeFolder", "a"))
			{
				watcher.Property(x => x.Files).ShouldEventually().HaveCount(1);
				watcher.Files.Should().Contain(x => x.FullPath.EndsWith("a"));
			}
		}

		[Test]
		public void TestWatchWithSearchPattern2()
		{
			_filesystem.CreateDirectory("SomeFolder");
			_filesystem.CreateDirectory("SomeFolder\\b");

			_filesystem.WriteAllBytes("SomeFolder\\a", new byte[0]);
			_filesystem.WriteAllBytes("SomeFolder\\ba", new byte[0]);
			_filesystem.WriteAllBytes("SomeFolder\\b\\b", new byte[0]);
			_filesystem.WriteAllBytes("SomeFolder\\c", new byte[0]);

			using (var watcher = _filesystem.Watchdog.StartDirectoryWatch("SomeFolder", "*a", SearchOption.AllDirectories))
			{
				watcher.Property(x => x.Files).ShouldEventually().HaveCount(2);
				watcher.Files.Should().Contain(x => x.FullPath.EndsWith("a"));
				watcher.Files.Should().Contain(x => x.FullPath.EndsWith("ba"));
			}
		}

		[Test]
		public void TestWatchNullSearchPattern()
		{
			_filesystem.CreateDirectory("SomeFolder");
			_filesystem.CreateDirectory("SomeFolder\\b");

			_filesystem.WriteAllBytes("SomeFolder\\a", new byte[0]);
			_filesystem.WriteAllBytes("SomeFolder\\ba", new byte[0]);
			_filesystem.WriteAllBytes("SomeFolder\\b\\b", new byte[0]);
			_filesystem.WriteAllBytes("SomeFolder\\c", new byte[0]);

			using (var watcher = _filesystem.Watchdog.StartDirectoryWatch("SomeFolder", null, SearchOption.AllDirectories))
			{
				watcher.Property(x => x.Files).ShouldEventually().HaveCount(4);
				watcher.Files.Should().Contain(x => x.FullPath.EndsWith("a"));
				watcher.Files.Should().Contain(x => x.FullPath.EndsWith("ba"));
				watcher.Files.Should().Contain(x => x.FullPath.EndsWith("b"));
				watcher.Files.Should().Contain(x => x.FullPath.EndsWith("c"));
			}
		}

		[Test]
		public void TestWatchEmptySearchPattern()
		{
			_filesystem.CreateDirectory("SomeFolder");
			_filesystem.CreateDirectory("SomeFolder\\b");

			_filesystem.WriteAllBytes("SomeFolder\\a", new byte[0]);
			_filesystem.WriteAllBytes("SomeFolder\\ba", new byte[0]);
			_filesystem.WriteAllBytes("SomeFolder\\b\\b", new byte[0]);
			_filesystem.WriteAllBytes("SomeFolder\\c", new byte[0]);

			using (var watcher = _filesystem.Watchdog.StartDirectoryWatch("SomeFolder", "", SearchOption.AllDirectories))
			{
				watcher.Property(x => x.Files).ShouldEventually().BeEmpty();
			}
		}
	}
}
