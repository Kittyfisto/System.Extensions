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
		string CurrentDirectory { get; set; }

		/// <summary>
		///     An object representing the current directory.
		/// </summary>
		IDirectoryInfoAsync Current { get; }

		/// <summary>
		///     The current root directories (="drives") of this filesystem.
		/// </summary>
		Task<IEnumerable<IDirectoryInfoAsync>> Roots { get; }

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
		/// <param name="path"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		IDirectoryInfoAsync GetDirectoryInfo(string path);

		/// <summary>
		///     Returns an enumerable collection of file names in a specified path which match
		///     the specified pattern (if a pattern is specified).
		/// </summary>
		/// <param name="path"></param>
		/// <param name="searchPattern"></param>
		/// <param name="searchOption"></param>
		/// <param name="tolerateNonExistantPath">
		///     When set to true, then this method will never throw a
		///     <see cref="DirectoryNotFoundException" /> exception and instead return an empty enumeration
		/// </param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		Task<IReadOnlyList<string>> EnumerateFiles(string path,
			string searchPattern = null,
			SearchOption searchOption = SearchOption.TopDirectoryOnly,
			bool tolerateNonExistantPath = false);

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
		///     Opens a binary file, reads the contents of the file into a byte array,
		///     and then closes the file.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		Task<byte[]> ReadAllBytes(string path);

		/// <summary>
		///     Creates a file in a particular path.  If the file exists, it is replaced.
		///     The file is opened with ReadWrite accessand cannot be opened by another
		///     application until it has been closed.  An IOException is thrown if the
		///     directory specified doesn't exist.
		///     Your application must have Create, Read, and Write permissions to
		///     the file.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		Task<Stream> CreateFile(string path);

		/// <summary>
		///     Opens the given file for reading.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		Task<Stream> OpenRead(string path);

		/// <summary>
		///     Opens an existing file or creates a new file for writing.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		Task<Stream> OpenWrite(string path);

		/// <summary>
		///     Creates a new file, writes the specified stream from its current position, and then closes the file. If the target file already exists, it is overwritten.
		/// </summary>
		/// <remarks>
		///     The given buffer may not be modified until the task has finished.
		/// </remarks>
		/// <param name="path"></param>
		/// <param name="stream"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> or <paramref name="stream" /> is null</exception>
		Task Write(string path, Stream stream);

		/// <summary>
		///     Copies an existing file to a new file. Overwriting a file of the same name is not allowed.
		/// </summary>
		/// <param name="sourceFileName"></param>
		/// <param name="destFileName"></param>
		/// <returns></returns>
		Task CopyFile(string sourceFileName, string destFileName);

		/// <summary>
		///     Deletes the specified file.
		///     If the file does not exist, Delete succeeds without throwing
		///     an exception.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		Task DeleteFile(string path);

		#endregion
	}
}