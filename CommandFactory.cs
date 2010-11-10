namespace m
{
	public class CommandFactory : ICommandFactory
	{
		public ICommand NewCommand(CommandType type)
		{
			return new Command(type);
		}

		public ICommand NewChangeStateCommand(CommandType type, int magnitude)
		{
			return new ChangeStateCommand(type, magnitude);
		}

		public ICommand NewFileCommand(CommandType type, ISearchQuery searchQuery)
		{
			return new FileCommand(type, searchQuery);
		}
		
		public ICommand NewLoopCommand(LoopType loopType)
		{
			return new LoopCommand(loopType);
		}
	}
}