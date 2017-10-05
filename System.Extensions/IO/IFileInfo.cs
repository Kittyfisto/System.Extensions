namespace System.IO
{
	/// <summary>
	///     Equivalent to the <see cref="FileInfo" /> class, but mockable.
	///     Used by <see cref="IFilesystem" />.
	/// </summary>
	public interface IFileInfo
	{
		/// <summary>
		///     The name of the file in question.
		/// </summary>
		string Name { get; }

		/// <summary>
		///     The length of the file in bytes.
		/// </summary>
		long Length { get; }

		/// <summary>
		///     True when the file cannot be written to.
		/// </summary>
		bool IsReadOnly { get; }

		/// <summary>
		///     Whether or not the file exists (is reachable).
		/// </summary>
		bool Exists { get; }
	}
}