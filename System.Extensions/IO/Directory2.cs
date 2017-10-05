using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.IO
{
	internal static class Directory2
	{
		public static readonly string DirectorySeparatorChar = Path.DirectorySeparatorChar.ToString();
		public static readonly string AltDirectorySeparatorChar = Path.AltDirectorySeparatorChar.ToString();

		/// <summary>
		///     Splits the given path into its individual components.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		[Pure]
		public static IReadOnlyList<string> Split(string path)
		{
			var components = new List<string>();
			var separator = Path.DirectorySeparatorChar;
			int next;
			var start = 0;
			while ((next = path.IndexOf(separator, start)) != -1)
			{
				var component = path.Substring(start, next - start);
				components.Add(component);
				start = next + 1;
			}

			if (Path.IsPathRooted(path))
				components[index: 0] = components[index: 0] + DirectorySeparatorChar;

			if (start < path.Length)
				components.Add(path.Substring(start));
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
				if (fullPath.EndsWith(DirectorySeparatorChar))
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