namespace System.IO.Extensions
{
	public interface IDirectoryInfo
	{
		string Name { get; }
		bool Exists { get; }
	}
}