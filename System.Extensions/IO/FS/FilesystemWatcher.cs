using System.Collections.Generic;
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
		private IReadOnlyList<IFileInfoAsync> _files;
		private readonly FilesystemWatchdog _filesystemWatchdog;
		private readonly Filesystem _filesystem;
		private readonly string _path;
		private readonly IPeriodicTask _task;
		private readonly ITaskScheduler _taskScheduler;
		private readonly SearchOption _searchOption;
		private readonly object _syncRoot;

		public FilesystemWatcher(FilesystemWatchdog filesystemWatchdog,
		                         Filesystem filesystem,
		                         ITaskScheduler taskScheduler,
		                         TimeSpan maximumLatency,
		                         string path,
		                         SearchOption searchOption)
		{
			_filesystemWatchdog = filesystemWatchdog;
			_filesystem = filesystem;
			_taskScheduler = taskScheduler;

			_syncRoot = new object();

			_path = path;
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
			try
			{
				var filePaths = _filesystem.EnumerateFiles(_path, searchPattern: null,
				                                           searchOption: _searchOption,
				                                           tolerateNonExistantPath: true).Result;
				Synchronize(filePaths);
			}
			catch (IOException e)
			{
				Console.WriteLine(e);
				throw;
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
		}

#pragma warning disable 67
		public event Action Changed;
#pragma warning restore 67

		#endregion
	}
}