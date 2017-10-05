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
		/// <inheritdoc />
		public bool Equals(string x, string y)
		{
			var nx = NormalizePath(x);
			var ny = NormalizePath(y);

			return string.Equals(nx, ny);
		}

		/// <inheritdoc />
		public int GetHashCode(string obj)
		{
			var nobj = NormalizePath(obj);
			return nobj != null ? nobj.GetHashCode() : 0;
		}

		/// <summary>
		///     Normalizes the given path so that differences in capitalisation
		///     or used slashes don't matter anymore.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		[Pure]
		public static string NormalizePath(string path)
		{
			if (string.IsNullOrEmpty(path))
				return path;

			Uri uri;
			if (!Uri.TryCreate(path, UriKind.Absolute, out uri))
				return path;

			return Path.GetFullPath(uri.LocalPath)
				.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
				.ToUpperInvariant();
		}
	}
}