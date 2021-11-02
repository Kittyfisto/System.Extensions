namespace System.IO
{
	/// <summary>
	///     Represents a file.
	///     Properties such as <see cref="Exists" /> always return the value
	///     from when this object was created, for example through
	///     <see cref="IFilesystem.GetFileInfo" />.
	/// </summary>
	public interface IFileInfo
	{
		/// <summary>
		///     Gets an instance of the parent directory.
		/// </summary>
		IDirectoryInfo Directory { get; }

		/// <summary>
		///     Gets a string representing the directory's full path.
		/// </summary>
		string DirectoryName { get; }

		/// <summary>
		///     The name of the file in question.
		/// </summary>
		string Name { get; }

		/// <summary>
		///     The full file path of the file in question.
		/// </summary>
		string FullPath { get; }

		/// <summary>
		///     The length of the file in bytes at the time this object was created.
		/// </summary>
		long Length { get; }

		/// <summary>
		///     True when the file cannot be written to (at the time this object was created).
		/// </summary>
		bool IsReadOnly { get; }

		/// <summary>
		///     Whether or not the file existed at the time this object was created.
		/// </summary>
		bool Exists { get; }

		/// <summary>
		///     The time the file was created, in in coordinated universal time (UTC).
		/// </summary>
		DateTime CreationTimeUtc { get; }

		/// <summary>
		///     The time the file was last accessed, in in coordinated universal time (UTC).
		/// </summary>
		DateTime LastAccessTimeUtc { get; }

		/// <summary>
		///     The time the file was last written to, in in coordinated universal time (UTC).
		/// </summary>
		DateTime LastWriteTimeUtc { get; }

		/// <summary>
		///     Creates a file in a particular path.  If the file exists, it is replaced.
		///     The file is opened with ReadWrite accessand cannot be opened by another 
		///     application until it has been closed.  An IOException is thrown if the 
		///     directory specified doesn't exist.
		///
		///     Your application must have Create, Read, and Write permissions to
		///     the file.
		/// </summary>
		/// <returns></returns>
		Stream Create();

		/// <summary>
		///     Deletes this file if it exists.
		///     Does nothing if the file doesn't exist.
		/// </summary>
		/// <returns></returns>
		void Delete();
	}
}