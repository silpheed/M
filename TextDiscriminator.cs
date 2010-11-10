using System;
using System.Collections.Generic;


namespace m
{
	public class TextDiscriminator : ITextDiscriminator
	{
		public const int DEFAULTTIMESKIP = 5;
		public const int DEFAULTTRACKSKIP = 1;

		private readonly ISearchQueryFactory searchQueryFactory;
		private readonly ICommandFactory commandFactory;

		public TextDiscriminator(ICommandFactory commandFactory, ISearchQueryFactory searchQueryFactory)
		{
			this.commandFactory = commandFactory;
			this.searchQueryFactory = searchQueryFactory;
		}

		public IList<ICommand> Interpret(string input)
		{
			IList<ICommand> payload = new List<ICommand>();

			if ((String.IsNullOrEmpty(input)) || (input.Trim() == String.Empty)) {
				payload.Add(commandFactory.NewCommand(CommandType.Ignore));
				return payload;
			}
			string[] args = input.Split(' ');
			int temp;

			//play track with shortest file name from the search
			if (args[0] == "m")
				payload.Add(CreateFileCommand(CommandType.PlayShortestTrack, args));
			//play a random track from the search
			else if (args[0] == "r")
				payload.Add(CreateFileCommand(CommandType.PlayRandomTrack, args));
			//play a playlist sequentially, with whatever loop is already chosen
			else if (args[0] == "pl")
				payload.Add(CreateFileCommand(CommandType.PlayPlaylist, args));
			//play a playlist sequentially with no loop
			else if (args[0] == "pln") {
				payload.Add(commandFactory.NewLoopCommand(LoopType.None));
				payload.Add(CreateFileCommand(CommandType.PlayPlaylist, args));
			}
			//play a playlist sequentially and continuously
			else if (args[0] == "plc") {
				payload.Add(commandFactory.NewLoopCommand(LoopType.Sequential));
				payload.Add(CreateFileCommand(CommandType.PlayPlaylist, args));
			}
			//play a playlist randomly and continuously
			else if (args[0] == "plr") {
				payload.Add(commandFactory.NewLoopCommand(LoopType.Random));
				payload.Add(CreateFileCommand(CommandType.PlayPlaylist, args));
			}
			//play the results of a search query sequentially with no loop
			else if (args[0] == "cn") {
				payload.Add(commandFactory.NewLoopCommand(LoopType.None));
				payload.Add(CreateFileCommand(CommandType.PlayQueryAsList, args));
			}
			//play the results of a search query sequentially and continuously
			else if (args[0] == "cc") {
				payload.Add(commandFactory.NewLoopCommand(LoopType.Sequential));
				payload.Add(CreateFileCommand(CommandType.PlayQueryAsList, args));
			}
			//play the results of a search query randomly and continuously
			else if ((args[0] == "cr") || (args[0] == "c")) {
				payload.Add(commandFactory.NewLoopCommand(LoopType.Random));
				payload.Add(CreateFileCommand(CommandType.PlayQueryAsList, args));
			}
			//find files but don't play them
			else if (args[0] == "f")
				payload.Add(CreateFileCommand(CommandType.ListFiles, args));
			//pause, or continue playing if paused
			else if (args[0] == "pp")
				payload.Add(commandFactory.NewCommand(CommandType.PlayPause));
			//repeat the current track or last track
			else if (args[0] == "re")
				payload.Add(commandFactory.NewCommand(CommandType.Repeat));
			//stop the track
			else if (args[0] == "x")
				payload.Add(commandFactory.NewCommand(CommandType.Stop));
			//list history
			else if (args[0] == "h")
				payload.Add(commandFactory.NewCommand(CommandType.ListHistory));
			//show track info
			else if (args[0] == "i")
				payload.Add(commandFactory.NewCommand(CommandType.CurrentTrackDetails));
			//show help
			else if ((args[0] == "--help") || (args[0] == "/?") || (args[0] == "?"))
				payload.Add(commandFactory.NewCommand(CommandType.Help));
			//show version
			else if (args[0] == "v")
				payload.Add(commandFactory.NewCommand(CommandType.Version));
			//remove current loop
			else if (args[0] == "ln")
				payload.Add(commandFactory.NewLoopCommand(LoopType.None));
			//change to sequential loop (lc is loop continuous, different meaning for the same result)
			else if ((args[0] == "ls") || (args[0] == "lc") || (args[0] == "l"))
				payload.Add(commandFactory.NewLoopCommand(LoopType.Sequential));
			//change loop to random
			else if (args[0] == "lr")
				payload.Add(commandFactory.NewLoopCommand(LoopType.Random));
			//move forward or back through the playlist or played tracks
			else if ((args[0] == "n") || (args[0] == "p"))
			{
				int skip = 0;
				if (args.Length > 1)
					Int32.TryParse(args[1], out skip);
				if (skip == 0)
					skip = DEFAULTTRACKSKIP;
				if (args[0] == "p")
					skip *= -1;
				payload.Add(commandFactory.NewChangeStateCommand(CommandType.SkipTrack, skip));
			}
			//go to a particular track in history
			else if ((args[0] == "g") && (args.Length == 2) && (Int32.TryParse(args[1], out temp)))
				payload.Add(commandFactory.NewChangeStateCommand(CommandType.SkipToTrack, temp));
			//increase or decrease volume
			else if ((args[0].StartsWith("+")) || (args[0].StartsWith("-")))
			{
				int magnitude = 0;
				foreach (char c in args[0])
				{
					if (c == '+')
						magnitude++;
					else if (c == '-')
						magnitude--;
					else
						break;
				}
				payload.Add(commandFactory.NewChangeStateCommand(CommandType.Volume, magnitude));
			}
			//skip seconds forward or back through the current track
			else if (args[0] == "s")
			{
				int skip = 0;
				if (args.Length > 1)
					Int32.TryParse(args[1], out skip);
				if (skip == 0)
					skip = DEFAULTTIMESKIP;
				payload.Add(commandFactory.NewChangeStateCommand(CommandType.SkipTime, skip));
			}
			//exit the program
			else if ((args[0] == "xx") || (args[0] == "q"))
				payload.Add(commandFactory.NewCommand(CommandType.Exit));
			//default - play track with shortest file name from the search
			else
				payload.Add(commandFactory.NewFileCommand(CommandType.PlayShortestTrack, searchQueryFactory.NewSearchQuery(input)));

			return payload;
		}

		private ICommand CreateFileCommand(CommandType commandType, string[] args)
		{
			string search = String.Empty;
			for (int i = 1; i < args.Length; i++)
				search += args[i] + " ";
			search = search.Trim();
			return commandFactory.NewFileCommand(commandType, searchQueryFactory.NewSearchQuery(search));
		}

	}
}