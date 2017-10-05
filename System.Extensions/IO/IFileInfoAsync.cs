using System.Threading.Tasks;

namespace System.IO
{
	/// <summary>
	///     Represents a file on disk.
	///     All changeable information about the file may only be queried asynchronously.
	///     Can be created through <see cref="IFilesystem.GetFileInfo"/>.
	/// </summary>
	public interface IFileInfoAsync
	{
		/// <summary>
		///     The name of the file in question.
		/// </summary>
		string Name { get; }

		/// <summary>
		///     The full file path of the file in question.
		/// </summary>
		string FullPath { get; }

		/// <summary>
		///     The length of the file in bytes.
		/// </summary>
		Task<long> Length { get; }

		/// <summary>
		///     True when the file cannot be written to.
		/// </summary>
		Task<bool> IsReadOnly { get; }

		/// <summary>
		///     Whether or not the file exists (is reachable).
		/// </summary>
		Task<bool> Exists { get; }
	}
}