using System.Text;

namespace System.IO.InMemory
{
	internal sealed class InMemoryFile
	{
		private readonly string _fullPath;
		private readonly string _name;
		private MemoryStream _content;

		public InMemoryFile(string fullPath)
		{
			_fullPath = fullPath;
			_name = Path.GetFileName(fullPath);
			_content = new MemoryStream();
		}

		public string Name => _name;

		public string FullPath => _fullPath;

		public bool IsReadOnly => true;

		public void Print(StringBuilder builder)
		{
			builder.AppendFormat("{0} [File]", FullPath);
		}

		public override string ToString()
		{
			return "{" + FullPath + "}";
		}

		public Stream Content => _content;

		public long Length => _content.Length;

		public void Clear()
		{
			_content = new MemoryStream();
		}
	}
}