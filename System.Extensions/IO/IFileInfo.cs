namespace System.IO
{
	/// <summary>
	///     Represents a file.
	///     Properties such as <see cref="Exists" /> always return the value
	///     from when this object was created, for example through
	///     <see cref="IFileInfoAsync.Capture" />.
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
	}
}