using MbUnit.Framework;

namespace m.tests
{
	//due to the design of Command and its children, this fixture doubles as a fixture for the Command family.
	[TestFixture]
	public class CommandFactoryFixture : MockingFixture
	{
		private ICommandFactory commandFactory;

		public override void SetUp()
		{
			commandFactory = new CommandFactory();
		}

		[Test]
		[Row(CommandType.Exit)]
		[Row(CommandType.PlayPause)]
		[Row(CommandType.Repeat)]
		[Row(CommandType.Stop)]
		[Row(CommandType.Ignore)]
		[Row(CommandType.ListHistory)]
		public void PlainCommandObjectCreatedIsSameAsAConstructedObject(CommandType commandType)
		{
			ICommand commandFromFactory = commandFactory.NewCommand(commandType);
			ICommand commandConstructedDirectly = new Command(commandType);

			//not worth implementing Command.Equals() just for testing
			Assert.IsNotNull(commandFromFactory);
			Assert.AreEqual(typeof(Command), commandFromFactory.GetType());
			Assert.AreEqual(commandConstructedDirectly.CommandType, commandFromFactory.CommandType);
		}

		[Test]
		[Row(CommandType.SkipTime, 10)]
		[Row(CommandType.SkipTrack, 10)]
		[Row(CommandType.SkipToTrack, 10)]
		[Row(CommandType.Volume, 10)]
		public void ChangeStateCommandObjectCreatedIsSameAsAConstructedObject(CommandType commandType, int magnitude)
		{
			IChangeStateCommand commandFromFactory = (ChangeStateCommand) commandFactory.NewChangeStateCommand(commandType, magnitude);
			IChangeStateCommand commandConstructedDirectly = new ChangeStateCommand(commandType, magnitude);

			Assert.IsNotNull(commandFromFactory);
			Assert.AreEqual(typeof(ChangeStateCommand), commandFromFactory.GetType());
			Assert.AreEqual(commandConstructedDirectly.CommandType, commandFromFactory.CommandType);
			Assert.AreEqual(commandConstructedDirectly.Magnitude, commandFromFactory.Magnitude);
		}

		[Test]
		[Row(CommandType.PlayShortestTrack)]
		[Row(CommandType.PlayRandomTrack)]
		[Row(CommandType.PlayPlaylist)]
		[Row(CommandType.PlayQueryAsList)]
		[Row(CommandType.ListFiles)]
		public void FileCommandObjectCreatedIsSameAsAConstructedObject(CommandType commandType)
		{
			ISearchQuery searchQuery = new SearchQuery("");
			IFileCommand commandFromFactory = (FileCommand)commandFactory.NewFileCommand(commandType, searchQuery);
			IFileCommand commandConstructedDirectly = new FileCommand(commandType, searchQuery);

			Assert.IsNotNull(commandFromFactory);
			Assert.AreEqual(typeof(FileCommand), commandFromFactory.GetType());
			Assert.AreEqual(commandConstructedDirectly.CommandType, commandFromFactory.CommandType);
			Assert.AreEqual(commandConstructedDirectly.Search, commandFromFactory.Search);
		}
		
		[Test]
		[Row(LoopType.Sequential)]
		[Row(LoopType.Random)]
		[Row(LoopType.None)]
		public void LoopCommandObjectCreatedIsSameAsAConstructedObject(LoopType loopType)
		{
			ILoopCommand commandFromFactory = (LoopCommand)commandFactory.NewLoopCommand(loopType);
			ILoopCommand commandConstructedDirectly = new LoopCommand(loopType);

			Assert.IsNotNull(commandFromFactory);
			Assert.AreEqual(typeof(LoopCommand), commandFromFactory.GetType());
			Assert.AreEqual(commandConstructedDirectly.CommandType, commandFromFactory.CommandType);
			Assert.AreEqual(commandConstructedDirectly.LoopType, commandFromFactory.LoopType);
		}
	}
}