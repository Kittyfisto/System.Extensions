using System.Collections.Generic;
using System.Security;
using System.Text;

namespace System.IO
{
	/// <summary>
	///     Interface to interact with the filesystem in an asynchronuos fashion only.
	/// </summary>
	public interface IFilesystem
	{
		#region Watching

		/// <summary>
		///   Allows monitoring of folders.
		/// </summary>
		IFilesystemWatchdog Watchdog { get; }

		#endregion

		#region Directories

		/// <summary>
		///     The current directory, used when relative paths are given to any of these methods.
		/// </summary>
		string CurrentDirectory { get; set; }

		/// <summary>
		///     An object representing the current directory.
		/// </summary>
		IDirectoryInfo Current { get; }

		/// <summary>
		///     The current root directories (="drives") of this filesystem.
		/// </summary>
		IEnumerable<IDirectoryInfo> Roots { get; }

		/// <summary>
		///     Creates the given directory if it doesn't exist yet.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		IDirectoryInfo CreateDirectory(string path);

		/// <summary>
		///     Deletes an empty directory from a specified path.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		void DeleteDirectory(string path);

		/// <summary>
		///     Deletes the specified directory and, if indicated, any subdirectories and files
		///     in the directory.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="recursive"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		void DeleteDirectory(string path, bool recursive);

		/// <summary>
		///     Determines whether the given path refers to an existing directory on disk.
		/// </summary>
		/// <param name="path">The path to test.</param>
		/// <returns></returns>
		bool DirectoryExists(string path);

		/// <summary>
		///     Obtains information about the given directory.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		IDirectoryInfo GetDirectoryInfo(string path);

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
		IReadOnlyList<string> EnumerateFiles(string path,
			string searchPattern = null,
			SearchOption searchOption = SearchOption.TopDirectoryOnly,
			bool tolerateNonExistantPath = false);

		/// <summary>
		///     Returns an enumerable collection of directory names in a specified path.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		IReadOnlyList<string> EnumerateDirectories(string path);

		/// <summary>
		///     Returns an enumerable collection of directory names in a specified path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="searchPattern"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		IReadOnlyList<string> EnumerateDirectories(string path, string searchPattern);

		/// <summary>
		///     Returns an enumerable collection of directory names in a specified path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="searchPattern"></param>
		/// <param name="searchOption"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		IReadOnlyList<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption);

		#endregion

		#region Files

		/// <summary>
		///     Obtains information about the given file.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"><paramref name="fileName"/> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by InvalidPathChars.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="fileName"/> is null.</exception>
		/// <exception cref="PathTooLongException">The specified <paramref name="fileName"/> exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
		IFileInfo GetFileInfo(string fileName);

		/// <summary>
		///     Determines whether the specified file exists.
		/// </summary>
		/// <param name="path">The file to check.</param>
		/// <returns></returns>
		bool FileExists(string path);

		/// <summary>
		///     The length of a file (in bytes).
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		long FileLength(string path);

		/// <summary>
		///     Whether or not the given file is readonly.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		bool IsFileReadOnly(string path);

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
		/// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.
		/// -or-
		/// <paramref name="path"/> specified a file that is read-only.</exception>
		/// <exception cref="ArgumentException"><paramref name="path"/> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by InvalidPathChars.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
		/// <exception cref="PathTooLongException">The specified <paramref name="path"/> exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
		/// <exception cref="DirectoryNotFoundException">The specified <paramref name="path"/> is invalid, (for example, it is on an unmapped drive).</exception>
		/// <exception cref="IOException">An I/O error occurred while creating the file.</exception>
		/// <exception cref="NotSupportedException"><paramref name="path"/> is in an invalid format.</exception>
		Stream CreateFile(string path);

