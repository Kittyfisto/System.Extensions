using System.Security;

namespace System.IO
{
	internal static class Path2
	{
		public static void CheckInvalidPathChars(string path, bool checkAdditional = false)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			if (HasIllegalCharacters(path, checkAdditional))
				throw new ArgumentException("Argument_InvalidPathChars");
		}

		/// <summary>
		///     Returns a value indicating if the given path contains invalid characters (", &lt;, &gt;, |
		///     NUL, or any ASCII char whose integer representation is in the range of 1 through 31).
		///     Does not check for wild card characters ? and *.
		/// </summary>
		public static bool HasIllegalCharacters(string path, bool checkAdditional = false)
		{
			return AnyPathHasIllegalCharacters(path, checkAdditional);
		}

		/// <summary>
		///     Tests if the given path is well formed and MIGHT point to a directory / file.
		///     Does not check for the presence of any dirve and/or directory.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool IsValidPath(string path)
		{
			try
			{
				Path.GetFullPath(path);
				return true;
			}
			catch (ArgumentException)
			{
				return false;
			}
			catch (SecurityException)
			{
				return false;
			}
			catch (NotSupportedException)
			{
				return false;
			}
			catch (PathTooLongException)
			{
				return false;
			}
		}

		/// <summary>
		/// Throws an <see cref="ArgumentException"/> if the given path is invalid
		/// </summary>
		/// <param name="path"></param>
		public static void ThrowIfPathIsInvalid(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			if (!IsValidPath(path))
				throw new ArgumentException(string.Format("The path '{0}' is invalid", path), nameof(path));
		}

		/// <summary>
		///     Version of HasIllegalCharacters that checks no AppContextSwitches. Only use if you know you need to skip switches
		///     and don't care
		///     about proper device path handling.
		/// </summary>
		private static bool AnyPathHasIllegalCharacters(string path, bool checkAdditional = false)
		{
			return path.IndexOfAny(Path.GetInvalidPathChars()) >= 0 || checkAdditional && AnyPathHasWildCardCharacters(path);
		}

		/// <summary>
		///     Version of HasWildCardCharacters that checks no AppContextSwitches. Only use if you know you need to skip switches
		///     and don't care
		///     about proper device path handling.
		/// </summary>
		private static bool AnyPathHasWildCardCharacters(string path, int startIndex = 0)
		{
			char currentChar;
			for (var i = startIndex; i < path.Length; i++)
			{
				currentChar = path[i];
				if (currentChar == '*' || currentChar == '?') return true;
			}
			return false;
		}
	}
}