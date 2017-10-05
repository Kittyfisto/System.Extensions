using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace System.Extensions.Test.IO
{
	[TestFixture]
	public class FilesystemTest
	{
		private Filesystem _filesystem;
		private SerialTaskScheduler _scheduler;

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

		[SetUp]
		public void Setup()
		{
			_scheduler = new SerialTaskScheduler();
			_filesystem = new Filesystem(_scheduler);
		}

		[TearDown]
		public void Teardown()
		{
			_scheduler.Dispose();
		}

		[Test]
		public void TestFileExists1()
		{
			Await(_filesystem.FileExists("wdawaddwawadoknfawonafw"))
				.Should().BeFalse("because the file doesn't exist");
		}

		[Test]
		public void TestFileExists2()
		{
			Await(_filesystem.FileExists(AssemblyFilePath))
				.Should().BeTrue("because that assembly most certainly exists");
		}

		[Test]
		public void TestFileExists3([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			File.Exists(invalidPath).Should().BeFalse();
			Await(_filesystem.FileExists(invalidPath)).Should().BeFalse();
		}

		[Test]
		public void TestDirectoryExists1()
		{
			Await(_filesystem.DirectoryExists("dawwadwadadwawd"))
				.Should().BeFalse("because the directory doesn't exist");
		}

		[Test]
		public void TestDirectoryExists2()
		{
			Await(_filesystem.DirectoryExists(AssemblyDirectory))
				.Should().BeTrue("because that directory most certainly exists");
		}

		[Test]
		public void TestDirectoryExists3([ValueSource(nameof(InvalidPaths))] string invalidPath)
		{
			Directory.Exists(invalidPath).Should().BeFalse();
			Await(_filesystem.DirectoryExists(invalidPath)).Should().BeFalse();
		}

		[Test]
		public void TestEnumerateFiles1()
		{
			var expected = Directory.EnumerateFiles(AssemblyDirectory).ToList();
			var actual = Await(_filesystem.EnumerateFiles(AssemblyDirectory));

			Console.WriteLine("Found {0} files", actual.Count);
			actual.Should().NotBeNull();
			actual.Should().BeEquivalentTo(expected);
		}

		[Test]
		public void TestEnumerateFiles2()
		{
			const string filter = "*.pdb";
			var expected = Directory.EnumerateFiles(AssemblyDirectory, filter).ToList();
			var actual = Await(_filesystem.EnumerateFiles(AssemblyDirectory, filter));

			Console.WriteLine("Found {0} files", actual.Count);
			actual.Should().NotBeNull();
			actual.Should().BeEquivalentTo(expected);
		}

		[Test]
		[Description("Verifies that both IFilesystem.EnumerateFiles and Directory.EnumerateFiles throw identical exceptions")]
		public void TestEnumerateFiles3()
		{
			new Action(() => Directory.EnumerateFiles(null)).ShouldThrow<ArgumentNullException>();
			new Action(() => Await(_filesystem.EnumerateFiles(null))).ShouldThrow<ArgumentNullException>();
		}

		[Test]
		[Description("Verifies that both IFilesystem.EnumerateFiles and Directory.EnumerateFiles throw identical exceptions")]
		public void TestEnumerateFiles4()
		{
			const string searchPattern = "*.*";
			new Action(() => Directory.EnumerateFiles(null, searchPattern)).ShouldThrow<ArgumentNullException>();
			new Action(() => Await(_filesystem.EnumerateFiles(null, searchPattern))).ShouldThrow<ArgumentNullException>();
		}

		[Test]
		[Description("Verifies that both IFilesystem.EnumerateFiles and Directory.EnumerateFiles throw identical exceptions")]
		public void TestEnumerateFiles5()
		{
			new Action(() => Directory.EnumerateFiles(AssemblyDirectory, null)).ShouldThrow<ArgumentNullException>();
			new Action(() => Await(_filesystem.EnumerateFiles(AssemblyDirectory, null))).ShouldThrow<ArgumentNullException>();
		}

		[Test]
		[Description("Verifies that both IFilesystem.EnumerateFiles and Directory.EnumerateFiles throw identical exceptions")]
		public void TestEnumerateFiles6()
		{
			new Action(() => Directory.EnumerateFiles(AssemblyFilePath)).ShouldThrow<IOException>();
			new Action(() => Await(_filesystem.EnumerateFiles(AssemblyFilePath))).ShouldThrow<IOException>();
		}

		[Test]
		public void TestGetFileInfo1()
		{
			var expected = new FileInfo(AssemblyFilePath);
			var actual = _filesystem.GetFileInfo(AssemblyFilePath);
			actual.Should().NotBeNull();
			actual.Name.Should().Be(expected.Name);
			actual.FullPath.Should().Be(AssemblyFilePath);
			Await(actual.Length).Should().Be(expected.Length, "because both methods should find the same file size");
			Await(actual.Exists).Should().BeTrue("because the file most certainly exists");
			Await(actual.IsReadOnly).Should().Be(expected.IsReadOnly, "because both methods should find the same attribute");
		}

		[Test]
		[Ignore("Not fully implemented")]
		public void TestGetFileInfo2([ValueSource(nameof(InvalidPaths))] string path)
		{
			new Action(() => new FileInfo(path)).ShouldThrow<ArgumentException>();
			new Action(() => _filesystem.GetFileInfo(path)).ShouldThrow<ArgumentException>();
		}

		[Test]
		public void TestGetDirectoryInfo1()
		{
			var info = _filesystem.GetDirectoryInfo(AssemblyDirectory);
			info.Should().NotBeNull();
			info.Name.Should().Be(Path.GetDirectoryName(AssemblyDirectory));
			info.FullName.Should().Be(AssemblyDirectory);
			Await(info.Exists).Should().BeTrue("because the folder most certainly exists");
		}

		[Test]
		public void TestGetDirectoryInfo2()
		{
			var info = _filesystem.GetDirectoryInfo(AssemblyDirectory);
			var expected = Await(_filesystem.EnumerateFiles(AssemblyDirectory));
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
			var info = _filesystem.GetDirectoryInfo(AssemblyDirectory);
			var expected = Await(_filesystem.EnumerateFiles(AssemblyDirectory, "*.dll"));
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
			var info = _filesystem.GetDirectoryInfo(AssemblyDirectory);
			var expected = Await(_filesystem.EnumerateFiles(AssemblyDirectory, "*.dll", searchOption));
			var actual = Await(info.EnumerateFiles("*.dll", searchOption));

			foreach (var fileInfo in actual)
			{
				fileInfo.Should().NotBeNull();
				expected.Should().Contain(fileInfo.FullPath);
				Await(fileInfo.Exists).Should().BeTrue();
			}
		}

		private static T Await<T>(Task<T> task)
		{
			task.Should().NotBeNull();

			var waitTime = TimeSpan.FromSeconds(2);
			task.Wait(waitTime).Should().BeTrue("because the task should've been finished after {0} seconds", waitTime.TotalSeconds);
			return task.Result;
		}

		private static string AssemblyFilePath
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().CodeBase;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return path;
			}
		}

		private static string AssemblyDirectory => Path.GetDirectoryName(AssemblyFilePath);
	}
}