		/// <summary>
		///     Opens the given file for reading.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"><paramref name="path"/> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by InvalidPathChars.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
		/// <exception cref="PathTooLongException">The specified <paramref name="path"/> exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
		/// <exception cref="DirectoryNotFoundException">The specified <paramref name="path"/> is invalid, (for example, it is on an unmapped drive).</exception>
		/// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a directory.
		/// -or-
		/// The caller does not have the required permission.</exception>
		/// <exception cref="FileNotFoundException">The file specified in <paramref name="path"/> was not found.</exception>
		/// <exception cref="NotSupportedException"><paramref name="path"/> is in an invalid format.</exception>
		/// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
		Stream OpenRead(string path);

		/// <summary>
		///     Opens an existing file or creates a new file for writing.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		/// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.
		/// -or-
		/// path specified a read-only file or directory.
		/// </exception>
		/// <exception cref="ArgumentException"><paramref name="path"/> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by InvalidPathChars.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
		/// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
		/// <exception cref="DirectoryNotFoundException">The specified path is invalid, (for example, it is on an unmapped drive).</exception>
		/// <exception cref="NotSupportedException"><paramref name="path"/> is in an invalid format.</exception>
		Stream OpenWrite(string path);

		/// <summary>
		///     Creates a new file, writes the specified stream from its current position, and then closes the file. If the target file already exists, it is overwritten.
		/// </summary>
		/// <remarks>
		///     The given buffer may not be modified until the task has finished.
		/// </remarks>
		/// <param name="path"></param>
		/// <param name="stream"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"><paramref name="path"/> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by InvalidPathChars.</exception>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> or <paramref name="stream" /> is null</exception>
		/// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
		/// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
		/// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
		/// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is read-only.
		/// -or-
		/// This operation is not supported on the current platform.
		/// -or-
		/// path specified a directory.
		/// -or-
		/// The caller does not have the required permission.</exception>
		/// <exception cref="NotSupportedException"><paramref name="path"/> is in an invalid format.</exception>
		/// <exception cref="SecurityException">The caller does not have the required permission.</exception>
		void Write(string path, Stream stream);

		/// <summary>
		///     Writes the given data to the given file.
		/// </summary>
		/// <remarks>
		///     This method copies the given buffer before writing to the file on the I/O thread.
		/// </remarks>
		/// <param name="path"></param>
		/// <param name="bytes"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"><paramref name="path"/> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by InvalidPathChars.</exception>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> or <paramref name="bytes" /> is null</exception>
		/// <exception cref="PathTooLongException">The specified <paramref name="path"/> exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
		/// <exception cref="DirectoryNotFoundException">The specified <paramref name="path"/> is invalid (for example, it is on an unmapped drive).</exception>
		/// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
		/// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a file that is read-only.
		/// -or-
		/// This operation is not supported on the current platform.
		/// -or-
		/// path specified a directory.
		/// -or-
		/// The caller does not have the required permission.</exception>
		/// <exception cref="NotSupportedException"><paramref name="path"/> is in an invalid format.</exception>
		/// <exception cref="SecurityException">The caller does not have the required permission.</exception>
		void WriteAllBytes(string path, byte[] bytes);

		/// <summary>
		///    Creates a new file, writes the specified string to the file, and then closes the file.
		///    If the target file already exists, it is overwritten.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="contents"></param>
		void WriteAllText(string path, string contents);

		/// <summary>
		///    Creates a new file, writes the specified string to the file using the specified encoding,
		///    and then closes the file. If the target file already exists, it is overwritten.
		/// </summary>
		/// <param name="path">The file to write to.</param>
		/// <param name="contents">The string to write to the file.</param>
		/// <param name="encoding">The encoding to apply to the string.</param>
		void WriteAllText(string path, string contents, Encoding encoding);

