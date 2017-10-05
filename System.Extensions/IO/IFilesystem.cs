using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.IO
{
	/// <summary>
	///     Interface to interact with the filesystem in an asynchronuos fashion only.
	/// </summary>
	public interface IFilesystem
	{
		#region Directories

		/// <summary>
		///     The current directory, used when relative paths are given to any of these methods.
		/// </summary>
		/// <remarks>
		///     **Always** equals <see cref="Directory.GetCurrentDirectory"/>.
		/// </remarks>
		string CurrentDirectory { get; set; }

		/// <summary>
		///     Creates the given directory if it doesn't exist yet.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		Task<IDirectoryInfoAsync> CreateDirectory(string path);

		/// <summary>
		///     Deletes an empty directory from a specified path.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		Task DeleteDirectory(string path);

		/// <summary>
		///     Deletes the specified directory and, if indicated, any subdirectories and files
		///     in the directory.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="recursive"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		Task DeleteDirectory(string path, bool recursive);

		/// <summary>
		///     Tests if a directory with the given path exists.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		Task<bool> DirectoryExists(string path);

		/// <summary>
		///     Obtains information about the given directory.
		/// </summary>
		/// <param name="directoryName"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="directoryName" /> is null</exception>
		IDirectoryInfoAsync GetDirectoryInfo(string directoryName);

		/// <summary>
		///     Returns an enumerable collection of file names in a specified path.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		Task<IReadOnlyList<string>> EnumerateFiles(string path);

		/// <summary>
		///     Returns an enumerable collection of file names in a specified path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="searchPattern"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		Task<IReadOnlyList<string>> EnumerateFiles(string path, string searchPattern);

		/// <summary>
		///     Returns an enumerable collection of file names in a specified path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="searchPattern"></param>
		/// <param name="searchOption"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		Task<IReadOnlyList<string>> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);

		/// <summary>
		///     Returns an enumerable collection of directory names in a specified path.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		Task<IReadOnlyList<string>> EnumerateDirectories(string path);

		/// <summary>
		///     Returns an enumerable collection of directory names in a specified path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="searchPattern"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		Task<IReadOnlyList<string>> EnumerateDirectories(string path, string searchPattern);

		/// <summary>
		///     Returns an enumerable collection of directory names in a specified path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="searchPattern"></param>
		/// <param name="searchOption"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		Task<IReadOnlyList<string>> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption);

		#endregion

		#region Files

		/// <summary>
		///     Obtains information about the given file.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		IFileInfoAsync GetFileInfo(string fileName);

		/// <summary>
		///     Tests if a file with the given path exists.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		Task<bool> FileExists(string path);

		/// <summary>
		///     The length of a file (in bytes).
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		Task<long> FileLength(string path);

		/// <summary>
		///     Whether or not the given file is readonly.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		Task<bool> IsFileReadOnly(string path);

		/// <summary>
		///     Writes the given data to the given file.
		/// </summary>
		/// <remarks>
		///     This method copies the given buffer before writing to the file on the I/O thread.
		/// </remarks>
		/// <param name="path"></param>
		/// <param name="bytes"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> or <paramref name="bytes" /> is null</exception>
		Task WriteAllBytes(string path, byte[] bytes);

		/// <summary>
		///     Opens the given file for reading.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		Task<Stream> OpenRead(string path);

		/// <summary>
		///     Opens the given file for writing.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		Task<Stream> OpenWrite(string path);

		/// <summary>
		///     Reads the given stream (FROM ITS CURRENT POSITION) to its end and writes the content to a file.
		/// </summary>
		/// <remarks>
		///     This method copies the given buffer before writing to the file on the I/O thread.
		/// </remarks>
		/// <param name="path"></param>
		/// <param name="stream"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> or <paramref name="stream" /> is null</exception>
		Task Write(string path, Stream stream);

		/// <summary>
		///     Deletes the specified file.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		Task DeleteFile(string path);

		#endregion
	}
}