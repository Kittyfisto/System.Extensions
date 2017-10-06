using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
	/// <summary>
	///     An <see cref="IFilesystem" /> implementation which backs all operations in memory.
	///     Can be used in producation code, but is mainly aimed at being used in unit tests
	///     (production code developed against <see cref="IFilesystem" /> does not need to be
	///     using the harddisk any more => unit tests will execute MUCH faster AND
	///     you will have more control over how encapsulated tests are performing).
	/// </summary>
	public sealed class InMemoryFilesystem
		: IFilesystem
	{
		private readonly Dictionary<string, InMemoryDirectory> _roots;
		private readonly object _syncRoot;
		private readonly ISerialTaskScheduler _taskScheduler;
		private string _currentDirectory;

		/// <summary>
		///     Initializes this object.
		///     All methods will be executed on a <see cref="ImmediateTaskScheduler" />.
		/// </summary>
		public InMemoryFilesystem()
			: this(new ImmediateTaskScheduler())
		{
		}

		/// <summary>
		///     Initializes this object.
		///     All methods will be executed using the given scheduler.
		/// </summary>
		/// <param name="taskScheduler"></param>
		public InMemoryFilesystem(ISerialTaskScheduler taskScheduler)
		{
			_syncRoot = new object();
			_taskScheduler = taskScheduler;
			_roots = new Dictionary<string, InMemoryDirectory>(new PathComparer());

			const string root = @"M:\";
			AddRoot(root);
			_currentDirectory = root;
		}

		/// <inheritdoc />
		public string CurrentDirectory
		{
			get { return _currentDirectory; }
			set { _currentDirectory = value; }
		}

		/// <inheritdoc />
		public IDirectoryInfoAsync Current
		{
			get
			{
				InMemoryDirectory directory;
				TryGetDirectory(CurrentDirectory, out directory);
				return directory;
			}
		}

		/// <inheritdoc />
		public Task<IEnumerable<IDirectoryInfoAsync>> Roots
		{
			get
			{
				return _taskScheduler.StartNew<IEnumerable<IDirectoryInfoAsync>>(() =>
				{
					lock (_syncRoot)
					{
						return _roots.Values.ToList();
					}
				});
			}
		}

		/// <inheritdoc />
		public Task<IDirectoryInfoAsync> CreateDirectory(string path)
		{
			path = CaptureFullPath(path);
			return _taskScheduler.StartNew<IDirectoryInfoAsync>(() =>
			{
				var parentPath = Path.GetDirectoryName(path);
				InMemoryDirectory parentDirectory;
				if (!TryGetDirectory(parentPath, out parentDirectory))
					throw new DirectoryNotFoundException();

				var directoryName = path.Substring(parentPath.Length);
				var directory = parentDirectory.CreateChildDirectory(directoryName);
				return directory;
			});
		}

		/// <inheritdoc />
		public Task DeleteDirectory(string path)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public Task DeleteDirectory(string path, bool recursive)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public Task<bool> DirectoryExists(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return Task.FromResult(false);

			if (Path2.HasIllegalCharacters(path))
				return Task.FromResult(false);

			path = CaptureFullPath(path);
			return _taskScheduler.StartNew(() =>
			{
				InMemoryDirectory unused;
				return TryGetDirectory(path, out unused);
			});
		}

		/// <inheritdoc />
		public IDirectoryInfoAsync GetDirectoryInfo(string directoryName)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateFiles(string path)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateFiles(string path, string searchPattern)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateDirectories(string path)
		{
			path = CaptureFullPath(path);
			return _taskScheduler.StartNew<IReadOnlyList<string>>(() =>
			{
				InMemoryDirectory directory;
				if (!TryGetDirectory(path, out directory))
					return new string[0];

				return directory.Subdirectories.Select(x => x.FullName).ToList();
			});
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateDirectories(string path, string searchPattern)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public IFileInfoAsync GetFileInfo(string fileName)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public Task<bool> FileExists(string path)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public Task<long> FileLength(string path)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public Task<bool> IsFileReadOnly(string path)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public Task WriteAllBytes(string path, byte[] bytes)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public Task<Stream> OpenRead(string path)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public Task<Stream> OpenWrite(string path)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public Task Write(string path, Stream stream)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public Task DeleteFile(string path)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Adds a new root directory (i.e. "drive") to this file system.
		/// </summary>
		/// <param name="name"></param>
		public void AddRoot(string name)
		{
			lock (_syncRoot)
			{
				_roots.Add(name, new InMemoryDirectory(_taskScheduler, root: null, parent: null, name: name));
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
				var current = _currentDirectory;
				var abs = Path.Combine(current, path);
				return abs;
			}

			return path;
		}

		private bool TryGetDirectory(string path, out InMemoryDirectory directory)
		{
			var components = Directory2.Split(path);
			if (!TryGetRoot(components[0], out directory))
			{
				return false;
			}

			for(int i = 1; i < components.Count; ++i)
			{
				var directoryName = components[i];
				if (!directory.TryGetDirectory(directoryName, out directory))
					return false;
			}

			return true;
		}

		private bool TryGetRoot(string root, out InMemoryDirectory directory)
		{
			if (root == null)
			{
				directory = null;
				return false;
			}

			lock (_syncRoot)
			{
				return _roots.TryGetValue(root, out directory);
			}
		}

		private sealed class InMemoryFile
			: IFileInfoAsync
		{
			private readonly string _fullPath;
			private readonly string _name;

			public InMemoryFile(string fullPath)
			{
				_fullPath = fullPath;
				_name = Path.GetFileName(fullPath);
			}

			public override string ToString()
			{
				return "{" + _fullPath + "}";
			}

			public string Name => _name;

			public string FullPath => _fullPath;

			public Task<IFileInfo> Capture()
			{
				throw new NotImplementedException();
			}

			public Task<long> Length
			{
				get { throw new NotImplementedException(); }
			}

			public Task<bool> IsReadOnly
			{
				get { throw new NotImplementedException(); }
			}

			public Task<bool> Exists
			{
				get { throw new NotImplementedException(); }
			}
		}

		private sealed class InMemoryDirectory
			: IDirectoryInfoAsync
		{
			private readonly ISerialTaskScheduler _taskScheduler;
			private readonly Dictionary<string, InMemoryFile> _files;
			private readonly string _fullName;
			private readonly string _name;
			private readonly InMemoryDirectory _parent;
			private readonly InMemoryDirectory _root;
			private readonly Dictionary<string, InMemoryDirectory> _subDirectories;
			private readonly object _syncRoot;

			public InMemoryDirectory(ISerialTaskScheduler taskScheduler, InMemoryDirectory root, InMemoryDirectory parent, string name)
			{
				_taskScheduler = taskScheduler;
				_root = parent != null ? root : this;
				_parent = parent;
				_name = name;
				_fullName = parent != null ? Path.Combine(parent.FullName, name) : name;
				_syncRoot = new object();
				_subDirectories = new Dictionary<string, InMemoryDirectory>();
				_files = new Dictionary<string, InMemoryFile>();
			}

			public override string ToString()
			{
				return "{" + _fullName + "}";
			}

			public IDirectoryInfoAsync Root => _root;

			public IDirectoryInfoAsync Parent => _parent;

			public string Name => _name;

			public string FullName => _fullName;

			public Task<bool> Exists => Task.FromResult(true);

			public IEnumerable<IDirectoryInfoAsync> Subdirectories
			{
				get
				{
					lock (_syncRoot)
					{
						return _subDirectories.Values.ToList();
					}
				}
			}

			public Task<IDirectoryInfo> Capture()
			{
				throw new NotImplementedException();
			}

			public Task<IEnumerable<IFileInfoAsync>> EnumerateFiles()
			{
				throw new NotImplementedException();
			}

			public Task<IEnumerable<IFileInfoAsync>> EnumerateFiles(string searchPattern)
			{
				throw new NotImplementedException();
			}

			public Task<IEnumerable<IFileInfoAsync>> EnumerateFiles(string searchPattern, SearchOption searchOption)
			{
				throw new NotImplementedException();
			}

			public Task<IDirectoryInfoAsync> CreateSubdirectory(string path)
			{
				if (!Path.IsPathRooted(path))
				{
					return _taskScheduler.StartNew<IDirectoryInfoAsync>(() =>
					{
						var components = Directory2.Split(path);
						var directory = this;
						foreach (var directoryName in components)
						{
							directory = directory.CreateChildDirectory(directoryName);
						}
						return directory;
					});
				}

				throw new NotImplementedException();
			}

			public bool TryGetDirectoryFromPath(string path, out InMemoryDirectory directory)
			{
				var root = Path.GetPathRoot(path);
				InMemoryDirectory parent;
				if (!TryGetDirectory(root, out parent))
				{
					directory = null;
					return false;
				}

				var remainingPath = path.Substring(root.Length);
				return parent.TryGetDirectoryFromPath(remainingPath, out directory);
			}

			public bool TryGetDirectory(string directoryName, out InMemoryDirectory directory)
			{
				lock (_syncRoot)
				{
					return _subDirectories.TryGetValue(directoryName, out directory);
				}
			}

			public InMemoryDirectory CreateChildDirectory(string directoryName)
			{
				lock (_syncRoot)
				{
					directoryName = directoryName.Replace(Directory2.DirectorySeparatorString, "");
					directoryName = directoryName.Replace(Directory2.AltDirectorySeparatorString, "");

					InMemoryDirectory directory;
					if (!_subDirectories.TryGetValue(directoryName, out directory))
					{
						directory = new InMemoryDirectory(_taskScheduler, _root, this, directoryName);
						_subDirectories.Add(directoryName, directory);
					}
					return directory;
				}
			}
		}
	}
}