using System.Diagnostics.Contracts;

namespace System.IO
{
	internal static class Directory2
	{
		public static readonly string DirectorySeparatorChar = Path.DirectorySeparatorChar.ToString();

		/// <summary>
		/// See http://referencesource.microsoft.com/#mscorlib/system/io/directoryinfo.cs,e3b20cb1c28ea93f
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
				{
					s = fullPath.Substring(0, fullPath.Length - 1);
				}
				dirName = Path.GetFileName(s);
			}
			else
			{
				dirName = fullPath;  // For rooted paths, like "c:\"
			}
			return dirName;
		}
	}
}
