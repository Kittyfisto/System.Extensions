using System.Collections.Generic;
using System.Diagnostics.Contracts;
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

		private readonly ISerialTaskScheduler _scheduler;

		public Filesystem(ISerialTaskScheduler scheduler)
		{
			if (scheduler == null)
				throw new ArgumentNullException(nameof(scheduler));

			_scheduler = scheduler;
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateFiles(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			path = CaptureFullPath(path);
			return _scheduler.StartNew<IReadOnlyList<string>>(() =>
			{
				Log.DebugFormat("Enumerating files of directory '{0}'...", path);
				return Directory.EnumerateFiles(path).ToList();
			});
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateFiles(string path, string searchPattern)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			path = CaptureFullPath(path);
			return _scheduler.StartNew<IReadOnlyList<string>>(() =>
			{
				Log.DebugFormat("Enumerating files of directory '{0}'...", path);
				return Directory.EnumerateFiles(path, searchPattern).ToList();
			});
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			path = CaptureFullPath(path);
			return _scheduler.StartNew<IReadOnlyList<string>>(() =>
			{
				Log.DebugFormat("Enumerating files of directory '{0}'...", path);
				return Directory.EnumerateFiles(path, searchPattern, searchOption).ToList();
			});
		}

		public Task<IReadOnlyList<string>> EnumerateDirectories(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			path = CaptureFullPath(path);
			return _scheduler.StartNew<IReadOnlyList<string>>(() =>
			{
				Log.DebugFormat("Enumerating directories of directory '{0}'...", path);
				return Directory.EnumerateDirectories(path).ToList();
			});
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateDirectories(string path, string searchPattern)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			path = CaptureFullPath(path);
			return _scheduler.StartNew<IReadOnlyList<string>>(() =>
			{
				Log.DebugFormat("Enumerating directories of directory '{0}'...", path);
				return Directory.EnumerateDirectories(path, searchPattern).ToList();
			});
		}

		/// <inheritdoc />
		public Task<IReadOnlyList<string>> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			path = CaptureFullPath(path);
			return _scheduler.StartNew<IReadOnlyList<string>>(() =>
			{
				Log.DebugFormat("Enumerating directories of directory '{0}'...", path);
				return Directory.EnumerateDirectories(path, searchPattern, searchOption).ToList();
			});
		}

		/// <inheritdoc />
		public Task<IFileInfo> GetFileInfo(string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException(nameof(fileName));

			fileName = CaptureFullPath(fileName);
			return _scheduler.StartNew(() =>
			{
				Log.DebugFormat("Obtaining information about file '{0}'...", fileName);
				return FileInfo2.Capture(fileName);
			});
		}

		/// <inheritdoc />
		public Task<IDirectoryInfo> GetDirectoryInfo(string directoryName)
		{
			if (directoryName == null)
				throw new ArgumentNullException(nameof(directoryName));

			directoryName = CaptureFullPath(directoryName);
			return _scheduler.StartNew(() =>
			{
				Log.DebugFormat("Obtaining information about directory '{0}'...", directoryName);
				return DirectoryInfo2.Capture(directoryName);
			});
		}

		/// <inheritdoc />
		public Task<bool> FileExists(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return Task.FromResult(false);

			path = CaptureFullPath(path);
			return _scheduler.StartNew(() =>
			{
				Log.DebugFormat("Testing if file '{0}' exists...", path);
				return File.Exists(path);
			});
		}

		/// <inheritdoc />
		public Task WriteAllBytes(string path, byte[] bytes)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (bytes == null)
				throw new ArgumentNullException(nameof(bytes));

			// I don't like this copy but I also don't want to allow the user to introduce
			// race conditions... What to do?
			var copy = new byte[bytes.Length];
			bytes.CopyTo(copy, 0);

			path = CaptureFullPath(path);

			return _scheduler.StartNew(() =>
			{
				Log.DebugFormat("Writing {0} bytes to '{1}'...", copy, path);
				File.WriteAllBytes(path, copy);
			});
		}

		/// <inheritdoc />
		public Task<Stream> OpenRead(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			path = CaptureFullPath(path);
			return _scheduler.StartNew<Stream>(() =>
			{
				Log.DebugFormat("Opening file '{0}' for reading...", path);
				return File.OpenRead(path);
			});
		}

		/// <inheritdoc />
		public Task<Stream> OpenWrite(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			path = CaptureFullPath(path);
			return _scheduler.StartNew<Stream>(() =>
			{
				Log.DebugFormat("Opening file '{0}' for writing...", path);
				return File.OpenWrite(path);
			});
		}

		/// <inheritdoc />
		public Task Write(string path, Stream stream)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			path = CaptureFullPath(path);

			int capacity = stream.CanSeek ? (int)(stream.Length - stream.Position) : 0;

			// We'll copy up to 4k at a time, but we don't want to create a 4k
			// buffer for 1 byte of data...
			byte[] buffer = new byte[Math.Min(capacity, 4096)];
			byte[] copy;
			using (MemoryStream ms = new MemoryStream(capacity))
			{
				int read;
				while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
				{
					ms.Write(buffer, 0, read);
				}
				copy = ms.ToArray();
			}

			return _scheduler.StartNew(() =>
			{
				Log.DebugFormat("Writing {0} bytes to {1}", copy, path);
				File.WriteAllBytes(path, copy);
			});
		}

		/// <inheritdoc />
		public Task DeleteFile(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			path = CaptureFullPath(path);
			return _scheduler.StartNew(() =>
			{
				Log.DebugFormat("Deleting file '{0}'", path);
				File.Delete(path);
			});
		}

		/// <inheritdoc />
		public Task<IDirectoryInfo> CreateDirectory(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			path = CaptureFullPath(path);
			return _scheduler.StartNew(() =>
			{
				Log.DebugFormat("Creating directory '{0}'...", path);
				return DirectoryInfo2.Capture(Directory.CreateDirectory(path));
			});
		}

		/// <inheritdoc />
		public Task DeleteDirectory(string path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			path = CaptureFullPath(path);
			return _scheduler.StartNew(() =>
			{
				Log.DebugFormat("Deleting directory '{0}'...", path);
				Directory.Delete(path);
			});
		}

		/// <inheritdoc />
		public Task DeleteDirectory(string path, bool recursive)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));

			path = CaptureFullPath(path);
			return _scheduler.StartNew(() =>
			{
				Log.DebugFormat("Deleting directory '{0}'{1}...", path, recursive ? " recursive" : string.Empty);
				Directory.Delete(path, recursive);
			});
		}

		/// <inheritdoc />
		public Task<bool> DirectoryExists(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
				return Task.FromResult(false);

			path = CaptureFullPath(path);
			return _scheduler.StartNew(() =>
			{
				Log.DebugFormat("Testing if directory '{0}' exists...", path);
				return Directory.Exists(path);
			});
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
	}
}