using System.Collections.Generic;
using System.Reflection;
using log4net;

namespace System.IO
{
	/// <summary>
	///     Responsible for notifying a single <see cref="IDirectoryChangeListener" />
	///     about file creations. Handles (some) faults produced by the actual listener
	///     implementation.
	/// </summary>
	internal sealed class FileCreatedNotifier
		: IFilesystemWatch
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly object _syncRoot;
		private readonly string _path;
		private readonly HashSet<string> _currentFiles;
		private readonly List<string> _pendingNotifications;
		private readonly IDirectoryChangeListener _listener;

		public FileCreatedNotifier(string path, IDirectoryChangeListener listener)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (listener == null)
				throw new ArgumentNullException(nameof(listener));

			_path = path;
			_syncRoot = new object();
			_listener = listener;
			_currentFiles = new HashSet<string>();
			_pendingNotifications = new List<string>();
		}

		public string Path => _path;

		public void OnFileCreated(string fileName)
		{
			lock (_syncRoot)
				if (AddFile(fileName))
					NotifyListener();
		}

		public void Synchronise(IEnumerable<string> currentFiles)
		{
			lock (_syncRoot)
			{
				foreach (var file in currentFiles)
				{
					AddFile(file);
				}

				NotifyListener();
			}
		}

		private bool AddFile(string file)
		{
			if (!_currentFiles.Contains(file))
			{
				_pendingNotifications.Add(file);
				_currentFiles.Add(file);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Notifies the listener about all pending changes.
		/// </summary>
		private void NotifyListener()
		{
			foreach (var file in _pendingNotifications)
			{
				TryNotifyListener(file);
			}
			_pendingNotifications.Clear();
		}

		/// <summary>
		/// Notifies the listener that the given file has been added.
		/// </summary>
		/// <param name="file"></param>
		/// <returns>False when <see cref="IDirectoryChangeListener.OnFileCreated"/> threw an exception, true otherwise</returns>
		private bool TryNotifyListener(string file)
		{
			try
			{
				_listener.OnFileCreated(file);
				return true;
			}
			catch (Exception e)
			{
				Log.DebugFormat("Listener {0} threw an exception while being notified of file creation '{1}': {2}",
					_listener,
					file,
					e);
				return false;
			}
		}
	}
}