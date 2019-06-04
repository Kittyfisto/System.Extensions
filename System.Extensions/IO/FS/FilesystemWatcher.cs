using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;

namespace System.IO.FS
{
	internal sealed class FilesystemWatcher
		: IFilesystemWatcher
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Dictionary<string, IFileInfoAsync> _filesByPath;
		private readonly FilesystemWatchdog _filesystemWatchdog;
		private readonly Filesystem _filesystem;
		private readonly IPeriodicTask _task;
		private readonly ITaskScheduler _taskScheduler;
		private readonly SearchOption _searchOption;
		private readonly object _syncRoot;
		private readonly string _searchPattern;

		private IReadOnlyList<IFileInfoAsync> _files;
		private string _path;

		public FilesystemWatcher(FilesystemWatchdog filesystemWatchdog,
		                         Filesystem filesystem,
		                         ITaskScheduler taskScheduler,
		                         TimeSpan maximumLatency,
		                         string path,
		                         string searchPattern,
		                         SearchOption searchOption)
		{
			_filesystemWatchdog = filesystemWatchdog;
			_filesystem = filesystem;
			_taskScheduler = taskScheduler;

			_syncRoot = new object();

			_path = path;
			_searchPattern = searchPattern;
			_searchOption = searchOption;
			_filesByPath = new Dictionary<string, IFileInfoAsync>();
			_files = new IFileInfoAsync[0];

			_task = _taskScheduler.StartPeriodic(Update, maximumLatency, $"FilesystemWatcher({path})");
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			_taskScheduler.StopPeriodic(_task);
			_filesystemWatchdog.StopDirectoryWatch(this);
		}

		#endregion

		private void Update()
		{
			EnumerateOnce();
		}

		private void EnumerateOnce()
		{
			var filePaths = EnumerateFiles();
			Synchronize(filePaths);
		}

		[Pure]
		private IReadOnlyList<string> EnumerateFiles()
		{
			try
			{
				return _filesystem.EnumerateFiles(_path, searchPattern: _searchPattern,
				                                  searchOption: _searchOption,
				                                  tolerateNonExistantPath: true).Result;
			}
			catch (IOException e)
			{
				Log.DebugFormat("Caught exception: {0}", e);
				return new string[0];
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
				return new string[0];
			}
		}

		private void Synchronize(IReadOnlyList<string> filePaths)
		{
			bool changed = false;

			lock (_syncRoot)
			{
				foreach (var path in filePaths)
				{
					if (!_filesByPath.ContainsKey(path))
					{
						_filesByPath.Add(path, _filesystem.GetFileInfo(path));
						changed = true;
					}
				}

				foreach (var path in _filesByPath.Keys.ToList())
				{
					if (!filePaths.Contains(path))
					{
						_filesByPath.Remove(path);
						changed = true;
					}
				}

				if (changed)
				{
					_files = _filesByPath.Values.ToList();
				}
			}

			if (changed)
				EmitChanged();
		}

		private void EmitChanged()
		{
			try
			{
				Changed?.Invoke();
			}
			catch (Exception e)
			{
				Log.ErrorFormat("Caught unexpected exception: {0}", e);
			}
		}

		#region Implementation of IFilesystemWatcher

		public IEnumerable<IFileInfoAsync> Files
		{
			get { return _files; }
		}

		public string Path
		{
			get { return _path; }
			set
			{
				_path = value;
				EnumerateOnce();
			}
		}

#pragma warning disable 67
		public event Action Changed;
#pragma warning restore 67

		#endregion
	}
}