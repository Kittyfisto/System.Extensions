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
	}
}