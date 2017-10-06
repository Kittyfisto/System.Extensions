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
		public static readonly string ServerShareString = string.Format("{0}{0}", Path.DirectorySeparatorChar);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		[Pure]
		public static IReadOnlyList<string> Tokenise(string path)
		{
			var components = Split(path);
			var ret = new string[components.Count];
			ret[0] = components[0];
			for (int i = 1; i < ret.Length; ++i)
			{
				ret[i] = Path.Combine(ret[i - 1], components[i]);
			}
			return ret;
		}

		/// <summary>
		///     Splits the given path into its individual components.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		[Pure]
		public static IReadOnlyList<string> Split(string path)
		{
			var components = new List<string>();

			int next;
			var start = 0;
			var separators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
			if (path.StartsWith(ServerShareString))
			{
				start = path.IndexOfAny(separators, 2);
				if (start != -1)
				{
					components.Add(path.Substring(0, start));
					++start;
				}
				else
				{
					components.Add(path);
				}
			}

			while (start != -1 && (next = path.IndexOfAny(separators, start)) != -1)
			{
				var component = path.Substring(start, next - start);
				components.Add(component);
				start = next + 1;
			}

			if (Path.IsPathRooted(path))
				components[index: 0] = components[index: 0] + DirectorySeparatorString;

			if (start != -1 && start < path.Length)
				components.Add(path.Substring(start));

			if (path.EndsWithAny(separators))
			{
				var last = components[components.Count - 1];
				if (!last.EndsWithAny(separators))
				{
					last += Path.DirectorySeparatorChar;
					components[components.Count - 1] = last;
				}
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