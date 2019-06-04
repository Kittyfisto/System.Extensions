using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO.InMemory
{
	/// <summary>
	/// This class is responsible for pro
	/// </summary>
	internal sealed class InMemoryDirectoryInfo
		: IDirectoryInfoAsync
	{
		private readonly InMemoryFilesystem _filesystem;
		private readonly ISerialTaskScheduler _taskScheduler;
		private readonly string _fullName;
		private readonly string _name;

		public InMemoryDirectoryInfo(InMemoryFilesystem filesystem,
		                             ISerialTaskScheduler taskScheduler,
		                             string fullName)
		{
			if (filesystem == null)
				throw new ArgumentNullException(nameof(filesystem));
			if (taskScheduler == null)
				throw new ArgumentNullException(nameof(taskScheduler));
			if (fullName == null)
				throw new ArgumentNullException(nameof(fullName));

			_filesystem = filesystem;
			_taskScheduler = taskScheduler;
			_fullName = fullName;
			_name = Path.GetFileName(fullName);
			if (string.IsNullOrEmpty(_name))
				_name = fullName;
		}

		public IDirectoryInfoAsync Root
		{
			get
			{
				var directoryName = Path.GetPathRoot(_fullName);
				if (Equals(directoryName, _fullName))
					return this;

				return new InMemoryDirectoryInfo(_filesystem, _taskScheduler,
				                                 directoryName);
			}
		}

		public IDirectoryInfoAsync Parent
		{
			get
			{
				var directoryName = Path.GetDirectoryName(_fullName);
				if (directoryName == null)
					return null;

				return new InMemoryDirectoryInfo(_filesystem, _taskScheduler,
				                                 directoryName);
			}
		}

		public override int GetHashCode()
		{
			return new PathComparer().GetHashCode(_fullName);
		}

		public override bool Equals(object obj)
		{
			var other = obj as InMemoryDirectoryInfo;
			if (other == null)
				return false;

			return new PathComparer().Equals(_fullName, other._fullName);
		}

		public string Name => _name;

		public string FullName => _fullName;

		public Task<bool> Exists => _filesystem.DirectoryExists(_fullName);

		public Task<bool> FileExists(string filename)
		{
			var path = CaptureFullPath(filename);
			return _filesystem.FileExists(path);
		}

		public Task<IDirectoryInfo> Capture()
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<IFileInfoAsync>> EnumerateFiles()
		{
			return EnumerateFiles("*", SearchOption.TopDirectoryOnly);
		}

		public Task<IEnumerable<IFileInfoAsync>> EnumerateFiles(string searchPattern)
		{
			return EnumerateFiles(searchPattern, SearchOption.TopDirectoryOnly);
		}

		public Task<IEnumerable<IFileInfoAsync>> EnumerateFiles(string searchPattern, SearchOption searchOption)
		{
			return _taskScheduler.StartNew<IEnumerable<IFileInfoAsync>>(() =>
			{
				var directory = _filesystem.GetDirectory(_fullName);
				var files = new List<InMemoryFileInfo>();
				foreach (var fileName in directory.EnumerateFiles(searchPattern, searchOption))
				{
					files.Add(new InMemoryFileInfo(_filesystem, _taskScheduler, fileName));
				}
				return files;
			});
		}

		/// <inheritdoc />
		public Task Create()
		{
			return _taskScheduler.StartNew(() => { _filesystem.CreateDirectorySync(_fullName); });
		}

		public Task Delete()
		{
			return _filesystem.DeleteDirectory(_fullName);
		}

		public Task<IDirectoryInfoAsync> CreateSubdirectory(string path)
		{
			path = CaptureFullPath(path);
			return _taskScheduler.StartNew<IDirectoryInfoAsync>(() =>
			{
				_filesystem.CreateDirectorySync(path);
				return new InMemoryDirectoryInfo(_filesystem, _taskScheduler, path);
			});
		}

		public override string ToString()
		{
			return "{" + FullName + "}";
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
				var current = _fullName;
				var abs = Path.Combine(current, path);
				return abs;
			}

			return path;
		}

	}
}