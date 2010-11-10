namespace m
{
	public interface IPlatform
	{
		bool IsWindows { get; }
		bool IsUnix { get; }
		bool IsMono { get; }
		bool IsDotNet { get; }		
	}
}