using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.IO
{
	internal sealed class FilesystemWatchdog
		: IFilesystemWatchdog
	{
		private readonly object _syncRoot;
		private readonly IFilesystem _filesystem;
		private readonly Dictionary<string, FolderWatcher> _watches;

		public FilesystemWatchdog(IFilesystem filesystem)
		{
			if (filesystem == null)
				throw new ArgumentNullException(nameof(filesystem));

			_filesystem = filesystem;
			_syncRoot = new object();
			_watches = new Dictionary<string, FolderWatcher>();
		}

		/// <inheritdoc />
		public IFilesystemWatcher StartDirectoryWatch(string path, IDirectoryChangeListener listener)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			path = CaptureFullPath(path);
			var watch = GetOrCreateWatch(path);
			return watch.Add(listener);
		}

		private FolderWatcher GetOrCreateWatch(string path)
		{
			lock (_syncRoot)
			{
				FolderWatcher watcher;
				if (!_watches.TryGetValue(path, out watcher))
				{
					watcher = new FolderWatcher(_filesystem, path);
					_watches.Add(path, watcher);
				}
				return watcher;
			}
		}

		/// <inheritdoc />
		public void StopWatch(IFilesystemWatcher filesystemWatcher)
		{
			lock (_syncRoot)
			{
				var path = filesystemWatcher.Path;
				FolderWatcher watcher;
				if (path != null && _watches.TryGetValue(path, out watcher))
				{
					watcher.Remove(filesystemWatcher);
					if (!watcher.HasListeners)
					{
						_watches.Remove(path);
						watcher.Dispose();
					}
				}
			}
		}

		[Pure]
		private string CaptureFullPath(string path)
		{
			// We want to ensure that Directory.CurrentDirectory is captured on the calling thread.
			// If we don't, then:
			// 1. GetDirectoryInfo()
			// 2. Directory.SetCurrentDirectory()
			// will NOT behave in a deterministic fashion.

			if (!Path.IsPathRooted(path))
			{
				var current = Directory.GetCurrentDirectory();
				var abs = Path.Combine(current, path);
				return abs;
			}

			return path;
		}
	}
}