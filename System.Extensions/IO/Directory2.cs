using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.IO
{
	internal static class Directory2
	{
		private static readonly char[] DirectorySeparatorChars =
			new[] {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};
		public static readonly string DirectorySeparatorString = Path.DirectorySeparatorChar.ToString();
		public static readonly string AltDirectorySeparatorString = Path.AltDirectorySeparatorChar.ToString();

		/// <summary>
		///     Splits the given path into its individual components.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		[Pure]
		public static IReadOnlyList<string> Split(string path)
		{
			var components = new List<string>();
			var separators = new[] {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};
			int next;
			var start = 0;
			while ((next = path.IndexOfAny(separators, start)) != -1)
			{
				var component = path.Substring(start, next - start);
				components.Add(component);
				start = next + 1;
			}

			if (Path.IsPathRooted(path))
				components[index: 0] = components[index: 0] + DirectorySeparatorString;

			if (start < path.Length)
				components.Add(path.Substring(start));

			if (path.EndsWithAny(separators))
			{
				var last = components[components.Count - 1];
				last += Path.DirectorySeparatorChar;
				components[components.Count - 1] = last;
			}

			return components;
		}

		/// <summary>
		///     See http://referencesource.microsoft.com/#mscorlib/system/io/directoryinfo.cs,e3b20cb1c28ea93f
		/// </summary>
		/// <param name="fullPath"></param>
		/// <returns></returns>
		[Pure]
		public static string GetDirName(string fullPath)
		{
			string dirName;
			if (fullPath.Length > 3)
			{
				var s = fullPath;
				if (fullPath.EndsWithAny(DirectorySeparatorChars))
					s = fullPath.Substring(startIndex: 0, length: fullPath.Length - 1);
				dirName = Path.GetFileName(s);
			}
			else
			{
				dirName = fullPath; // For rooted paths, like "c:\"
			}
			return dirName;
		}
	}
}