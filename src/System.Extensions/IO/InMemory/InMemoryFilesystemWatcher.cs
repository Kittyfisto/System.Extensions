using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using log4net;

namespace System.IO.InMemory
{
	sealed class InMemoryFilesystemWatcher
		: IFilesystemWatcher
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly InMemoryFilesystemWatchdog _watchdog;
		private readonly InMemoryFilesystem _filesystem;
		private readonly string _searchPattern;
		private readonly SearchOption _searchOption;
		private readonly object _syncRoot;

		private IReadOnlyList<IFileInfo> _files;
		private string _path;

		public InMemoryFilesystemWatcher(InMemoryFilesystemWatchdog watchdog,
		                                 InMemoryFilesystem filesystem,
		                                 string path,
		                                 string searchPattern,
		                                 SearchOption searchOption)
		{
			_watchdog = watchdog;
			_filesystem = filesystem;
			_path = path;
			_searchPattern = searchPattern;
			_searchOption = searchOption;
			_syncRoot = new object();
			_files = new IFileInfo[0];

			Update();
		}

		public void Update()
		{
			var files = FindFiles();
			Synchronize(files);
		}

		#region Implementation of IFilesystemWatcher

		public IEnumerable<IFileInfo> Files
		{
			get { return _files; }
		}

#pragma warning disable 67
		public event Action Changed;
#pragma warning restore 67

		public string Path
		{
			get { return _path;}
			set
			{
				_path = value;
				Update();
			}
		}

		#endregion

		#region Implementation of IDisposable

		public void Dispose()
		{
			_watchdog.StopWatch(this);
		}

		#endregion

		[Pure]
		private IReadOnlyList<IFileInfo> FindFiles()
		{
			if (_filesystem.DirectoryExists(_path))
			{
				return _filesystem.EnumerateFiles(_path, _searchPattern, _searchOption).Select(x => _filesystem.GetFileInfo(x)).ToList();
			}

			return new IFileInfo[0];
		}

		private void Synchronize(IReadOnlyList<IFileInfo> files)
		{
			bool changed = false;

			lock (_syncRoot)
			{
				foreach (var file in files)
				{
					if (!_files.Contains(file))
					{
						changed = true;
						break;
					}
				}

				foreach (var file in _files)
				{
					if (!files.Contains(file))
					{
						changed = true;
						break;
					}
				}

				_files = files;
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
	}
}