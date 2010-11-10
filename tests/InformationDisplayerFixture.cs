using System;
using System.Collections.Generic;
using MbUnit.Framework;
using Rhino.Mocks;

namespace m.tests
{
	[TestFixture]
	public class InformationDisplayerFixture : MockingFixture
	{
		private IInformationDisplayer informationDisplayer;
		private IStateMachine player;
		private IFileFinder fileFinder;
		private IConsoleFacade console;

		public override void SetUp()
		{
			player = CreateMock<IStateMachine>();
			fileFinder = CreateMock<IFileFinder>();
			console = CreateMock<IConsoleFacade>();
			informationDisplayer = new InformationDisplayer(fileFinder, player, console);
		}

		[Test]
		public void IgnoreIsIgnored()
		{
			informationDisplayer.ProcessCommand(new Command(CommandType.Ignore));
		}

		[Test]
		[Row(3)]
		[Row(0)]
		public void ListFiles(int numOfTracks)
		{
			string[] possibleTracks = new string[] { "something/a track.mp3", "more things.blah", "meh/meh/meh/meh.meh" };
			IList<string> returnedFiles = new List<string>();
			for (int i = 0; i < numOfTracks; i++)
				returnedFiles.Add(possibleTracks[i]);

			IFileCommand listFilesCommand = CreateMock<IFileCommand>();
			
			SetupResult.For(listFilesCommand.CommandType).Return(CommandType.ListFiles);
			ISearchQuery searchQuery = CreateMock<ISearchQuery>();
			SetupResult.For(listFilesCommand.Search).Return(searchQuery);
			Expect.Call(
				fileFinder.FindFiles(searchQuery, new List<FileTypes>(InformationDisplayer.SEARCHFOR), FileListSort.None))
				.Return(returnedFiles);

			console.WriteLine(null);
			LastCall.On(console).IgnoreArguments().Repeat.Times(Math.Max(1, returnedFiles.Count));
			ReplayAll();

			informationDisplayer.ProcessCommand(listFilesCommand);
			VerifyAll();
		}

		[Test]
		public void DisplayHelp()
		{
			ICommand displayHelpCommand = CreateMock<ICommand>();
			SetupResult.For(displayHelpCommand.CommandType).Return(CommandType.Help);
			console.WriteLine(null);
			LastCall.IgnoreArguments().Repeat.AtLeastOnce();
			ReplayAll();

			informationDisplayer.ProcessCommand(displayHelpCommand);
			VerifyAll();
		}

		[Test]
		public void DisplayVersion()
		{
			ICommand displayVersionCommand = CreateMock<ICommand>();
			SetupResult.For(displayVersionCommand.CommandType).Return(CommandType.Version);
			console.WriteLine(null);
			LastCall.IgnoreArguments().Repeat.AtLeastOnce();
			ReplayAll();

			informationDisplayer.ProcessCommand(displayVersionCommand);
			VerifyAll();
		}
		
		[Test]
		public void ListHistory()
		{
			ICommand listHistoryCommand = CreateMock<ICommand>();
			SetupResult.For(listHistoryCommand.CommandType).Return(CommandType.ListHistory);
			
			SetupResult.For(player.History).Return(new List<string>(new string[] { "track 1.mp3", "some dir/track 2.mp3", "track 3.mp3", "another/dir/track 4.mp3" }));
			SetupResult.For(player.CurrentTrackOrdinal).Return(3);
			console.WriteLine("1. track 1.mp3");
			console.WriteLine("2. track 2.mp3");
			console.WriteLine(">>> 3. track 3.mp3 <<<");
			console.WriteLine("4. track 4.mp3");

			ReplayAll();

			informationDisplayer.ProcessCommand(listHistoryCommand);
			VerifyAll();
		}

		[Test]
		[Row("playing")]
		[Row("paused")]
		[Row("stopped")]
		[Row("n/a")]
		public void ListCurrentTrackDetails(string stateString)
		{
			ICommand currentTrackInfoCommand = CreateMock<ICommand>();
			SetupResult.For(currentTrackInfoCommand.CommandType).Return(CommandType.CurrentTrackDetails);

			SetupResult.For(player.History).Return(new List<string>(new string[] { "track 1.mp3", "some dir/track 2.mp3", "track 3.mp3", "another/dir/track 4.mp3" }));
			SetupResult.For(player.CurrentTrack).Return("some dir/track 2.mp3");
			SetupResult.For(player.CurrentTrackOrdinal).Return(2);
			bool statePlaying = false;
			bool statePaused = false;
			bool stateStopped = false;
			
			if (stateString == "playing")
				statePlaying = true;
			if (stateString == "paused")
				statePaused = true;
			if (stateString == "stopped")
				stateStopped = true;

			SetupResult.For(player.IsPlaying).Return(statePlaying);
			SetupResult.For(player.IsPaused).Return(statePaused);
			SetupResult.For(player.IsStopped).Return(stateStopped);
			
			console.WriteLine("File: some dir/track 2.mp3");
			console.Write("State: ");

			if (stateString == "playing")
				console.WriteLine("Playing");
			if (stateString == "paused")
				console.WriteLine("Paused");
			if (stateString == "stopped")
				console.WriteLine("Stopped");
			if (stateString == "n/a")
				console.WriteLine("Unknown");

			console.WriteLine("Index: 2/4");
			ReplayAll();

			informationDisplayer.ProcessCommand(currentTrackInfoCommand);
			VerifyAll();
		}

		[Test]
		public void ListCurrentTrackDetailsWithNoHistory()
		{
			ICommand currentTrackInfoCommand = CreateMock<ICommand>();
			SetupResult.For(currentTrackInfoCommand.CommandType).Return(CommandType.CurrentTrackDetails);
			SetupResult.For(player.History).Return(new List<string>(new string[] { }));
			console.WriteLine(null);
			LastCall.On(console).IgnoreArguments();
			ReplayAll();

			informationDisplayer.ProcessCommand(currentTrackInfoCommand);
			VerifyAll();
		}


	}
}
