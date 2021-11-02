using System.Collections.Generic;
using System.IO;
using System.Reflection;

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
			/*":",*/ //< Behaves differently in .NET Standard than in .NET
			"?",
			"C\\?",
			/*"fo:bar?*>",*/ //< Behaves differently in .NET Standard than in .NET
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
	}
}