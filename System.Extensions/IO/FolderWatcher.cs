using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using log4net;

namespace System.IO
{
	/// <summary>
	///     Responsible for watching over changes to a single folder.
	/// </summary>
	internal sealed class FolderWatcher
		: IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly List<FileCreatedNotifier> _creationNotifiers;
		private readonly IFilesystem _filesystem;
		private readonly string _path;
		private readonly object _syncRoot;
		private readonly FileSystemWatcher _watcher;

		public FolderWatcher(IFilesystem filesystem, string path)
		{
			if (filesystem == null)
				throw new ArgumentNullException(nameof(filesystem));

			_filesystem = filesystem;
			_syncRoot = new object();
			_creationNotifiers = new List<FileCreatedNotifier>();

			_path = path;
			_watcher = new FileSystemWatcher(path);
			_watcher.Created += OnFileCreated;
			_watcher.EnableRaisingEvents = true;

			Log.DebugFormat("Starting filesystem watch on '{0}'...", _path);
		}

		/// <summary>
		/// </summary>
		/// <remarks>
		///     If a file is being created, but nobody listens....
		/// </remarks>
		public bool HasListeners
		{
			get
			{
				lock (_syncRoot)
				{
					return _creationNotifiers.Count > 0;
				}
			}
		}

		public void Dispose()
		{
			_watcher.Created -= OnFileCreated;
			_watcher.Dispose();

			Log.DebugFormat("Stopping filesystem watch on '{0}'...", _path);
		}

		public IFilesystemWatcher Add(IDirectoryChangeListener listener)
		{
			var notifier = new FileCreatedNotifier(_path, listener);

			lock (_syncRoot)
			{
				_creationNotifiers.Add(notifier);
			}

			return notifier;
		}

		public void Remove(IFilesystemWatcher filesystemWatcher)
		{
			lock (_syncRoot)
			{
				_creationNotifiers.Remove(filesystemWatcher as FileCreatedNotifier);
			}
		}

		public async Task Synchronise()
		{
			var files = await _filesystem.EnumerateFiles(_path);
			lock (_syncRoot)
			{
				foreach (var notifier in _creationNotifiers)
					notifier.Synchronise(files);
			}
		}

		private void OnFileCreated(object sender, FileSystemEventArgs args)
		{
			var fileName = args.FullPath;

			lock (_syncRoot)
			{
				foreach (var notifier in _creationNotifiers)
					notifier.OnFileCreated(fileName);
			}
		}
	}
}