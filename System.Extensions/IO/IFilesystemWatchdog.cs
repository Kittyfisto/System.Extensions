namespace System.IO
{
	internal interface IFilesystemWatchdog
	{
		/// <summary>
		///     Creates a new filesystem-watch which notifies the given listener about the creation/deleting of files.
		/// </summary>
		/// <remarks>
		///     The given listener will continue to be notified until the returned watch is disposed of.
		/// </remarks>
		/// <param name="path"></param>
		/// <param name="listener"></param>
		/// <returns></returns>
		IFilesystemWatcher StartDirectoryWatch(string path, IDirectoryChangeListener listener);

		/// <summary>
		///     Stops the given watch.
		///     Should be called when one is no longer interested in being notified of file related
		///     events (in order to free-up system resources).
		/// </summary>
		/// <param name="filesystemWatcher"></param>
		void StopWatch(IFilesystemWatcher filesystemWatcher);
	}

	
}