		/// <summary>
		///     Opens a binary file, reads the contents of the file into a byte array,
		///     and then closes the file.
		/// </summary>
		/// <param name="path">The file to open for reading.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">path is a zero-length string, contains only white space, or contains one or more invalid characters as defined by InvalidPathChars.</exception>
		/// <exception cref="ArgumentNullException">When <paramref name="path" /> is null</exception>
		/// <exception cref="PathTooLongException">The specified <paramref name="path"/> exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
		/// <exception cref="DirectoryNotFoundException">The specified <paramref name="path"/> is invalid (for example, it is on an unmapped drive).</exception>
		/// <exception cref="IOException">An I/O error occurred while opening the file.</exception>
		/// <exception cref="UnauthorizedAccessException"><paramref name="path"/> specified a directory.
		/// -or-
		/// The caller does not have the required permission.</exception>
		/// <exception cref="FileNotFoundException">The file specified in <paramref name="path"/> was not found.</exception>
		/// <exception cref="NotSupportedException"><paramref name="path"/> is in an invalid format.</exception>
		/// <exception cref="SecurityException">The caller does not have the required permission.</exception>
		byte[] ReadAllBytes(string path);

		/// <summary>
		///    Opens a text file, reads all the text in the file into a string, and then closes the file.
		/// </summary>
		/// <param name="path">The file to open for reading.</param>
		string ReadAllText(string path);

		/// <summary>
		///    Opens a file, reads all text in the file with the specified encoding, and then closes the file.
		/// </summary>
		/// <param name="path">The file to open for reading.</param>
		/// <param name="encoding">The encoding applied to the contents of the file.</param>
		string ReadAllText(string path, Encoding encoding);

		/// <summary>
		///    Opens a text file, reads all lines of the file, and then closes the file.
		/// </summary>
		/// <param name="path">The file to open for reading.</param>
		/// <returns></returns>
		IReadOnlyList<string> ReadAllLines(string path);

		/// <summary>
		///    Opens a file, reads all lines of the file with the specified encoding, and then closes the file.
		/// </summary>
		/// <param name="path">The file to open for reading.</param>
		/// <param name="encoding">The encoding applied to the contents of the file.</param>
		/// <returns></returns>
		IReadOnlyList<string> ReadAllLines(string path, Encoding encoding);

		/// <summary>
		///     Copies an existing file to a new file. Overwriting a file of the same name is not allowed.
		/// </summary>
		/// <param name="sourceFileName">The file to copy.</param>
		/// <param name="destFileName">The name of the destination file. This cannot be a directory or an existing file.</param>
		/// <returns></returns>
		/// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.</exception>
		/// <exception cref="ArgumentException"><paramref name="sourceFileName"/> or <paramref name="destFileName"/> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by InvalidPathChars.
		/// -or-
		/// <paramref name="sourceFileName"/> or <paramref name="destFileName"/> specifies a directory.</exception>
		/// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
		/// <exception cref="DirectoryNotFoundException">The path specified in <paramref name="sourceFileName"/> or <paramref name="destFileName"/> is invalid (for example, it is on an unmapped drive).</exception>
		/// <exception cref="FileNotFoundException"><paramref name="sourceFileName"/> was not found.</exception>
		/// <exception cref="IOException"><paramref name="destFileName"/> exists</exception>
		/// <exception cref="NotSupportedException"><paramref name="sourceFileName"/> or <paramref name="destFileName"/> is in an invalid format.</exception>
		void CopyFile(string sourceFileName, string destFileName);

		/// <summary>
		///     Deletes the specified file.
		///     If the file does not exist, Delete succeeds without throwing
		///     an exception.
		/// </summary>
		/// <param name="path">The name of the file to be deleted. Wildcard characters are not supported.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"><paramref name="path"/> is a zero-length string, contains only white space, or contains one or more invalid characters as defined by InvalidPathChars.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
		/// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
		/// <exception cref="IOException">The specified file is in use.
		/// -or-
		/// There is an open handle on the file, and the operating system is Windows XP or earlier. This open handle can result from enumerating directories and files. For more information, see How to: Enumerate Directories and Files.</exception>
		/// <exception cref="NotSupportedException"><paramref name="path"/> is in an invalid format.</exception>
		/// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.</exception>
		/// <exception cref="UnauthorizedAccessException">The caller does not have the required permission.
		/// -or-
		/// The file is an executable file that is in use.
		/// -or-
		/// path is a directory.
		/// -or-
		/// path specified a read-only file.</exception>
		void DeleteFile(string path);

		#endregion
	}
}