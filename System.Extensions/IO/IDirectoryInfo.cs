namespace System.IO
{
	public interface IDirectoryInfo
	{
		string Name { get; }
		bool Exists { get; }
	}
}