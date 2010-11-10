namespace m
{
	public interface ICommand
	{
		CommandType CommandType { get; }
	}

	public interface IChangeStateCommand : ICommand
	{
		int Magnitude { get; }
	}

	public interface IFileCommand : ICommand
	{
		ISearchQuery Search { get; }
	}
	
	public interface ILoopCommand : ICommand
	{
		LoopType LoopType { get; }
	}

	public enum CommandType
	{
		CurrentTrackDetails,
		PlayShortestTrack,
		PlayRandomTrack,
		PlayQueryAsList,
		PlayPlaylist,
		ListHistory,
		SkipToTrack,
		ListFiles,
		PlayPause,
		SkipTrack,
		SkipTime,
		Version,
		Volume,
		Repeat,
		Ignore,
		Stop,
		Loop,
		Help,
		Exit
	}

	public enum LoopType
	{
		Sequential,
		Random,
		None
	}
}