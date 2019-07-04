using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace System.Extensions.Test.IO
{
	[TestFixture]
	public sealed class InMemoryFilesystemTest
		: AbstractFilesystemTest
	{
		protected override IFilesystem Create()
		{
			return new InMemoryFilesystem();
		}

		public new InMemoryFilesystem Filesystem => (InMemoryFilesystem) base.Filesystem;

		[Test]
		public void TestCreateDirectory()
		{
			var temp = @"C:\Users\simon\AppData\Local\Temp\";
			Filesystem.AddRoot(Path.GetPathRoot(temp));
			var directory = Filesystem.CreateDirectory(temp);
			directory.FullName.Should().Be(temp);
			directory.Name.Should().Be("Temp");
			directory.FullName.Should().Be(temp);

			var filePath = Path.Combine(temp, "foo.txt");
			Filesystem.WriteAllText(filePath, "What's up!");
		}

		[Test]
		public void TestCreateAndDeleteDirectory()
		{
			var temp = @"C:\Users\simon\AppData\Local\Temp\";
			Filesystem.AddRoot(Path.GetPathRoot(temp));
			Filesystem.CreateDirectory(temp);
			Filesystem.DirectoryExists(temp).Should().BeTrue();
			Filesystem.DirectoryExists(@"C:\Users\simon\AppData\Local\Temp\").Should().BeTrue();

			Filesystem.DeleteDirectory(temp);
			Filesystem.DirectoryExists(temp).Should().BeFalse();
		}

		[Test]
		public void TestCreateDirectoryAndTestExistence()
		{
			var temp = @"C:\Users\simon\AppData\Local\Temp\";
			Filesystem.AddRoot(Path.GetPathRoot(temp));
			Filesystem.CreateDirectory(temp);
			Filesystem.DirectoryExists(@"C:\Users\simon\AppData\Local\Temp\").Should().BeTrue();
			Filesystem.DirectoryExists(@"C:/Users/simon/AppData/Local/Temp/").Should().BeTrue();
			Filesystem.DirectoryExists(@"C:\Users\simon\AppData\Local\Temp").Should().BeTrue();
			Filesystem.DirectoryExists(@"C:\Users/simon\AppData/Local\Temp").Should().BeTrue();
		}

		[Test]
		public void TestPrint1()
		{
			var print = Filesystem.Print();
			Console.WriteLine(print);
			print.Should().Be("M:\\ [Drive]\r\n");
		}

		[Test]
		public void TestPrint2()
		{
			Filesystem.AddRoot("D:\\");
			var print = Filesystem.Print();
			Console.WriteLine(print);
			print.Should().Be("D:\\ [Drive]\r\nM:\\ [Drive]\r\n");
		}
	}
}