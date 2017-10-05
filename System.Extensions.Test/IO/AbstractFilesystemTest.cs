using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace System.Extensions.Test.IO
{
	[TestFixture]
	public abstract class AbstractFilesystemTest
	{
		private IFilesystem _filesystem;

		protected abstract IFilesystem Create();

		public IFilesystem Filesystem => _filesystem;

		[SetUp]
		public void Setup()
		{
			_filesystem = Create();
		}

		public static IEnumerable<string> InvalidPaths => new[]
		{
			null,
			"",
			" ",
			"  ",
			"\t",
			"\r",
			"\n",
			" \t ",
			":",
			"?",
			"C\\?"
		};

		[Test]
		public void TestDirectoryExistsInvalidPath([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			Await(Filesystem.DirectoryExists(invalidPath)).Should().BeFalse();
		}

		[Test]
		public void TestTestCurrentDirectory1()
		{
			Filesystem.CurrentDirectory.Should().NotBeNull();
		}

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

		protected static T Await<T>(Task<T> task)
		{
			task.Should().NotBeNull();

			var waitTime = TimeSpan.FromSeconds(2);
			task.Wait(waitTime).Should().BeTrue("because the task should've been finished after {0} seconds", waitTime.TotalSeconds);
			return task.Result;
		}
	}
}
