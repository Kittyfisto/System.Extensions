﻿using System.IO;
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
		public void TestCtor()
		{
			new Action(() => new InMemoryFilesystem(null))
				.ShouldThrow<ArgumentNullException>();
		}

		[Test]
		public void TestPrint1()
		{
			var print = Wait(Filesystem.Print());
			Console.WriteLine(print);
			print.Should().Be("M:\\ [Drive]\r\n");
		}

		[Test]
		public void TestPrint2()
		{
			Filesystem.AddRoot("D:\\");
			var print = Wait(Filesystem.Print());
			Console.WriteLine(print);
			print.Should().Be("D:\\ [Drive]\r\nM:\\ [Drive]\r\n");
		}
	}
}