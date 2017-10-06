using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace System.IO
{
	internal sealed class DirectoryInfoAsync
		: IDirectoryInfoAsync
	{
		private readonly Filesystem _filesystem;
		private readonly string _name;
		private readonly string _fullName;
		private readonly IDirectoryInfoAsync _parent;

		public DirectoryInfoAsync(Filesystem filesystem, IDirectoryInfoAsync root, IDirectoryInfoAsync parent,
			string fullName, string name)
		{
			_filesystem = filesystem;
			Root = parent != null ? root : this;
			_parent = parent;
			_name = name;
			_fullName = fullName;
		}

		public DirectoryInfoAsync(Filesystem filesystem, IDirectoryInfoAsync root, IDirectoryInfoAsync parent,
			string fullName)
			: this(filesystem, root, parent, fullName, Directory2.GetDirName(fullName))
		{
		}

		public IDirectoryInfoAsync Root { get; }

		public IDirectoryInfoAsync Parent => _parent;

		public string Name => _name;

		public string FullName => _fullName;

		public Task<bool> Exists => _filesystem.DirectoryExists(FullName);

		public Task<IDirectoryInfo> Capture()
		{
			return _filesystem.CaptureDirectory(FullName);
		}

		public async Task<IEnumerable<IFileInfoAsync>> EnumerateFiles()
		{
			var fileNames = await _filesystem.EnumerateFiles(FullName);
			return fileNames.Select(x => new FileInfoAsync(_filesystem, x)).ToList();
		}

		public async Task<IEnumerable<IFileInfoAsync>> EnumerateFiles(string searchPattern)
		{
			var fileNames = await _filesystem.EnumerateFiles(FullName, searchPattern);
			return fileNames.Select(x => new FileInfoAsync(_filesystem, x)).ToList();
		}

		public async Task<IEnumerable<IFileInfoAsync>> EnumerateFiles(string searchPattern, SearchOption searchOption)
		{
			var fileNames = await _filesystem.EnumerateFiles(FullName, searchPattern, searchOption);
			return fileNames.Select(x => new FileInfoAsync(_filesystem, x)).ToList();
		}

		public Task Create()
		{
			return _filesystem.CreateDirectory(_name);
		}

		public Task<IDirectoryInfoAsync> CreateSubdirectory(string path)
		{
			if (!Path.IsPathRooted(path))
				path = Path.Combine(FullName, path);

			return _filesystem.CreateDirectory(path);
		}

		public override string ToString()
		{
			return "{" + FullName + "}";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filesystem"></param>
		/// <param name="driveName"></param>
		/// <returns></returns>
		[Pure]
		public static DirectoryInfoAsync FromRoot(Filesystem filesystem, string driveName)
		{
			var root = new DirectoryInfoAsync(filesystem, null, null, driveName, driveName);
			return root;
		}

		[Pure]
		public static DirectoryInfoAsync RelativeTo(DirectoryInfoAsync parent, string directoryName)
		{
			var fullName = Path.Combine(parent.FullName, directoryName);
			return new DirectoryInfoAsync(parent._filesystem,
				parent.Root,
				parent,
				fullName,
				directoryName);
		}

		/// <summary>
		///     Creates a new directory info for the given path.
		/// </summary>
		/// <remarks>
		///     Creates parent and root objects for the entire path.
		/// </remarks>
		/// <param name="filesystem"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		[Pure]
		public static DirectoryInfoAsync FromPath(Filesystem filesystem, string path)
		{
			var components = Directory2.Split(path);
			var root = FromRoot(filesystem, components[0]);
			var directory = root;
			for (var i = 1; i < components.Count; ++i)
			{
				var directoryName = components[i];
				directory = RelativeTo(directory, directoryName);
			}
			return directory;
		}
	}
}