namespace System.IO
{
	/// <summary>
	///     Equivalent to the <see cref="DirectoryInfo" /> class, but mockable.
	///     Used by <see cref="IFilesystem" />.
	/// </summary>
	public interface IDirectoryInfo
	{
		/// <summary>
		///     The name of the directory.
		/// </summary>
		string Name { get; }

		/// <summary>
		///     Whether or not the directory exists.
		/// </summary>
		bool Exists { get; }
	}
}