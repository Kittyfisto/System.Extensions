namespace System.IO
{
	/// <summary>
	///     Represents a directory.
	///     Properties such as <see cref="Exists" /> always return the value
	///     from when this object was created, for example through
	///     <see cref="IDirectoryInfoAsync.Capture" />.
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
	}
}