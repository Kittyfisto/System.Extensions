namespace System.IO
{
	/// <summary>
	///     This listener is invoked whenever a file in the watched directory is created.
	/// </summary>
	internal interface IDirectoryChangeListener
	{
		/// <summary>
		/// </summary>
		/// <param name="file"></param>
		void OnFileCreated(string file);

		/// <summary>
		/// </summary>
		/// <param name="file"></param>
		void OnFileRemoved(string file);
	}
}