using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;

namespace System.Extensions.Test.IO
{
	public abstract class AbstractFileTest
	{
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
			"C\\?",
			"fo:bar?*>",
			"C:\\almost\0valid"
		};

		protected static string AssemblyFilePath
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().CodeBase;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return path;
			}
		}

		protected static string AssemblyDirectory => Path.GetDirectoryName(AssemblyFilePath);

		protected static void Wait(Task task)
		{
			task.Should().NotBeNull();

			var waitTime = TimeSpan.FromSeconds(2);
			task.Wait(waitTime).Should().BeTrue("because the task should've been finished after {0} seconds", waitTime.TotalSeconds);
		}

		protected static T Wait<T>(Task<T> task)
		{
			task.Should().NotBeNull();

			var waitTime = TimeSpan.FromSeconds(2);
			task.Wait(waitTime).Should().BeTrue("because the task should've been finished after {0} seconds", waitTime.TotalSeconds);
			return task.Result;
		}
	}
}