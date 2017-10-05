using System.Collections.Generic;
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

		public DirectoryInfoAsync(Filesystem filesystem, string fullName, string name)
		{
			_filesystem = filesystem;
			_name = name;
			_fullName = fullName;
		}

		public DirectoryInfoAsync(Filesystem filesystem, string fullName)
			: this(filesystem, fullName, Path.GetDirectoryName(fullName))
		{}

		public string Name => _name;

		public string FullName => _fullName;

		public Task<bool> Exists => _filesystem.DirectoryExists(_fullName);

		public Task<IDirectoryInfo> Capture()
		{
			return _filesystem.CaptureDirectory(_fullName);
		}

		public async Task<IEnumerable<IFileInfoAsync>> EnumerateFiles()
		{
			var fileNames = await _filesystem.EnumerateFiles(_fullName);
			return fileNames.Select(x => new FileInfoAsync(_filesystem, x)).ToList();
		}

		public async Task<IEnumerable<IFileInfoAsync>> EnumerateFiles(string searchPattern)
		{
			var fileNames = await _filesystem.EnumerateFiles(_fullName, searchPattern);
			return fileNames.Select(x => new FileInfoAsync(_filesystem, x)).ToList();
		}

		public async Task<IEnumerable<IFileInfoAsync>> EnumerateFiles(string searchPattern, SearchOption searchOption)
		{
			var fileNames = await _filesystem.EnumerateFiles(_fullName, searchPattern, searchOption);
			return fileNames.Select(x => new FileInfoAsync(_filesystem, x)).ToList();
		}
	}
}