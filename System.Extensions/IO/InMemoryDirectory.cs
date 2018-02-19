using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace System.IO
{
	internal sealed class InMemoryDirectory
	{
		private readonly Dictionary<string, InMemoryFile> _files;
		private readonly string _name;
		private readonly Dictionary<string, InMemoryDirectory> _subDirectories;
		private readonly object _syncRoot;
		private readonly string _fullName;

		public InMemoryDirectory(InMemoryDirectory parent,
		                         string name)
		{
			_name = name;
			_fullName = parent != null ? Path.Combine(parent.FullName, name) : name;
			_syncRoot = new object();
			_subDirectories = new Dictionary<string, InMemoryDirectory>(new PathComparer());
			_files = new Dictionary<string, InMemoryFile>(new PathComparer());
		}

		public string FullName => _fullName;
		
		public string Name => _name;

		public IEnumerable<InMemoryDirectory> Subdirectories
		{
			get
			{
				lock (_syncRoot)
				{
					return _subDirectories.Values.ToList();
				}
			}
		}

		public IEnumerable<InMemoryFile> Files
		{
			get
			{
				lock (_syncRoot)
				{
					return _files.Values.ToList();
				}
			}
		}

		[Pure]
		public bool ContainsFile(string fileName)
		{
			lock (_syncRoot)
			{
				return _files.ContainsKey(fileName);
			}
		}

		public IReadOnlyList<string> EnumerateFiles(string searchPattern,
		                                            SearchOption searchOption)
		{
			var directories = new Stack<InMemoryDirectory>();
			var files = new List<string>();

			Predicate<string> isMatch;
			if (searchPattern != null)
			{
				var regex = CreateRegex(searchPattern);
				isMatch = regex.IsMatch;
			}
			else
			{
				isMatch = x => true;
			}

			directories.Push(this);
			while (directories.Count > 0)
			{
				var directory = directories.Pop();

				foreach (var file in directory.Files)
				{
					var fileName = file.FullPath;
					if (isMatch(fileName))
						files.Add(fileName);
				}

				if (searchOption == SearchOption.AllDirectories)
					foreach (var subDirectory in directory.Subdirectories)
						directories.Push(subDirectory);
			}

			return files;
		}

		public Stream CreateFile(string fileName)
		{
			lock (_syncRoot)
			{
				InMemoryFile file;
				if (_files.TryGetValue(fileName, out file))
				{
					file.Clear();
				}
				else
				{
					var fullPath = Path.Combine(FullName, fileName);
					file = new InMemoryFile(fullPath);
					_files.Add(fileName, file);
				}

				return new StreamProxy(file.Content);
			}
		}

		public Stream OpenReadSync(string fileName)
		{
			lock (_syncRoot)
			{
				InMemoryFile file;
				if (!_files.TryGetValue(fileName, out file))
					throw new FileNotFoundException();

				var stream = new StreamProxy(file.Content) {Position = 0};
				return stream;
			}
		}

		public Stream OpenWriteSync(string fileName)
		{
			lock (_syncRoot)
			{
				InMemoryFile file;
				if (!_files.TryGetValue(fileName, out file))
					return null;

				file.Clear();
				var stream = new StreamProxy(file.Content) {Position = 0};
				return stream;
			}
		}

		public bool TryDeleteFile(string fileName)
		{
			lock (_syncRoot)
			{
				return _files.Remove(fileName);
			}
		}

		public InMemoryDirectory CreateSubdirectory(string directoryName)
		{
			lock (_syncRoot)
			{
				InMemoryDirectory directory;
				if (!_subDirectories.TryGetValue(directoryName, out directory))
				{
					directory = new InMemoryDirectory(this, directoryName);
					_subDirectories.Add(directoryName, directory);
				}
				return directory;
			}
		}

		public bool TryGetDirectory(string directoryName, out InMemoryDirectory directory)
		{
			lock (_syncRoot)
			{
				return _subDirectories.TryGetValue(directoryName, out directory);
			}
		}

		public bool DeleteSubdirectory(string directoryName, bool recursive, out bool isEmpty)
		{
			lock (_syncRoot)
			{
				InMemoryDirectory directory;
				if (_subDirectories.TryGetValue(directoryName, out directory))
				{
					if (directory.Files.Any())
					{
						// You wan never delete an entire directory tree which
						// includes files
						isEmpty = false;
						return false;
					}

					if (!recursive && directory.Subdirectories.Any())
					{
						isEmpty = false;
						return false;
					}

					_subDirectories.Remove(directoryName);
					isEmpty = true;
					return true;
				}

				isEmpty = true;
				return false;
			}
		}

		public void Print(StringBuilder builder)
		{
			lock (_syncRoot)
			{
				builder.AppendFormat("{0} [Drive]", FullName);
				builder.AppendLine();
				foreach (var directory in _subDirectories.Values.OrderBy(x => x.Name))
				{
					directory.Print(builder);
					builder.AppendLine();
				}
				foreach (var file in _files.Values.OrderBy(x => x.Name))
				{
					file.Print(builder);
					builder.AppendLine();
				}
			}
		}

		public bool TryGetFile(string fileName, out InMemoryFile file)
		{
			lock (_syncRoot)
			{
				return _files.TryGetValue(fileName, out file);
			}
		}

		[Pure]
		private static Regex CreateRegex(string searchPattern)
		{
			var regexPattern = "^" + Regex.Escape(searchPattern)
			                              .Replace(@"\*", ".*")
			                              .Replace(@"\?", ".")
			                       + "$";
			var regex = new Regex(regexPattern);
			return regex;
		}
	}
}