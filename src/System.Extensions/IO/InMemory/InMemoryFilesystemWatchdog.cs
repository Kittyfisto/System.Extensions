using System.Collections.Generic;

namespace System.IO.InMemory
{
	internal sealed class InMemoryFilesystemWatchdog
		: IFilesystemWatchdog
	{
		private readonly InMemoryFilesystem _filesystem;
		private readonly object _syncRoot;
		private readonly List<InMemoryFilesystemWatcher> _watchers;

		public InMemoryFilesystemWatchdog(InMemoryFilesystem filesystem)
		{
			_filesystem = filesystem;
			_syncRoot = new object();
			_watchers = new List<InMemoryFilesystemWatcher>();
		}

		public void NotifyWatchers()
		{
			lock (_syncRoot)
			{
				foreach (var watcher in _watchers) watcher.Update();
			}
		}

		#region Implementation of IFilesystemWatchdog

		public IFilesystemWatcher StartDirectoryWatch(string path,
		                                              string searchPattern = null,
		                                              SearchOption searchOption = SearchOption.TopDirectoryOnly)
		{
			var watcher = new InMemoryFilesystemWatcher(this, _filesystem, path, searchPattern, searchOption);
			lock (_syncRoot)
			{
				_watchers.Add(watcher);
			}

			return watcher;
		}

		public IFilesystemWatcher StartDirectoryWatch(string path,
		                                              TimeSpan maximumLatency,
		                                              string searchPattern = null,
		                                              SearchOption searchOption = SearchOption.TopDirectoryOnly)
		{
			return StartDirectoryWatch(path, searchPattern, searchOption);
		}

		public void StopWatch(IFilesystemWatcher filesystemWatcher)
		{
			lock (_syncRoot)
			{
				_watchers.Remove(filesystemWatcher as InMemoryFilesystemWatcher);
			}
		}

		#endregion
	}
}