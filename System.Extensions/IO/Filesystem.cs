using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO.FS;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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

		private readonly FilesystemWatchdog _watchdog;

		/// <summary>
		/// Initializes this object.
		/// </summary>
		/// <param name="taskScheduler">The scheduler used for timers (to trigger periodic I/O tasks)</param>
		public Filesystem(ITaskScheduler taskScheduler)
		{
			if (taskScheduler == null)
				throw new ArgumentNullException(nameof(taskScheduler));

			_watchdog = new FilesystemWatchdog(this, taskScheduler);
		}

		/// <inheritdoc />
		public IReadOnlyList<string> EnumerateFiles(string path,
			string searchPattern = null,
			SearchOption searchOption = SearchOption.TopDirectoryOnly,
			bool tolerateNonExistantPath = false)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
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
		}

		/// <inheritdoc />
		public IReadOnlyList<string> EnumerateDirectories(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			Log.DebugFormat("Enumerating directories of directory '{0}'...", path);
			return Directory.EnumerateDirectories(path).ToList();
		}

		/// <inheritdoc />
		public IReadOnlyList<string> EnumerateDirectories(string path, string searchPattern)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			Log.DebugFormat("Enumerating directories of directory '{0}'...", path);
			return Directory.EnumerateDirectories(path, searchPattern).ToList();
		}

		/// <inheritdoc />
		public IReadOnlyList<string> EnumerateDirectories(string path,
			string searchPattern,
			SearchOption searchOption)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			Log.DebugFormat("Enumerating directories of directory '{0}'...", path);
			return Directory.EnumerateDirectories(path, searchPattern, searchOption).ToList();
		}

		/// <inheritdoc />
		public IFileInfo GetFileInfo(string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException(nameof(fileName));

			if (!Path2.IsValidPath(fileName))
				throw new ArgumentException(nameof(fileName));

			fileName = CaptureFullPath(fileName);
			return CaptureFile(fileName);
		}

		/// <inheritdoc />
		public IDirectoryInfo GetDirectoryInfo(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			if (!Path2.IsValidPath(path))
				throw new ArgumentException(nameof(path));

			var fullPath = CaptureFullPath(path);
			return CreateDirectoryInfo(fullPath);
		}

		/// <inheritdoc />
		public bool FileExists(string path)
		{
			if (!Path2.IsValidPath(path))
				return false;

			path = CaptureFullPath(path);
			Log.DebugFormat("Testing if file '{0}' exists...", path);
			return File.Exists(path);
		}

		/// <inheritdoc />
		public long FileLength(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			Log.DebugFormat("Getting length of '{0}'...", path);
			return new FileInfo(path).Length;
		}

		/// <inheritdoc />
		public bool IsFileReadOnly(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			Log.DebugFormat("Testing if '{0}' is readonly...", path);
			return new FileInfo(path).IsReadOnly;
		}

		/// <inheritdoc />
		public void WriteAllBytes(string path, byte[] bytes)
		{
			Path2.ThrowIfPathIsInvalid(path);
			if (bytes == null)
				throw new ArgumentNullException(nameof(bytes));

			path = CaptureFullPath(path);
			Log.DebugFormat("Writing {0} bytes to '{1}'...", bytes.Length, path);
			File.WriteAllBytes(path, bytes);
		}

		/// <inheritdoc />
		public void WriteAllText(string path, string contents)
		{
			File.WriteAllText(path, contents);
		}

		/// <inheritdoc />
		public void WriteAllText(string path, string contents, Encoding encoding)
		{
			File.WriteAllText(path, contents, encoding);
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
			return File.ReadAllText(path);
		}

		/// <inheritdoc />
		public string ReadAllText(string path, Encoding encoding)
		{
			return File.ReadAllText(path, encoding);
		}

		/// <inheritdoc />
		public IReadOnlyList<string> ReadAllLines(string path)
		{
			return File.ReadAllLines(path);
		}

		/// <inheritdoc />
		public IReadOnlyList<string> ReadAllLines(string path, Encoding encoding)
		{
			return File.ReadAllLines(path, encoding);
		}

		/// <inheritdoc />
		public Stream OpenRead(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			Log.DebugFormat("Opening file '{0}' for reading...", path);
			return File.OpenRead(path);
		}

		/// <inheritdoc />
		public Stream OpenWrite(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			Log.DebugFormat("Opening file '{0}' for writing...", path);
			return File.OpenWrite(path);
		}

		/// <inheritdoc />
		public void Write(string path, Stream stream)
		{
			Path2.ThrowIfPathIsInvalid(path);
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			path = CaptureFullPath(path);
			Log.DebugFormat("Writing data to {0}", path);
			using (var targetStream = File.OpenWrite(path))
			{
				stream.CopyTo(targetStream);
				targetStream.SetLength(targetStream.Position);
			}
		}

		/// <inheritdoc />
		public void CopyFile(string sourceFileName, string destFileName)
		{
			Path2.ThrowIfPathIsInvalid(sourceFileName, nameof(sourceFileName));
			Path2.ThrowIfPathIsInvalid(destFileName, nameof(destFileName));

			var sourceFilePath = CaptureFullPath(sourceFileName);
			var destFilePath = CaptureFullPath(destFileName);
			Log.DebugFormat("Copying '{0}' to '{1}'", sourceFilePath, destFilePath);
			File.Copy(sourceFilePath, destFilePath);
		}

		/// <inheritdoc />
		public void DeleteFile(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			Log.DebugFormat("Deleting file '{0}'", path);
			File.Delete(path);
		}

		/// <inheritdoc />
		public IFilesystemWatchdog Watchdog => _watchdog;

		/// <inheritdoc />
		public DateTime FileCreationTimeUtc(string fullPath)
		{
			return new FileInfo(fullPath).CreationTimeUtc;
		}

		/// <inheritdoc />
		public DateTime FileLastAccessTimeUtc(string fullPath)
		{
			return new FileInfo(fullPath).LastAccessTimeUtc;
		}

		/// <inheritdoc />
		public DateTime FileLastWriteTimeUtc(string fullPath)
		{
			return new FileInfo(fullPath).LastWriteTimeUtc;
		}

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
		public IDirectoryInfo Current => CreateDirectoryInfo(CurrentDirectory);

		/// <inheritdoc />
		public IEnumerable<IDirectoryInfo> Roots
		{
			get
			{
				var drives = DriveInfo.GetDrives();
				return drives.Select(x => CreateDirectoryInfo(x.Name)).ToList();
			}
		}

		/// <inheritdoc />
		public IDirectoryInfo CreateDirectory(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);

			Log.DebugFormat("Creating directory '{0}'...", path);
			var info = Directory.CreateDirectory(path);
			return CreateDirectoryInfo(info.FullName);
		}

		/// <inheritdoc />
		public void DeleteDirectory(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			Log.DebugFormat("Deleting directory '{0}'...", path);
			Directory.Delete(path);
		}

		/// <inheritdoc />
		public void DeleteDirectory(string path, bool recursive)
		{
			Path2.ThrowIfPathIsInvalid(path);

			path = CaptureFullPath(path);
			Log.DebugFormat("Deleting directory '{0}'{1}...", path, recursive ? " recursive" : string.Empty);
			Directory.Delete(path, recursive);
		}

		/// <inheritdoc />
		public bool DirectoryExists(string path)
		{
			if (!Path2.IsValidPath(path))
				return false;

			path = CaptureFullPath(path);
			Log.DebugFormat("Testing if directory '{0}' exists...", path);
			return Directory.Exists(path);
		}

		/// <inheritdoc />
		public Stream CreateFile(string path)
		{
			Path2.ThrowIfPathIsInvalid(path);
			return new FileStream(path, FileMode.Create,
			                      FileAccess.Read | FileAccess.Write,
			                      FileShare.None);
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

		internal IFileInfo CaptureFile(string fullName)
		{
			Log.DebugFormat("Capturing properties of '{0}'...", fullName);

			return new FileInfo2(this, fullName);
		}

		internal IDirectoryInfo CreateDirectoryInfo(string path)
		{
			Log.DebugFormat("Capturing properties of '{0}'...", path);
			return DirectoryInfo2.Create(this, path);
		}
	}
}