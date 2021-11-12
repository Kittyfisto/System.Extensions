using System.Collections.Generic;

namespace System.IO
{
	/// <summary>
	///     Monitors one folder(tree) for changes.
	/// </summary>
	public interface IFilesystemWatcher
		: IDisposable
	{
		/// <summary>
		///     The current listing of files in the given folder(tree).
		/// </summary>
		IEnumerable<IFileInfo> Files { get; }

		/// <summary>
		///     The (root) path being monitored.
		/// </summary>
		string Path { get; set; }

		/// <summary>
		///     This event is fired whenever changes to the given <see cref="Path" /> occurred.
		/// </summary>
		event Action Changed;
	}
}