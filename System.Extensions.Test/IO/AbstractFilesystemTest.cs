using System.IO;
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
		public void TestDeleteFileInvalidPath([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => Filesystem.DeleteFile(path)).ShouldThrow<ArgumentException>();
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
		public void TestCreateDirectory4()
		{
			var dir = _filesystem.GetDirectoryInfo("SomeDirectory");
			Await(dir.Exists).Should().BeFalse("because there's no such directory yet");
			Await(dir.Create());
			Await(dir.Exists).Should().BeTrue("because we've just created this directory");
			Await(_filesystem.DirectoryExists("SomeDirectory")).Should().BeTrue();
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
	}
}
