using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.IO
{
	/// <summary>
	/// This class is responsible for pro
	/// </summary>
	internal sealed class DirectoryInfo2
		: IDirectoryInfo
	{
		private readonly IFilesystem _filesystem;
		private readonly DirectoryInfo2 _root;
		private readonly DirectoryInfo2 _parent;
		private readonly string _fullName;
		private readonly string _name;

		public static DirectoryInfo2 Create(IFilesystem filesystem, string fullName)
		{
			var subPaths = Directory2.Tokenise(fullName);

			var root = new DirectoryInfo2(filesystem, null, null, subPaths[0]);
			var parent = root;
			for (int i = 1; i < subPaths.Count; ++i)
			{
				var subPath = subPaths[i];
				parent = new DirectoryInfo2(filesystem, root, parent, subPath);
			}

			return parent;
		}

		public DirectoryInfo2(IFilesystem filesystem,
		                      DirectoryInfo2 root,
		                      DirectoryInfo2 parent,
		                      string fullName)
		{
			if (filesystem == null)
				throw new ArgumentNullException(nameof(filesystem));
			if (fullName == null)
				throw new ArgumentNullException(nameof(fullName));

			_filesystem = filesystem;
			_root = root ?? this;
			_parent = parent;
			_fullName = fullName;
			_name = Path.GetFileName(fullName);
			if (string.IsNullOrEmpty(_name))
				_name = fullName;
		}

		public IDirectoryInfo Root
		{
			get
			{
				return _root;
			}
		}

		public IDirectoryInfo Parent
		{
			get
			{
				return _parent;
			}
		}

		public override int GetHashCode()
		{
			return new PathComparer().GetHashCode(_fullName);
		}

		public override bool Equals(object obj)
		{
			var other = obj as DirectoryInfo2;
			if (other == null)
				return false;

			return new PathComparer().Equals(_fullName, other._fullName);
		}

		public string Name => _name;

		public string FullName => _fullName;

		public bool Exists => _filesystem.DirectoryExists(_fullName);

		public bool FileExists(string filename)
		{
			var path = CaptureFullPath(filename);
			return _filesystem.FileExists(path);
		}

		public IEnumerable<IFileInfo> EnumerateFiles()
		{
			return EnumerateFiles("*", SearchOption.TopDirectoryOnly);
		}

		public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern)
		{
			return EnumerateFiles(searchPattern, SearchOption.TopDirectoryOnly);
		}

		public IEnumerable<IFileInfo> EnumerateFiles(string searchPattern, SearchOption searchOption)
		{
			var files = new List<IFileInfo>();
			foreach (var fileName in _filesystem.EnumerateFiles(_fullName, searchPattern, searchOption))
			{
				files.Add(_filesystem.GetFileInfo(fileName));
			}
			return files;
		}

		/// <inheritdoc />
		public void Create()
		{
			_filesystem.CreateDirectory(_fullName);
		}

		public void Delete()
		{
			_filesystem.DeleteDirectory(_fullName);
		}

		public IDirectoryInfo CreateSubdirectory(string path)
		{
			path = CaptureFullPath(path);
			return _filesystem.CreateDirectory(path);
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