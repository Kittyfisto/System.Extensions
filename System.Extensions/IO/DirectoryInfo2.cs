namespace System.IO
{
	internal sealed class DirectoryInfo2
		: IDirectoryInfo
	{
		private readonly DirectoryInfo2 _root;
		private readonly DirectoryInfo2 _parent;
		private readonly string _fullName;
		private readonly string _name;
		private readonly bool _exists;

		public DirectoryInfo2(DirectoryInfo2 root, DirectoryInfo2 parent, string fullName, bool exists)
		{
			_root = parent == null ? this : root;
			_parent = parent;
			_fullName = fullName;
			_exists = exists;
			_name = parent == null ? fullName : Directory2.GetDirName(fullName);
		}

		public override string ToString()
		{
			return "{" + _fullName + "}";
		}

		public IDirectoryInfo Root => _root;

		public IDirectoryInfo Parent => _parent;

		public string Name => _name;

		public string FullName => _fullName;

		public bool Exists => _exists;
	}
}