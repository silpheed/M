namespace m
{
	public interface ICommandFactory
	{
		ICommand NewCommand(CommandType type);
		ICommand NewChangeStateCommand(CommandType type, int magnitude);
		ICommand NewFileCommand(CommandType type, ISearchQuery searchQuery);
		ICommand NewLoopCommand(LoopType loopType);
	}
}