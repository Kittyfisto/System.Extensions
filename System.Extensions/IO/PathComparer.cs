using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.IO
{
	/// <summary>
	///     Responsible for comparing two strings, treating them as paths.
	///     Two strings are equal if they represent the same path, regardless
	///     of formatting.
	/// </summary>
	public sealed class PathComparer
		: IEqualityComparer<string>
	{
		public bool Equals(string x, string y)
		{
			var nx = NormalizePath(x);
			var ny = NormalizePath(y);

			return string.Equals(nx, ny);
		}

		public int GetHashCode(string obj)
		{
			var nobj = NormalizePath(obj);
			return nobj.GetHashCode();
		}

		[Pure]
		public static string NormalizePath(string path)
		{
			return Path.GetFullPath(new Uri(path).LocalPath)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
				.ToUpperInvariant();
		}
	}
}