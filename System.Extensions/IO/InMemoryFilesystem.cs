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
	/// <remarks>
	///     This filesystem is implemented using two layers:
	///     At the core, there exist <see cref="InMemoryDirectory"/> and <see cref="InMemoryFile"/>
	///     which actually represent the directories and files (including content). These objects are
	///     linked and there is only one object per folder and per file.
	/// 
	///     At the outer layer, there are <see cref="InMemoryDirectoryInfo"/> and <see cref="InMemoryFileInfo"/>
	///     which represent the objects from the first layer, but do **NOT** hold any state (just like their
	///     .NET counterparts). Instead they are proxies for the actual directories and files.
	/// 
	///     This design has been chosen because it mirrors how <see cref="DirectoryInfo"/> and <see cref="FileInfo"/>
	///     behave: A <see cref="DirectoryInfo"/> object can exist, but its directory could not. Therefore
	/// </remarks>
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
						return _roots.Values.Select(CreateDirectoryInfo).ToList();
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
				CreateDirectorySync(path);
				return new InMemoryDirectoryInfo(this, _taskScheduler, path);
			});
		}

		/// <inheritdoc />
		public Task DeleteDirectory(string path)
		{
			return DeleteDirectory(path, recursive: false);
		}

		/// <inheritdoc />
		public Task DeleteDirectory(string path, bool recursive)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			return _taskScheduler.StartNew(() =>
			{
				var parent = Path.GetDirectoryName(path);
				InMemoryDirectory directory = GetDirectory(parent);

				var directoryName = Directory2.GetDirName(path);
				bool isEmpty;
				if (!directory.DeleteSubdirectory(directoryName, recursive, out isEmpty))
				{
					if (!isEmpty)
						throw new IOException(string.Format("The directory '{0}' is not empty", path));

					throw new DirectoryNotFoundException(string.Format("Could not find a part of the path '{0}'", path));
				}
			});
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
			return new InMemoryDirectoryInfo(this, _taskScheduler, path);
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateFiles(string path,
		                                                  string searchPattern = null,
		                                                  SearchOption searchOption = SearchOption.TopDirectoryOnly,
		                                                  bool tolerateNonExistantPath = false)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			return _taskScheduler.StartNew(() =>
			{
				InMemoryDirectory directory;
				if (!TryGetDirectory(path, out directory))
				{
					if (tolerateNonExistantPath)
						return new List<string>();

					throw new DirectoryNotFoundException(string.Format("Could not find a part of the path '{0}'", path));
				}

				return directory.EnumerateFiles(searchPattern, searchOption);
			});
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateDirectories(string path)
		{
			return EnumerateDirectories(path, "*");
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateDirectories(string path, string searchPattern)
		{
			return EnumerateDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			return _taskScheduler.StartNew<IReadOnlyList<string>>(() =>
			{
				InMemoryDirectory rootDirectory;
				if (!TryGetDirectory(path, out rootDirectory))
					return new string[0];

				var regex = CreateRegex(searchPattern);
				var directories = new Stack<InMemoryDirectory>();
				var result = new List<string>();

				directories.Push(rootDirectory);

				while (directories.Count > 0)
				{
					var directory = directories.Pop();
					foreach (var subDirectory in directory.Subdirectories)
					{
						if (regex.IsMatch(subDirectory.Name))
						{
							if (searchOption == SearchOption.AllDirectories)
							{
								directories.Push(subDirectory);
							}
							result.Add(subDirectory.FullName);
						}
					}
				}

				return result;
			});
		}

		/// <inheritdoc />
		public IFileInfoAsync GetFileInfo(string fileName)
		{
			Path2.ThrowIfPathIsInvalid(fileName);

			var path = CaptureFullPath(fileName);
			return new InMemoryFileInfo(this, _taskScheduler, path);
		}

		/// <inheritdoc />
		public Task<bool> FileExists(string fileName)
		{
			if (fileName == null)
				return Task.FromResult(result: false);

			if (!Path2.IsValidPath(fileName))
				return Task.FromResult(result: false);

			var path = CaptureFullPath(fileName);
			return new InMemoryFileInfo(this, _taskScheduler, path).Exists;
		}

		/// <inheritdoc />
		public Task<long> FileLength(string fileName)
		{
			Path2.ThrowIfPathIsInvalid(fileName);

			var path = CaptureFullPath(fileName);
			return new InMemoryFileInfo(this, _taskScheduler, path).Length;
		}

		/// <inheritdoc />
		public Task<bool> IsFileReadOnly(string path)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public Task<byte[]> ReadAllBytes(string path)
		{
			return OpenRead(path)
				.ContinueWith(task =>
				{
					using (var stream = task.Result)
					{
						return stream.ReadToEnd();
					}
				}, TaskContinuationOptions.ExecuteSynchronously);
		}

		/// <inheritdoc />
		public Task<Stream> CreateFile(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			return _taskScheduler.StartNew(() =>
			{
				var directoryPath = Path.GetDirectoryName(path);
				var directory = GetDirectory(directoryPath);
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
				var directory = GetDirectory(directoryPath);
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
				var directory = GetDirectory(directoryPath);
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
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			return OpenWrite(path)
				.ContinueWith(task =>
				{
					using (var targetStream = task.Result)
					{
						stream.CopyTo(targetStream);
					}
				}, TaskContinuationOptions.ExecuteSynchronously);
		}

		/// <inheritdoc />
		public Task WriteAllBytes(string path, byte[] bytes)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (bytes == null)
				throw new ArgumentNullException(nameof(bytes));

			return OpenWrite(path)
				.ContinueWith(task =>
				{
					using (var targetStream = task.Result)
					{
						targetStream.Write(bytes, 0, bytes.Length);
					}
				}, TaskContinuationOptions.ExecuteSynchronously);
		}

		/// <inheritdoc />
		public Task DeleteFile(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			return _taskScheduler.StartNew(() =>
			{
				var directoryPath = Path.GetDirectoryName(path);
				InMemoryDirectory directory = GetDirectory(directoryPath);
				var fileName = Path.GetFileName(path);
				directory.TryDeleteFile(fileName);
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
				_roots.Add(name, new InMemoryDirectory(parent: null, name: name));
			}
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
				foreach (var root in _roots.Values.OrderBy(x => x.Name)) root.Print(builder);
				return builder.ToString();
			});
		}

		private IDirectoryInfoAsync CreateDirectoryInfo(InMemoryDirectory arg)
		{
			return new InMemoryDirectoryInfo(this, _taskScheduler, arg.FullName);
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

		internal InMemoryDirectory GetDirectory(string path)
		{
			InMemoryDirectory directory;
			if (!TryGetDirectory(path, out directory))
				throw new DirectoryNotFoundException(string.Format("Could not find a part of the path '{0}'", path));

			return directory;
		}

		internal bool TryGetDirectory(string path, out InMemoryDirectory directory)
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

		internal bool TryGetFile(string filePath, out InMemoryFile file)
		{
			var directoryName = Path.GetDirectoryName(filePath);
			InMemoryDirectory directory;
			if (!TryGetDirectory(directoryName, out directory))
			{
				file = null;
				return false;
			}

			var fileName = Path.GetFileName(filePath);
			return directory.TryGetFile(fileName, out file);
		}

		/// <summary>
		///     Creates the entire directory tree for the given path.
		/// </summary>
		/// <param name="path"></param>
		internal InMemoryDirectory CreateDirectorySync(string path)
		{
			lock (_syncRoot)
			{
				var components = Directory2.Split(path);
				InMemoryDirectory directory;
				if (!TryGetRoot(components[index: 0], out directory))
					throw new DirectoryNotFoundException(string.Format("Could not find a part of the path '{0}'.", path));

				for (var i = 1; i < components.Count; ++i)
				{
					var directoryName = components[i];
					directory = directory.CreateSubdirectory(directoryName);
				}

				return directory;
			}
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
	}
}