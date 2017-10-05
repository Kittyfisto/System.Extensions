using System.Threading.Tasks;

namespace System.IO
{
	internal sealed class FileInfoAsync
		: IFileInfoAsync
	{
		private readonly Filesystem _filesystem;
		private readonly string _name;
		private readonly string _fullName;

		public FileInfoAsync(Filesystem filesystem, string fullName)
		{
			_filesystem = filesystem;
			_fullName = fullName;
			_name = Path.GetFileName(fullName);
		}

		public string Name => _name;

		public string FullPath => _fullName;

		public Task<long> Length => _filesystem.FileLength(_fullName);

		public Task<bool> IsReadOnly => _filesystem.IsFileReadOnly(_fullName);

		public Task<bool> Exists => _filesystem.FileExists(_fullName);
	}
}