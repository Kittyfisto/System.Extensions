namespace System.IO
{
	/// <summary>
	/// 
	/// </summary>
	public interface IFilesystemWatchdog
	{
		/// <summary>
		///     Creates a new filesystem-watch which notifies the given listener about the creation/deleting of files.
		/// </summary>
		/// <remarks>
		///     The given listener will continue to be notified until the returned watch is disposed of.
		/// </remarks>
		/// <remarks>
		///     The directory watch WILL CONSUME RESOURCES until it is disposed of.
		///     You MUST dispose of the returned object when you no longer need to watch for changes or
		///     memory and CPU time will be wasted until the AppDomain is unloaded.
		/// </remarks>
		/// <param name="path"></param>
		/// <param name="searchPattern"></param>
		/// <param name="searchOption"></param>
		/// <returns></returns>
		IFilesystemWatcher StartDirectoryWatch(string path, string searchPattern = null, SearchOption searchOption = SearchOption.TopDirectoryOnly);

		/// <summary>
		///     Creates a new filesystem-watch which notifies the given listener about the creation/deleting of files.
		/// </summary>
		/// <remarks>
		///     The given listener will continue to be notified until the returned watch is disposed of.
		/// </remarks>
		/// <remarks>
		///     The directory watch WILL CONSUME RESOURCES until it is disposed of.
		///     You MUST dispose of the returned object when you no longer need to watch for changes or
		///     memory and CPU time will be wasted until the AppDomain is unloaded.
		/// </remarks>
		/// <param name="path"></param>
		/// <param name="maximumLatency">
		///     The maximum amount of latency with which updates are perceived.
		///     Shouldn't be set to less than 100ms.
		/// </param>
		/// <param name="searchPattern"></param>
		/// <param name="searchOption"></param>
		/// <returns></returns>
		IFilesystemWatcher StartDirectoryWatch(string path, TimeSpan maximumLatency, string searchPattern = null, SearchOption searchOption = SearchOption.TopDirectoryOnly);
	}
}
