using System.Text;

namespace System.IO.InMemory
{
	internal sealed class InMemoryFile
	{
		private readonly DateTime _created;
		private readonly string _fullPath;
		private readonly string _name;
		private MemoryStream _content;
		private DateTime _lastAccessed;
		private DateTime _lastWritten;

		public InMemoryFile(string fullPath)
		{
			_created = DateTime.UtcNow;
			_lastWritten = _lastAccessed = _created;
			_fullPath = fullPath;
			_name = Path.GetFileName(fullPath);
			_content = new MemoryStream();
		}

		public DateTime LastAccessed
		{
			get { return _lastAccessed; }
		}

		public DateTime LastWritten
		{
			get { return _lastWritten; }
		}

		public DateTime Created
		{
			get { return _created; }
		}

		public string Name
		{
			get { return _name; }
		}

		public string FullPath
		{
			get { return _fullPath; }
		}

		public bool IsReadOnly
		{
			get { return true; }
		}

		public Stream Content
		{
			get
			{
				_lastAccessed = DateTime.UtcNow;
				_lastWritten = _lastAccessed;
				return _content;
			}
		}

		public long Length
		{
			get { return _content.Length; }
		}

		public void Print(StringBuilder builder)
		{
			builder.AppendFormat("{0} [File]", FullPath);
		}

		public override string ToString()
		{
			return "{" + FullPath + "}";
		}

		public void Clear()
		{
			_content = new MemoryStream();
			_lastWritten = DateTime.UtcNow;
		}
	}
}