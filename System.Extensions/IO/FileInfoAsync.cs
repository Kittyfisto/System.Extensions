using System.Threading.Tasks;

namespace System.IO
{
	internal sealed class FileInfoAsync
		: IFileInfoAsync
	{
		private readonly Filesystem _filesystem;
		private readonly string _name;
		private readonly string _fullPath;

		public FileInfoAsync(Filesystem filesystem, string fullPath)
		{
			_filesystem = filesystem;
			_fullPath = fullPath;
			_name = Path.GetFileName(fullPath);
		}

		public override int GetHashCode()
		{
			return new PathComparer().GetHashCode(_fullPath);
		}

		public override bool Equals(object obj)
		{
			var other = obj as FileInfoAsync;
			if (other == null)
				return false;

			return new PathComparer().Equals(_fullPath, other._fullPath);
		}

		public override string ToString()
		{
			return "{" + _fullPath + "}";
		}

		public string Name => _name;

		public string FullPath => _fullPath;

		public Task<IFileInfo> Capture()
		{
			return _filesystem.CaptureFile(_fullPath);
		}

		public Task<long> Length => _filesystem.FileLength(_fullPath);

		public Task<bool> IsReadOnly => _filesystem.IsFileReadOnly(_fullPath);

		public Task<bool> Exists => _filesystem.FileExists(_fullPath);

		public Task<Stream> Create()
		{
			return _filesystem.CreateFile(_fullPath);
		}

		public Task Delete()
		{
			return _filesystem.DeleteFile(_fullPath);
		}
	}
}