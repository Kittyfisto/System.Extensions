using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO.FS;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace System.IO
{
	/// <summary>
	///     Provides access to the filesystem.
	/// </summary>
	/// <remarks>
	///     TODO: Prevent shitty networked drives from blocking the I/O thread for 60 seconds
	/// </remarks>
	public sealed class Filesystem
		: IFilesystem
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ISerialTaskScheduler _ioScheduler;
		private readonly FilesystemWatchdog _watchdog;

		/// <summary>
		/// Initializes this object.
		/// </summary>
		/// <param name="ioScheduler">The scheduler used to perform I/O tasks</param>
		/// <param name="taskScheduler">The scheduler used for timers (to trigger periodic I/O tasks)</param>
		public Filesystem(ISerialTaskScheduler ioScheduler,
		                  ITaskScheduler taskScheduler)
		{
			if (ioScheduler == null)
				throw new ArgumentNullException(nameof(ioScheduler));
			if (taskScheduler == null)
				throw new ArgumentNullException(nameof(taskScheduler));

			_ioScheduler = ioScheduler;
			_watchdog = new FilesystemWatchdog(this, taskScheduler);
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateFiles(string path,
			string searchPattern = null,
			SearchOption searchOption = SearchOption.TopDirectoryOnly,
			bool tolerateNonExistantPath = false)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			return _ioScheduler.StartNew<IReadOnlyList<string>>(() =>
			{
				Log.DebugFormat("Enumerating files of directory '{0}'...", path);
				if (searchPattern == null)
				{
					searchPattern = "*";
				}

				if (tolerateNonExistantPath)
				{
					if (!Directory.Exists(path))
						return new string[0];

					try
					{
						return Directory.EnumerateFiles(path, searchPattern, searchOption).ToList();
					}
					catch (DirectoryNotFoundException)
					{
						return new string[0];
					}
				}

				return Directory.EnumerateFiles(path, searchPattern, searchOption).ToList();
			});
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateDirectories(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			return _ioScheduler.StartNew<IReadOnlyList<string>>(() =>
			{
				Log.DebugFormat("Enumerating directories of directory '{0}'...", path);
				return Directory.EnumerateDirectories(path).ToList();
			});
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateDirectories(string path, string searchPattern)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			return _ioScheduler.StartNew<IReadOnlyList<string>>(() =>
			{
				Log.DebugFormat("Enumerating directories of directory '{0}'...", path);
				return Directory.EnumerateDirectories(path, searchPattern).ToList();
			});
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateDirectories(string path,
			string searchPattern,
			SearchOption searchOption)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			return _ioScheduler.StartNew<IReadOnlyList<string>>(() =>
			{
				Log.DebugFormat("Enumerating directories of directory '{0}'...", path);
				return Directory.EnumerateDirectories(path, searchPattern, searchOption).ToList();
			});
		}

		/// <inheritdoc />
		public IFileInfoAsync GetFileInfo(string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException(nameof(fileName));

			if (!Path2.IsValidPath(fileName))
				throw new ArgumentException(nameof(fileName));

			fileName = CaptureFullPath(fileName);
			return new FileInfoAsync(this, fileName);
		}

		/// <inheritdoc />
		public IDirectoryInfoAsync GetDirectoryInfo(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			if (!Path2.IsValidPath(path))
				throw new ArgumentException(nameof(path));

			path = CaptureFullPath(path);
			return DirectoryInfoAsync.FromPath(this, path);
		}

		/// <inheritdoc />
		public Task<bool> FileExists(string path)
		{
			if (!Path2.IsValidPath(path))
				return Task.FromResult(false);

			path = CaptureFullPath(path);
			return _ioScheduler.StartNew(() =>
			{
				Log.DebugFormat("Testing if file '{0}' exists...", path);
				return File.Exists(path);
			});
		}

		/// <inheritdoc />
		public Task<long> FileLength(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			return _ioScheduler.StartNew(() =>
			{
				Log.DebugFormat("Getting length of '{0}'...", path);
				return new FileInfo(path).Length;
			});
		}

		/// <inheritdoc />
		public Task<bool> IsFileReadOnly(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			return _ioScheduler.StartNew(() =>
			{
				Log.DebugFormat("Testing if '{0}' is readonly...", path);
				return new FileInfo(path).IsReadOnly;
			});
		}

		/// <inheritdoc />
		public Task WriteAllBytes(string path, byte[] bytes)
		{
			Path2.ThrowIfPathIsInvalid(path);
			if (bytes == null)
				throw new ArgumentNullException(nameof(bytes));

			// I don't like this copy but I also don't want to allow the user to introduce
			// race conditions... What to do?
			var copy = new byte[bytes.Length];
			bytes.CopyTo(copy, 0);

			path = CaptureFullPath(path);

			return _ioScheduler.StartNew(() =>
			{
				Log.DebugFormat("Writing {0} bytes to '{1}'...", copy, path);
				File.WriteAllBytes(path, copy);
			});
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
				});
		}

		/// <inheritdoc />
		public Task<Stream> OpenRead(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			return _ioScheduler.StartNew<Stream>(() =>
			{
				Log.DebugFormat("Opening file '{0}' for reading...", path);
				return File.OpenRead(path);
			});
		}

		/// <inheritdoc />
		public Task<Stream> OpenWrite(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			return _ioScheduler.StartNew<Stream>(() =>
			{
				Log.DebugFormat("Opening file '{0}' for writing...", path);
				return File.OpenWrite(path);
			});
		}

		/// <inheritdoc />
		public Task Write(string path, Stream stream)
		{
			Path2.ThrowIfPathIsInvalid(path);
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			path = CaptureFullPath(path);
			return _ioScheduler.StartNew(() =>
			{
				Log.DebugFormat("Writing data to {0}", path);
				using (var targetStream = File.OpenWrite(path))
				{
					stream.CopyTo(targetStream);
					targetStream.SetLength(targetStream.Position);
				}
			});
		}

		/// <inheritdoc />
		public Task CopyFile(string sourceFileName, string destFileName)
		{
			Path2.ThrowIfPathIsInvalid(sourceFileName, nameof(sourceFileName));
			Path2.ThrowIfPathIsInvalid(destFileName, nameof(destFileName));

			var sourceFilePath = CaptureFullPath(sourceFileName);
			var destFilePath = CaptureFullPath(destFileName);
			return _ioScheduler.StartNew(() =>
			{
				Log.DebugFormat("Copying '{0}' to '{1}'", sourceFilePath, destFilePath);
				File.Copy(sourceFilePath, destFilePath);
			});
		}

		/// <inheritdoc />
		public Task DeleteFile(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			return _ioScheduler.StartNew(() =>
			{
				Log.DebugFormat("Deleting file '{0}'", path);
				File.Delete(path);
			});
		}

		/// <inheritdoc />
		public IFilesystemWatchdog Watchdog => _watchdog;

		/// <summary>
		///     **Always** equals <see cref="Directory.GetCurrentDirectory" />.
		///     Changing this property changes <see cref="Directory.SetCurrentDirectory" />.
		/// </summary>
		public string CurrentDirectory
		{
			get { return Directory.GetCurrentDirectory(); }
			set { Directory.SetCurrentDirectory(value); }
		}

		/// <inheritdoc />
		public IDirectoryInfoAsync Current => DirectoryInfoAsync.FromPath(this, CurrentDirectory);

		/// <inheritdoc />
		public Task<IEnumerable<IDirectoryInfoAsync>> Roots
		{
			get
			{
				return _ioScheduler.StartNew<IEnumerable<IDirectoryInfoAsync>>(() =>
				{
					var drives = DriveInfo.GetDrives();
					return drives.Select(x => DirectoryInfoAsync.FromRoot(this, x.Name)).ToList();
				});
			}
		}

		/// <inheritdoc />
		public Task<IDirectoryInfoAsync> CreateDirectory(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);

			return _ioScheduler.StartNew<IDirectoryInfoAsync>(() =>
			{
				Log.DebugFormat("Creating directory '{0}'...", path);
				var info = Directory.CreateDirectory(path);
				return DirectoryInfoAsync.FromPath(this, info.FullName);
			});
		}

		/// <inheritdoc />
		public Task DeleteDirectory(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			return _ioScheduler.StartNew(() =>
			{
				Log.DebugFormat("Deleting directory '{0}'...", path);
				Directory.Delete(path);
			});
		}

		/// <inheritdoc />
		public Task DeleteDirectory(string path, bool recursive)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			return _ioScheduler.StartNew(() =>
			{
				Log.DebugFormat("Deleting directory '{0}'{1}...", path, recursive ? " recursive" : string.Empty);
				Directory.Delete(path, recursive);
			});
		}

		/// <inheritdoc />
		public Task<bool> DirectoryExists(string path)
		{
			if (!Path2.IsValidPath(path))
				return Task.FromResult(false);

			path = CaptureFullPath(path);
			return _ioScheduler.StartNew(() =>
			{
				Log.DebugFormat("Testing if directory '{0}' exists...", path);
				return Directory.Exists(path);
			});
		}

		/// <inheritdoc />
		public Task<Stream> CreateFile(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			return _ioScheduler.StartNew<Stream>(() => new FileStream(path, FileMode.Create,
				FileAccess.Read | FileAccess.Write,
				FileShare.None));
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
				var current = Directory.GetCurrentDirectory();
				var abs = Path.Combine(current, path);
				return abs;
			}

			return path;
		}

		internal Task<IFileInfo> CaptureFile(string fullName)
		{
			return _ioScheduler.StartNew<IFileInfo>(() =>
			{
				Log.DebugFormat("Capturing properties of '{0}'...", fullName);

				var info = new FileInfo(fullName);
				var directory = CaptureDirectoryBranch(info.DirectoryName);
				return new FileInfo2(directory, fullName, info.Length, info.IsReadOnly, info.Exists);
			});
		}

		internal Task<IDirectoryInfo> CaptureDirectory(string path)
		{
			return _ioScheduler.StartNew<IDirectoryInfo>(() =>
			{
				Log.DebugFormat("Capturing properties of '{0}'...", path);
				return CaptureDirectoryBranch(path);
			});
		}

		private DirectoryInfo2 CaptureDirectoryBranch(string path)
		{
			var info = new DirectoryInfo(path);
			var root = new DirectoryInfo2(null, null, path, Directory.Exists(path));
			if (ReferenceEquals(info, info.Root))
			{
				return root;
			}

			return Capture(root, info);
		}

		private DirectoryInfo2 Capture(DirectoryInfo2 root, DirectoryInfo info)
		{
			if (info == null)
				return null;

			if (info.Parent == null)
				return root;

			var parentSnapshot = Capture(root, info.Parent);
			var snapshot = new DirectoryInfo2(root, parentSnapshot, info.FullName, info.Exists);
			return snapshot;
		}
	}
}