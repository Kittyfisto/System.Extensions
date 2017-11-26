using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
			if (taskScheduler == null)
				throw new ArgumentNullException(nameof(taskScheduler));

			_syncRoot = new object();
			_taskScheduler = taskScheduler;
			_roots = new Dictionary<string, InMemoryDirectory>(new PathComparer());

			const string root = @"M:\";
			AddRoot(root);
			CurrentDirectory = root;
		}

		/// <inheritdoc />
		public string CurrentDirectory { get; set; }

		/// <inheritdoc />
		public IDirectoryInfoAsync Current => GetDirectoryInfo(CurrentDirectory);

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
				var components = Directory2.Split(path);
				InMemoryDirectory directory;
				if (!TryGetRoot(components[index: 0], out directory))
					throw new DirectoryNotFoundException();

				for (var i = 1; i < components.Count; ++i)
					directory = directory.CreateChildDirectory(components[i]);
				return directory;
			});
		}

		/// <inheritdoc />
		public Task DeleteDirectory(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			return _taskScheduler.StartNew(() =>
			{
				var parent = Path.GetDirectoryName(path);
				InMemoryDirectory directory;
				if (!TryGetDirectory(parent, out directory))
					throw new DirectoryNotFoundException(string.Format("Could not find a part of the path '{0}'", path));

				var directoryName = Directory2.GetDirName(path);
				bool isEmpty;
				if (!directory.DeleteSubdirectory(directoryName, out isEmpty))
				{
					if (!isEmpty)
						throw new IOException(string.Format("The directory '{0}' is not empty", path));

					throw new DirectoryNotFoundException(string.Format("Could not find a part of the path '{0}'", path));
				}
			});
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
				return Task.FromResult(result: false);

			if (Path2.HasIllegalCharacters(path))
				return Task.FromResult(result: false);

			path = CaptureFullPath(path);
			return _taskScheduler.StartNew(() =>
			{
				InMemoryDirectory unused;
				return TryGetDirectory(path, out unused);
			});
		}

		/// <inheritdoc />
		public IDirectoryInfoAsync GetDirectoryInfo(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			if (!Path2.IsValidPath(path))
				throw new ArgumentException(nameof(path));

			path = CaptureFullPath(path);
			var components = Directory2.Split(path);
			var rootName = components[index: 0];
			InMemoryDirectory root;
			if (!TryGetRoot(rootName, out root))
				throw new IOException(string.Format("No such drive '{0}'", rootName));

			var directory = root;
			for (var i = 1; i < components.Count; ++i)
			{
				var directoryName = components[i];
				InMemoryDirectory next;
				if (!directory.TryGetDirectory(directoryName, out next))
					next = new InMemoryDirectory(this, _taskScheduler,
						root, directory, directoryName);

				directory = next;
			}

			return directory;
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateFiles(string path,
			string searchPattern = null,
			SearchOption searchOption = SearchOption.TopDirectoryOnly,
			bool tolerateNonExistantPath = false)
		{
			Path2.ThrowIfPathIsInvalid(path);

			if (searchOption == SearchOption.AllDirectories)
				throw new NotImplementedException();

			path = CaptureFullPath(path);
			return _taskScheduler.StartNew<IReadOnlyList<string>>(() =>
			{
				InMemoryDirectory directory;
				if (!TryGetDirectory(path, out directory))
				{
					if (tolerateNonExistantPath)
						return new List<string>();

					throw new DirectoryNotFoundException(string.Format("Could not find a part of the path '{0}'", path));
				}

				var files = directory.Files.Select(x => x.FullPath);

				if (searchPattern != null)
				{
					var regex = CreateRegex(searchPattern);
					files = files.Where(x => regex.IsMatch(x));
				}

				return files.ToList();
			});
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateDirectories(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

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
			var task = EnumerateDirectories(path);
			return _taskScheduler.StartNew<IReadOnlyList<string>>(() =>
			{
				var regex = CreateRegex(searchPattern);
				var matches = task.Result.Where(x => regex.IsMatch(x)).ToList();
				return matches;
			});
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public IFileInfoAsync GetFileInfo(string fileName)
		{
			Path2.ThrowIfPathIsInvalid(fileName);

			var path = CaptureFullPath(fileName);
			var directoryPath = Path.GetDirectoryName(path);
			InMemoryDirectory directory;
			if (!TryGetDirectory(directoryPath, out directory))
				throw new NotImplementedException();

			var fname = Path.GetFileName(path);
			return directory.GetFileInfo(fname);
		}

		/// <inheritdoc />
		public Task<bool> FileExists(string fileName)
		{
			if (fileName == null)
				return Task.FromResult(result: false);

			if (!Path2.IsValidPath(fileName))
				return Task.FromResult(result: false);

			var path = CaptureFullPath(fileName);
			return _taskScheduler.StartNew(() =>
			{
				var directoryPath = Path.GetDirectoryName(path);
				InMemoryDirectory directory;
				if (!TryGetDirectory(directoryPath, out directory))
					return false;

				return directory.ContainsFile(Path.GetFileName(path));
			});
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
		public Task<Stream> CreateFile(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			return _taskScheduler.StartNew(() =>
			{
				var directoryPath = Path.GetDirectoryName(path);
				InMemoryDirectory directory;
				if (!TryGetDirectory(directoryPath, out directory))
					throw new DirectoryNotFoundException(string.Format("Could not find a part of the path '{0}'", path));

				var fileName = Path.GetFileName(path);
				return directory.CreateFile(fileName);
			});
		}

		/// <inheritdoc />
		public Task<Stream> OpenRead(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			return _taskScheduler.StartNew(() =>
			{
				var directoryPath = Path.GetDirectoryName(path);
				InMemoryDirectory directory;
				if (!TryGetDirectory(directoryPath, out directory))
					throw new DirectoryNotFoundException(string.Format("Could not find a part of the path '{0}'", path));

				var fileName = Path.GetFileName(path);
				return directory.OpenReadSync(fileName);
			});
		}

		/// <inheritdoc />
		public Task<Stream> OpenWrite(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			return _taskScheduler.StartNew(() =>
			{
				var directoryPath = Path.GetDirectoryName(path);
				InMemoryDirectory directory;
				if (!TryGetDirectory(directoryPath, out directory))
					throw new DirectoryNotFoundException(string.Format("Could not find a part of the path '{0}'", path));

				var fileName = Path.GetFileName(path);
				var stream = directory.OpenWriteSync(fileName);
				if (stream != null)
					return stream;

				return directory.CreateFile(fileName);
			});
		}

		/// <inheritdoc />
		public Task Write(string path, Stream stream)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public Task DeleteFile(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			return _taskScheduler.StartNew(() =>
			{
				var directoryPath = Path.GetDirectoryName(path);
				InMemoryDirectory directory;
				if (!TryGetDirectory(directoryPath, out directory))
					throw new DirectoryNotFoundException(string.Format("Could not find a part of the path '{0}'", path));

				var fileName = Path.GetFileName(path);
				directory.TryDeleteFileSync(fileName);
			});
		}

		/// <summary>
		///     Adds a new root directory (i.e. "drive") to this file system.
		/// </summary>
		/// <param name="name"></param>
		public void AddRoot(string name)
		{
			lock (_syncRoot)
			{
				_roots.Add(name, new InMemoryDirectory(this, _taskScheduler, root: null, parent: null, name: name));
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
				var current = CurrentDirectory;
				var abs = Path.Combine(current, path);
				return abs;
			}

			return path;
		}

		private bool TryGetDirectory(string path, out InMemoryDirectory directory)
		{
			var components = Directory2.Split(path);
			if (!TryGetRoot(components[index: 0], out directory))
				return false;

			for (var i = 1; i < components.Count; ++i)
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

		[Pure]
		private static Regex CreateRegex(string searchPattern)
		{
			var regexPattern = "^" + Regex.Escape(searchPattern)
				                   .Replace(@"\*", ".*")
				                   .Replace(@"\?", ".")
			                   + "$";
			var regex = new Regex(regexPattern);
			return regex;
		}

		/// <summary>
		///     Creates a string which represents every file and directory in this filesystem.
		/// </summary>
		/// <returns></returns>
		public Task<string> Print()
		{
			return _taskScheduler.StartNew(() =>
			{
				var builder = new StringBuilder();
				foreach (var root in _roots.Values.OrderBy(x => x.Name))
				{
					root.PrintSync(builder);
				}
				return builder.ToString();
			});
		}

		private sealed class InMemoryFile
			: IFileInfoAsync
		{
			private readonly InMemoryFilesystem _filesystem;
			private readonly string _name;
			private MemoryStream _content;

			public InMemoryFile(InMemoryFilesystem filesystem, string fullPath)
			{
				if (filesystem == null)
					throw new ArgumentNullException(nameof(filesystem));

				_filesystem = filesystem;
				FullPath = fullPath;
				_name = Path.GetFileName(fullPath);
				_content = new MemoryStream();
			}

			public Stream Content => _content;

			public string Name => _name;

			public string FullPath { get; }

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

			public Task<bool> Exists => _filesystem.FileExists(FullPath);

			public Task<Stream> Create()
			{
				throw new NotImplementedException();
			}

			public override string ToString()
			{
				return "{" + FullPath + "}";
			}

			public void Clear()
			{
				_content = new MemoryStream();
			}

			public void PrintSync(StringBuilder builder)
			{
				builder.AppendFormat("{0} [File]", FullPath);
			}
		}

		private sealed class InMemoryDirectory
			: IDirectoryInfoAsync
		{
			private readonly Dictionary<string, InMemoryFile> _files;
			private readonly InMemoryFilesystem _filesystem;
			private readonly string _name;
			private readonly InMemoryDirectory _parent;
			private readonly InMemoryDirectory _root;
			private readonly Dictionary<string, InMemoryDirectory> _subDirectories;
			private readonly object _syncRoot;
			private readonly ISerialTaskScheduler _taskScheduler;

			public InMemoryDirectory(InMemoryFilesystem filesystem, ISerialTaskScheduler taskScheduler, InMemoryDirectory root,
				InMemoryDirectory parent, string name)
			{
				_filesystem = filesystem;
				_taskScheduler = taskScheduler;
				_root = parent != null ? root : this;
				_parent = parent;
				_name = name;
				FullName = parent != null ? Path.Combine(parent.FullName, name) : name;
				_syncRoot = new object();
				_subDirectories = new Dictionary<string, InMemoryDirectory>();
				_files = new Dictionary<string, InMemoryFile>(new PathComparer());
			}

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

			public IEnumerable<IFileInfoAsync> Files
			{
				get
				{
					lock (_syncRoot)
					{
						return _files.Values.ToList();
					}
				}
			}

			public IDirectoryInfoAsync Root => _root;

			public IDirectoryInfoAsync Parent => _parent;

			public string Name => _name;

			public string FullName { get; }

			public Task<bool> Exists => _filesystem.DirectoryExists(FullName);

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

			/// <inheritdoc />
			public Task Create()
			{
				return _taskScheduler.StartNew(() => { _parent.AddChildDirectory(this); });
			}

			public Task<IDirectoryInfoAsync> CreateSubdirectory(string path)
			{
				if (!Path.IsPathRooted(path))
					return _taskScheduler.StartNew<IDirectoryInfoAsync>(() =>
					{
						var components = Directory2.Split(path);
						var directory = this;
						foreach (var directoryName in components)
							directory = directory.CreateChildDirectory(directoryName);
						return directory;
					});

				throw new NotImplementedException();
			}

			public override string ToString()
			{
				return "{" + FullName + "}";
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
						directory = new InMemoryDirectory(_filesystem, _taskScheduler, _root, this, directoryName);
						AddChildDirectory(directory);
					}
					return directory;
				}
			}

			public bool DeleteSubdirectory(string directoryName, out bool isEmpty)
			{
				lock (_syncRoot)
				{
					InMemoryDirectory directory;
					if (_subDirectories.TryGetValue(directoryName, out directory))
					{
						if (directory.Files.Any() || directory.Subdirectories.Any())
						{
							isEmpty = false;
							return false;
						}

						_subDirectories.Remove(directoryName);
						isEmpty = true;
						return true;
					}

					isEmpty = true;
					return false;
				}
			}

			private void AddChildDirectory(InMemoryDirectory directory)
			{
				lock (_syncRoot)
				{
					InMemoryDirectory tmp;
					if (_subDirectories.TryGetValue(directory.Name, out tmp))
					{
						if (!ReferenceEquals(tmp, directory))
							throw new Exception("This shouldn't happen");
					}
					else
					{
						_subDirectories.Add(directory.Name, directory);
					}
				}
			}

			[Pure]
			public bool ContainsFile(string fileName)
			{
				lock (_syncRoot)
				{
					return _files.ContainsKey(fileName);
				}
			}

			public Stream CreateFile(string fileName)
			{
				lock (_syncRoot)
				{
					InMemoryFile file;
					if (_files.TryGetValue(fileName, out file))
					{
						file.Clear();
					}
					else
					{
						var fullPath = Path.Combine(FullName, fileName);
						file = new InMemoryFile(_filesystem, fullPath);
						_files.Add(fileName, file);
					}

					return new StreamProxy(file.Content);
				}
			}

			public Stream OpenReadSync(string fileName)
			{
				lock (_syncRoot)
				{
					InMemoryFile file;
					if (!_files.TryGetValue(fileName, out file))
						throw new FileNotFoundException();

					var stream = new StreamProxy(file.Content) {Position = 0};
					return stream;
				}
			}

			public Stream OpenWriteSync(string fileName)
			{
				lock (_syncRoot)
				{
					InMemoryFile file;
					if (!_files.TryGetValue(fileName, out file))
						return null;

					file.Clear();
					var stream = new StreamProxy(file.Content) {Position = 0};
					return stream;
				}
			}

			public InMemoryFile GetFileInfo(string fname)
			{
				lock (_syncRoot)
				{
					InMemoryFile file;
					if (!_files.TryGetValue(fname, out file))
					{
						var fullPath = Path.Combine(FullName, fname);
						file = new InMemoryFile(_filesystem, fullPath);
					}

					return file;
				}
			}

			public bool TryDeleteFileSync(string fileName)
			{
				lock (_syncRoot)
				{
					return _files.Remove(fileName);
				}
			}

			public void PrintSync(StringBuilder builder)
			{
				lock (_syncRoot)
				{
					builder.AppendFormat("{0} [Drive]", FullName);
					builder.AppendLine();
					foreach (var directory in _subDirectories.Values.OrderBy(x => x.Name))
					{
						directory.PrintSync(builder);
						builder.AppendLine();
					}
					foreach (var file in _files.Values.OrderBy(x => x.Name))
					{
						file.PrintSync(builder);
						builder.AppendLine();
					}
				}
			}
		}
	}
}