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