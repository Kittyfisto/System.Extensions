namespace System.IO
{
	internal sealed class FileInfo2
		: IFileInfo
	{
		private readonly DirectoryInfo2 _directory;
		private readonly string _fullPath;
		private readonly string _name;
		private readonly long _length;
		private readonly bool _isReadOnly;
		private readonly bool _exists;

		public FileInfo2(DirectoryInfo2 directory, string fullPath, long length, bool isReadOnly, bool exists)
		{
			_directory = directory;
			_fullPath = fullPath;
			_length = length;
			_isReadOnly = isReadOnly;
			_exists = exists;
			_name = Path.GetFileName(fullPath);
		}

		public override string ToString()
		{
			return "{" + _fullPath + "}";
		}

		public IDirectoryInfo Directory => _directory;

		public string DirectoryName => _directory.FullName;

		public string Name => _name;

		public string FullPath => _fullPath;

		public long Length => _length;

		public bool IsReadOnly => _isReadOnly;

		public bool Exists => _exists;
	}
}
