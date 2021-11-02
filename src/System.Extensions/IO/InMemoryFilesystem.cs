using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO.InMemory;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
	///     At the outer layer, there are <see cref="DirectoryInfo2"/> and <see cref="FileInfo2"/>
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
		private readonly InMemoryFilesystemWatchdog _watchdog;

		/// <summary>
		///     Initializes this object.
		///     All methods will be executed using the given scheduler.
		/// </summary>
		public InMemoryFilesystem()
		{
			_syncRoot = new object();
			_roots = new Dictionary<string, InMemoryDirectory>(new PathComparer());
			_watchdog = new InMemoryFilesystemWatchdog(this);

			const string root = @"M:\";
			AddRoot(root);
			CurrentDirectory = root;
		}

		/// <inheritdoc />
		public IFilesystemWatchdog Watchdog => _watchdog;

		/// <inheritdoc />
		public DateTime FileCreationTimeUtc(string fullPath)
		{
			return GetFileInfo(fullPath).CreationTimeUtc;
		}

		/// <inheritdoc />
		public DateTime FileLastAccessTimeUtc(string fullPath)
		{
			return GetFileInfo(fullPath).LastAccessTimeUtc;
		}

		/// <inheritdoc />
		public DateTime FileLastWriteTimeUtc(string fullPath)
		{
			return GetFileInfo(fullPath).LastWriteTimeUtc;
		}

		/// <inheritdoc />
		public string CurrentDirectory { get; set; }

		/// <inheritdoc />
		public IDirectoryInfo Current => GetDirectoryInfo(CurrentDirectory);

		/// <inheritdoc />
		public IEnumerable<IDirectoryInfo> Roots
		{
			get
			{
				lock (_syncRoot)
				{
					return _roots.Values.Select(x => CreateDirectoryInfo(x.FullName)).ToList();
				}
			}
		}

		/// <inheritdoc />
		public IDirectoryInfo CreateDirectory(string path)
		{
			var fullPath = CaptureFullPath(path);
			CreateDirectorySync(fullPath);
			return GetDirectoryInfo(fullPath);
		}

		/// <inheritdoc />
		public void DeleteDirectory(string path)
		{
			DeleteDirectory(path, recursive: false);
		}

		/// <inheritdoc />
		public void DeleteDirectory(string path, bool recursive)
		{
			Path2.ThrowIfPathIsInvalid(path);

			var fullPath = CaptureFullPath(path);
			var directory = GetDirectory(fullPath);

			var parent = directory.Parent;

			bool isEmpty;
			if (!parent.DeleteSubdirectory(directory.Name, recursive, out isEmpty))
			{
				if (!isEmpty)
					throw new IOException(string.Format("The directory '{0}' is not empty", path));

				throw new DirectoryNotFoundException(string.Format("Could not find a part of the path '{0}'", path));
			}

		}

		/// <inheritdoc />
		public bool DirectoryExists(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return false;

			if (Path2.HasIllegalCharacters(path))
				return false;

			path = CaptureFullPath(path);
			InMemoryDirectory unused;
			return TryGetDirectory(path, out unused);
		}

		/// <inheritdoc />
		public IDirectoryInfo GetDirectoryInfo(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			if (!Path2.IsValidPath(path))
				throw new ArgumentException(nameof(path));

			path = CaptureFullPath(path);
			return CreateDirectoryInfo(path);
		}

		/// <inheritdoc />
		public IReadOnlyList<string> EnumerateFiles(string path,
		                                            string searchPattern = null,
		                                            SearchOption searchOption = SearchOption.TopDirectoryOnly,
		                                            bool tolerateNonExistantPath = false)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			InMemoryDirectory directory;
			if (!TryGetDirectory(path, out directory))
			{
				if (tolerateNonExistantPath)
					return new List<string>();

				throw new DirectoryNotFoundException(string.Format("Could not find a part of the path '{0}'", path));
			}

			return directory.EnumerateFiles(searchPattern, searchOption);
		}

		/// <inheritdoc />
		public IReadOnlyList<string> EnumerateDirectories(string path)
		{
			return EnumerateDirectories(path, "*");
		}

		/// <inheritdoc />
		public IReadOnlyList<string> EnumerateDirectories(string path, string searchPattern)
		{
			return EnumerateDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);
		}

		/// <inheritdoc />
		public IReadOnlyList<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
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
		}

		/// <inheritdoc />
		public IFileInfo GetFileInfo(string fileName)
		{
			Path2.ThrowIfPathIsInvalid(fileName, nameof(fileName));

			var path = CaptureFullPath(fileName);
			return new InMemoryFileInfo(this, path);
		}

		/// <inheritdoc />
		public bool FileExists(string fileName)
		{
			if (fileName == null)
				return false;

			if (!Path2.IsValidPath(fileName))
				return false;

			var fullFileName = CaptureFullPath(fileName);
			InMemoryFile unused;
			return TryGetFile(fullFileName, out unused);
		}

		/// <inheritdoc />
		public long FileLength(string fileName)
		{
			Path2.ThrowIfPathIsInvalid(fileName, nameof(fileName));

			var fullFileName = CaptureFullPath(fileName);
			InMemoryFile file;
			if (!TryGetFile(fullFileName, out file))
				throw new FileNotFoundException();

			return file.Length;
		}

		/// <inheritdoc />
		public bool IsFileReadOnly(string path)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc />
		public byte[] ReadAllBytes(string path)
		{
			using (var stream = OpenRead(path))
			{
				return stream.ReadToEnd();
			}
		}

		/// <inheritdoc />
		public string ReadAllText(string path)
		{
			using (var stream = OpenRead(path))
			using (var reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}

		/// <inheritdoc />
		public string ReadAllText(string path, Encoding encoding)
		{
			using (var stream = OpenRead(path))
			using (var reader = new StreamReader(stream, encoding))
			{
				return reader.ReadToEnd();
			}
		}

		/// <inheritdoc />
		public IReadOnlyList<string> ReadAllLines(string path)
		{
			using (var stream = OpenRead(path))
			using (var reader = new StreamReader(stream))
			{
				var lines = new List<string>();
				while (!reader.EndOfStream)
				{
					lines.Add(reader.ReadLine());
				}

				return lines.ToArray();
			}
		}

		/// <inheritdoc />
		public IReadOnlyList<string> ReadAllLines(string path, Encoding encoding)
		{
			using (var stream = OpenRead(path))
			using (var reader = new StreamReader(stream, encoding))
			{
				var lines = new List<string>();
				while (!reader.EndOfStream)
				{
					lines.Add(reader.ReadLine());
				}

				return lines.ToArray();
			}
		}

		/// <inheritdoc />
		public Stream CreateFile(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			var directoryPath = Path.GetDirectoryName(path);
			var directory = GetDirectory(directoryPath);
			var fileName = Path.GetFileName(path);
			var stream = directory.CreateFile(fileName);

			_watchdog.NotifyWatchers();

			return stream;
		}

		/// <inheritdoc />
		public Stream Open(string path, FileMode mode, FileAccess access, FileShare share)
		{
			Stream stream;
			switch (mode)
			{
				case FileMode.Open:
					return OpenRead(path);
				case FileMode.Append:
					stream = OpenWrite(path);
					try
					{
						stream.Position = stream.Length;
						return stream;
					}
					catch (Exception)
					{
						stream.Dispose();
						throw;
					}
				case FileMode.CreateNew:
					if (FileExists(path))
						throw new IOException($"The given file already exists: {path}");
					return OpenWrite(path);
				case FileMode.Truncate:
					if (!FileExists(path))
						throw new FileNotFoundException($"No such file: {path}");
					stream = OpenWrite(path);
					try
					{
						stream.SetLength(0);
						return stream;
					}
					catch (Exception)
					{
						stream.Dispose();
						throw;
					}
				case FileMode.Create:
					stream = OpenWrite(path);
					try
					{
						stream.SetLength(0);
						return stream;
					}
					catch (Exception)
					{
						stream.Dispose();
						throw;
					}
				default:
					throw new NotImplementedException();
			}
		}

		/// <inheritdoc />
		public Stream OpenRead(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			var directoryPath = Path.GetDirectoryName(path);
			var directory = GetDirectory(directoryPath);
			var fileName = Path.GetFileName(path);
			return directory.OpenReadSync(fileName);
		}

		/// <inheritdoc />
		public Stream OpenWrite(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			var directoryPath = Path.GetDirectoryName(path);
			var directory = GetDirectory(directoryPath);
			var fileName = Path.GetFileName(path);
			var stream = directory.OpenWriteSync(fileName);

			_watchdog.NotifyWatchers();

			return stream;
		}

		/// <inheritdoc />
		public void Write(string path, Stream stream)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			using (var targetStream = OpenWrite(path))
			{
				stream.CopyTo(targetStream);
			}
		}

		/// <inheritdoc />
		public void CopyFile(string sourceFileName, string destFileName)
		{
			Path2.ThrowIfPathIsInvalid(sourceFileName, nameof(sourceFileName));
			Path2.ThrowIfPathIsInvalid(destFileName, nameof(destFileName));

			var sourceFilePath = CaptureFullPath(sourceFileName);
			var destFilePath = CaptureFullPath(destFileName);
			var destFileDirectory = Path.GetDirectoryName(destFilePath);
			var destFile = Path.GetFileName(destFileName);

			using (var stream = OpenRead(sourceFilePath))
			{
				var destDirectory = GetDirectory(destFileDirectory);
				InMemoryFile file;
				if (destDirectory.TryGetFile(destFile, out file))
					throw new IOException(string.Format("The file '{0}' already exists", destFilePath));

				using (var targetStream = destDirectory.OpenWriteSync(destFile))
				{
					stream.CopyTo(targetStream);
				}
			}
		}

		/// <inheritdoc />
		public void WriteAllBytes(string path, byte[] bytes)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (bytes == null)
				throw new ArgumentNullException(nameof(bytes));

			using (var stream = OpenWrite(path))
			{
				stream.Write(bytes, 0, bytes.Length);
				_watchdog.NotifyWatchers();
			}
		}

		/// <inheritdoc />
		public void WriteAllText(string path, string contents)
		{
			using (var stream = OpenWrite(path))
			using (var writer = new StreamWriter(stream))
			{
				writer.Write(contents);
				_watchdog.NotifyWatchers();
			}
		}

		/// <inheritdoc />
		public void WriteAllText(string path, string contents, Encoding encoding)
		{
			using (var stream = OpenWrite(path))
			using (var writer = new StreamWriter(stream, encoding))
			{
				writer.Write(contents);
				_watchdog.NotifyWatchers();
			}
		}

		/// <inheritdoc />
		public void DeleteFile(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			var directoryPath = Path.GetDirectoryName(path);
			InMemoryDirectory directory = GetDirectory(directoryPath);
			var fileName = Path.GetFileName(path);
			directory.TryDeleteFile(fileName);

			_watchdog.NotifyWatchers();
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
		public string Print()
		{
			var builder = new StringBuilder();
			foreach (var root in _roots.Values.OrderBy(x => x.Name)) root.Print(builder);
			return builder.ToString();
		}

		private IDirectoryInfo CreateDirectoryInfo(string fullName)
		{
			return DirectoryInfo2.Create(this, fullName);
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

		sealed class InMemoryFileInfo
			: FileInfo2
		{
			private readonly InMemoryFilesystem _filesystem;

			public InMemoryFileInfo(InMemoryFilesystem filesystem, string fullPath)
				: base(filesystem, fullPath)
			{
				_filesystem = filesystem;
			}

			#region Overrides of FileInfo2

			public override DateTime CreationTimeUtc
			{
				get
				{
					if (!_filesystem.TryGetFile(FullPath, out var file))
						return new DateTime(1601, 01, 01);
					return file.Created;
				}
			}

			public override DateTime LastAccessTimeUtc
			{
				get
				{
					if (!_filesystem.TryGetFile(FullPath, out var file))
						return new DateTime(1601, 01, 01);
					return file.LastAccessed;
				}
			}

			public override DateTime LastWriteTimeUtc
			{
				get
				{
					if (!_filesystem.TryGetFile(FullPath, out var file))
						return new DateTime(1601, 01, 01);
					return file.LastWritten;
				}
			}

			#endregion
		}
	}
}