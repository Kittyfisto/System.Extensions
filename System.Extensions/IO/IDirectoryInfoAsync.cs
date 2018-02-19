using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.IO
{
	/// <summary>
	///     Represents a directory on disk.
	///     Information (which may change) can only be queried asynchronously.
	///     Can be created through <see cref="IFilesystem.GetDirectoryInfo" />.
	/// </summary>
	public interface IDirectoryInfoAsync
	{
		/// <summary>
		///     Gets the root portion of the directory.
		/// </summary>
		IDirectoryInfoAsync Root { get; }

		/// <summary>
		///     Gets the parent directory of a specified subdirectory.
		/// </summary>
		/// <remarks>
		///     The parent directory, or null if the path is null or if the file path denotes
		///     a root (such as "\", "C:", or * "\\server\share").
		/// </remarks>
		IDirectoryInfoAsync Parent { get; }

		/// <summary>
		///     The name of the directory.
		/// </summary>
		string Name { get; }

		/// <summary>
		///     The full file path of the directory.
		/// </summary>
		string FullName { get; }

		/// <summary>
		///     Whether or not the directory exists.
		/// </summary>
		Task<bool> Exists { get; }

		/// <summary>
		///     Whether or not the given file exists.
		/// </summary>
		/// <param name="filename"></param>
		/// <returns></returns>
		Task<bool> FileExists(string filename);

		/// <summary>
		///     Captures and returns the current state/attributes of this directory.
		/// </summary>
		/// <returns></returns>
		Task<IDirectoryInfo> Capture();

		/// <summary>
		///     Returns an enumerable collection of file information in the current directory.
		/// </summary>
		Task<IEnumerable<IFileInfoAsync>> EnumerateFiles();

		/// <summary>
		///     Returns an enumerable collection of file information that matches a search pattern.
		/// </summary>
		/// <paramref name="searchPattern" />
		Task<IEnumerable<IFileInfoAsync>> EnumerateFiles(string searchPattern);

		/// <summary>
		///     Returns an enumerable collection of file information that matches a specified
		///     search pattern and search subdirectory option.
		/// </summary>
		/// <paramref name="searchPattern" />
		/// <paramref name="searchOption" />
		Task<IEnumerable<IFileInfoAsync>> EnumerateFiles(string searchPattern, SearchOption searchOption);

		/// <summary>
		///     Creates this directory.
		/// </summary>
		/// <returns></returns>
		Task Create();

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		Task Delete();

		/// <summary>
		///     Creates a subdirectory or subdirectories on the specified path.
		///     The specified path can be relative to this instance of the IDirectoryInfo
		///     class.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		Task<IDirectoryInfoAsync> CreateSubdirectory(string path);
	}
}
