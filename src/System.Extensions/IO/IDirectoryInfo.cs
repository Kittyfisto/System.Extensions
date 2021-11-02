using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace System.IO
{
	/// <summary>
	///     Represents a directory.
	///     Properties such as <see cref="Exists" /> always return the value
	///     from when this object was created, for example through
	///     <see cref="Capture" />.
	/// </summary>
	public interface IDirectoryInfo
	{
		/// <summary>
		///     Gets the root portion of the directory.
		/// </summary>
		/// <remarks>
		/// </remarks>
		IDirectoryInfo Root { get; }

		/// <summary>
		///     Gets the parent directory of a specified subdirectory.
		/// </summary>
		/// <remarks>
		///     The parent directory, or null if the path is null or if the file path denotes
		///     a root (such as "\", "C:", or * "\\server\share").
		/// </remarks>
		IDirectoryInfo Parent { get; }

		/// <summary>
		///     The name of the directory.
		/// </summary>
		string Name { get; }

		/// <summary>
		///     The full file path of the directory.
		/// </summary>
		string FullName { get; }

		/// <summary>
		///     Whether or not the directory existed at the time the snapshot was taken.
		/// </summary>
		bool Exists { get; }

		/// <summary>
		///     Whether or not the given file exists.
		/// </summary>
		/// <param name="filename"></param>
		/// <returns></returns>
		bool FileExists(string filename);

		/// <summary>
		///     Returns an enumerable collection of file information in the current directory.
		/// </summary>
		IEnumerable<IFileInfo> EnumerateFiles();

		/// <summary>
		///     Returns an enumerable collection of file information that matches a search pattern.
		/// </summary>
		/// <paramref name="searchPattern" />
		IEnumerable<IFileInfo> EnumerateFiles(string searchPattern);

		/// <summary>
		///     Returns an enumerable collection of file information that matches a specified
		///     search pattern and search subdirectory option.
		/// </summary>
		/// <paramref name="searchPattern" />
		/// <paramref name="searchOption" />
		IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption);

		/// <summary>
		///     Creates this directory.
		/// </summary>
		/// <returns></returns>
		void Create();

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		void Delete();

		/// <summary>
		///     Creates a subdirectory or subdirectories on the specified path.
		///     The specified path can be relative to this instance of the IDirectoryInfo
		///     class.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		IDirectoryInfo CreateSubdirectory(string path);
	}
}