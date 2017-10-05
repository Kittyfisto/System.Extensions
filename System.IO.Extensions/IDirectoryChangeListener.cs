namespace System.IO.Extensions
{
	/// <summary>
	///     This listener is invoked whenever a file in the watched directory is created.
	/// </summary>
	public interface IDirectoryChangeListener
	{
		/// <summary>
		/// </summary>
		/// <param name="file"></param>
		void OnFileCreated(string addedFile);

		/// <summary>
		/// </summary>
		/// <param name="file"></param>
		void OnFileRemoved(string file);
	}
}