namespace System.IO
{
	internal abstract class FileInfo2
		: IFileInfo
	{
		private readonly IFilesystem _filesystem;
		private readonly string _name;
		private readonly string _fullPath;
		private readonly string _directoryPath;

		protected FileInfo2(IFilesystem filesystem,
		                    string fullPath)
		{
			_filesystem = filesystem ?? throw new ArgumentNullException(nameof(filesystem));
			_fullPath = fullPath;
			_directoryPath = Path.GetDirectoryName(fullPath);
			_name = Path.GetFileName(fullPath);
		}

		public override int GetHashCode()
		{
			return new PathComparer().GetHashCode(_fullPath);
		}

		public override bool Equals(object obj)
		{
			var other = obj as FileInfo2;
			if (other == null)
				return false;

			return new PathComparer().Equals(_fullPath, other._fullPath);
		}

		public IDirectoryInfo Directory
		{
			get { return _filesystem.GetDirectoryInfo(_directoryPath); }
		}

		public string DirectoryName
		{
			get { return _directoryPath; }
		}

		public string Name => _name;

		public string FullPath => _fullPath;

		public long Length
		{
			get
			{
				return _filesystem.FileLength(_fullPath);
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return _filesystem.IsFileReadOnly(_fullPath);
			}
		}

		public bool Exists
		{
			get
			{
				return _filesystem.FileExists(_fullPath);
			}
		}

		public abstract DateTime CreationTimeUtc
		{
			get;
		}

		public abstract DateTime LastAccessTimeUtc
		{
			get;
		}

		public abstract DateTime LastWriteTimeUtc
		{
			get;
		}

		public Stream Create()
		{
			return _filesystem.CreateFile(_fullPath);
		}

		public void Delete()
		{
			_filesystem.DeleteFile(_fullPath);
		}

		public override string ToString()
		{
			return "{" + FullPath + "}";
		}
	}
}