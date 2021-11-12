using System.Collections.Generic;
using System.Threading;

namespace System.IO.FS
{
	internal sealed class FilesystemWatchdog
		: IFilesystemWatchdog
	{
		private readonly Filesystem _filesystem;
		private readonly ITaskScheduler _taskScheduler;
		private readonly object _syncRoot;
		private readonly List<FilesystemWatcher> _watchers;

		public FilesystemWatchdog(Filesystem filesystem, ITaskScheduler taskScheduler)
		{
			_filesystem = filesystem;
			_taskScheduler = taskScheduler;
			_syncRoot = new object();
			_watchers = new List<FilesystemWatcher>();
		}

		#region Implementation of IFilesystemWatchdog

		#endregion

		#region Implementation of IFilesystemWatchdog

		public IFilesystemWatcher StartDirectoryWatch(string path,string searchPattern = null, 
		                                              SearchOption searchOption = SearchOption.TopDirectoryOnly)
		{
			return StartDirectoryWatch(path, TimeSpan.FromMilliseconds(value: 500), searchPattern, searchOption);
		}

		public IFilesystemWatcher StartDirectoryWatch(string path,
		                                              TimeSpan maximumLatency,string searchPattern = null, 
		                                              SearchOption searchOption = SearchOption.TopDirectoryOnly)
		{
			var watcher = new FilesystemWatcher(this, _filesystem, _taskScheduler, maximumLatency,
			                                    path, searchPattern, searchOption);

			lock (_syncRoot)
			{
				_watchers.Add(watcher);
			}

			return watcher;
		}

		#endregion

		public void StopDirectoryWatch(FilesystemWatcher watcher)
		{
			lock (_syncRoot)
			{
				_watchers.Remove(watcher);
			}
		}
	}
}