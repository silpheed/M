namespace m
{
	public class Command : ICommand
	{
		private readonly CommandType _commandType;
		
		public Command(CommandType type)
		{
			_commandType = type;
		}
		
		public CommandType CommandType
		{
			get { return _commandType; }
		}

	}

	public class ChangeStateCommand : Command, IChangeStateCommand
	{
		private readonly int _magnitude;

		public ChangeStateCommand(CommandType type, int magnitude) : base(type)
		{
			_magnitude = magnitude;
		}

		public int Magnitude
		{
			get { return _magnitude; }
		}
	}

	public class FileCommand : Command, IFileCommand
	{
		private readonly ISearchQuery _search;

		public FileCommand(CommandType type, ISearchQuery searchQuery) : base(type)
		{
			_search = searchQuery;
		}

		public ISearchQuery Search
		{
			get { return _search; }
		}
	}
	
	public class LoopCommand : Command, ILoopCommand
	{
		private readonly LoopType _loopType;
		
		public LoopCommand(LoopType loopType)
			: base(CommandType.Loop)
		{
			_loopType = loopType;
		}

		public LoopType LoopType
		{
			get { return _loopType; }
		}
	}

}