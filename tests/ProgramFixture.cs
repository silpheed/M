using System.Collections.Generic;
using MbUnit.Framework;
using Rhino.Mocks;

namespace m.tests
{
	[TestFixture]
	public class ProgramFixture : MockingFixture
	{
		private ITextDiscriminator textDiscriminator;
		private IConsoleFacade console;
		private IStateMachine player;
		private IInformationDisplayer informationDisplayer;

		public override void SetUp()
		{
			textDiscriminator = CreateMock<ITextDiscriminator>();
			console = CreateMock<IConsoleFacade>();
			player = CreateMock<IStateMachine>();
			informationDisplayer = CreateMock<IInformationDisplayer>();
		}

		private void QuickExit()
		{
			IList<ICommand> commands = new List<ICommand>();
			commands.Add(new Command(CommandType.Exit));
			Expect.Call(textDiscriminator.Interpret(null)).IgnoreArguments().Return(commands);
		}

		[Test]
		public void ExitOnExitCommand()
		{
			QuickExit();
			ReplayAll();

			Program.Run(@"anything".Split(' '), textDiscriminator, console, player, informationDisplayer);
			VerifyAll();
		}

		[Test]
		public void ExitOnStopCommandWhilePlayerStopped()
		{
			IList<ICommand> commands = new List<ICommand>();
			commands.Add(new Command(CommandType.Stop));
			Expect.Call(textDiscriminator.Interpret(null)).IgnoreArguments().Return(commands);
			Expect.Call(player.IsStopped).Return(true);
			ReplayAll();

			Program.Run(@"anything".Split(' '), textDiscriminator, console, player, informationDisplayer);
			VerifyAll();
		}

		[Test]
		public void WillLoopForeverWhileWhenNotExiting()
		{
			IList<ICommand> playCommand = new List<ICommand>();
			playCommand.Add(new Command(CommandType.PlayShortestTrack));
			
			SetupResult.For(player.IsStopped).Return(false);
			SetupResult.For(console.ReadLine()).Return("anything");
			Expect.Call(textDiscriminator.Interpret(null)).IgnoreArguments().Return(playCommand);
			player.ProcessCommand(playCommand[0]);
			informationDisplayer.ProcessCommand(playCommand[0]);
			Expect.Call(textDiscriminator.Interpret(null)).IgnoreArguments().Return(playCommand);
			player.ProcessCommand(playCommand[0]);
			informationDisplayer.ProcessCommand(playCommand[0]);
			Expect.Call(textDiscriminator.Interpret(null)).IgnoreArguments().Return(playCommand);
			player.ProcessCommand(playCommand[0]);
			informationDisplayer.ProcessCommand(playCommand[0]);
			QuickExit();
			ReplayAll();

			Program.Run(@"anything".Split(' '), textDiscriminator, console, player, informationDisplayer);
			VerifyAll();
		}
		
		[Test]
		public void IgnoreIsIgnored()
		{
			IList<ICommand> commands = new List<ICommand>();
			commands.Add(new Command(CommandType.Ignore));
			commands.Add(new Command(CommandType.Ignore));
			commands.Add(new Command(CommandType.Ignore));
			Expect.Call(textDiscriminator.Interpret(null)).IgnoreArguments().Return(commands);
			SetupResult.For(console.ReadLine()).Return("anything");
			QuickExit();
			ReplayAll();

			Program.Run(@"anything".Split(' '), textDiscriminator, console, player, informationDisplayer);
			VerifyAll();
		}
		

		[Test]
		public void OtherCommandsHitTheStateMachineAndInformationDisplayer()
		{
			ICommand command = new Command(CommandType.Repeat);
			IList<ICommand> commands = new List<ICommand>();
			commands.Add(command);
			Expect.Call(textDiscriminator.Interpret(null)).IgnoreArguments().Return(commands);
			SetupResult.For(player.IsStopped).Return(false);
			SetupResult.For(console.ReadLine()).Return("anything");
			player.ProcessCommand(command);
			informationDisplayer.ProcessCommand(command);
			QuickExit();
			ReplayAll();

			Program.Run(@"anything".Split(' '), textDiscriminator, console, player, informationDisplayer);
			VerifyAll();
		}
	}
}