using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
	internal sealed class InMemoryFileInfo
		: IFileInfoAsync
	{
		private readonly InMemoryFilesystem _filesystem;
		private readonly ISerialTaskScheduler _taskScheduler;
		private readonly string _name;
		private readonly string _fullPath;
		private readonly string _directoryPath;

		public InMemoryFileInfo(InMemoryFilesystem filesystem,
		                        ISerialTaskScheduler taskScheduler,
		                        string fullPath)
		{
			if (filesystem == null)
				throw new ArgumentNullException(nameof(filesystem));
			if (taskScheduler == null)
				throw new ArgumentNullException(nameof(taskScheduler));

			_filesystem = filesystem;
			_taskScheduler = taskScheduler;
			_fullPath = fullPath;
			_directoryPath = Path.GetDirectoryName(fullPath);
			_name = Path.GetFileName(fullPath);
		}

		public override int GetHashCode()
		{
			return new PathComparer().GetHashCode(_fullPath);
		}

		public override bool Equals(object obj)
		{
			var other = obj as InMemoryFileInfo;
			if (other == null)
				return false;

			return new PathComparer().Equals(_fullPath, other._fullPath);
		}

		public string Name => _name;

		public string FullPath => _fullPath;

		public Task<IFileInfo> Capture()
		{
			throw new NotImplementedException();
		}

		public Task<long> Length
		{
			get
			{
				return _taskScheduler.StartNew(() =>
				{
					InMemoryFile file;
					if (!_filesystem.TryGetFile(_fullPath, out file))
						throw new FileNotFoundException();

					return file.Length;
				});
			}
		}

		public Task<bool> IsReadOnly
		{
			get
			{
				return _taskScheduler.StartNew(() =>
				{
					InMemoryFile file;
					if (!_filesystem.TryGetFile(_name, out file))
						throw new FileNotFoundException();

					return file.IsReadOnly;
				});
			}
		}

		public Task<bool> Exists
		{
			get
			{
				return _taskScheduler.StartNew(() =>
				{
					InMemoryFile unused;
					return _filesystem.TryGetFile(_fullPath, out unused);
				});
			}
		}

		public Task<Stream> Create()
		{
			return _taskScheduler.StartNew(() =>
			{
				InMemoryDirectory directory;
				if (!_filesystem.TryGetDirectory(_directoryPath, out directory))
					throw new DirectoryNotFoundException();

				return directory.CreateFile(_name);
			});
		}

		public Task Delete()
		{
			return _taskScheduler.StartNew(() =>
			{
				InMemoryDirectory directory;
				if (_filesystem.TryGetDirectory(_directoryPath, out directory))
				{
					directory.TryDeleteFile(_name);
				}
			});
		}

		public override string ToString()
		{
			return "{" + FullPath + "}";
		}
	}
}