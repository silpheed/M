using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace m
{
	public class InformationDisplayer : IInformationDisplayer
	{
		public static readonly FileTypes[] SEARCHFOR = new FileTypes[] { FileTypes.MP3, FileTypes.M3U, FileTypes.PLS };
		private readonly IFileFinder fileFinder;
		private readonly IStateMachine player;
		private readonly IConsoleFacade console;

		public InformationDisplayer(IFileFinder fileFinder, IStateMachine player, IConsoleFacade console)
		{
			this.fileFinder = fileFinder;
			this.player = player;
			this.console = console;
		}

		public void ProcessCommand(ICommand command)
		{
			if (command.CommandType == CommandType.Ignore)
				return;
			if (command.CommandType == CommandType.ListHistory)
			{
				DisplayHistory();
				return;
			}
			if (command.CommandType == CommandType.Help)
			{
				DisplayHelp();
				return;
			}
			if (command.CommandType == CommandType.Version)
			{
				DisplayVersion();
				return;
			}
			if (command.CommandType == CommandType.CurrentTrackDetails)
			{
				DisplayCurrentTrackDetails();
				return;
			}
			if ((command is IFileCommand) && (command.CommandType == CommandType.ListFiles))
			{
				DisplayTracks((command as IFileCommand).Search);
				return;
			}
			//ignore any other commands
		}

		//File:  Full path/and/filename.mp3
		//MAYBE - Time:  0:34/3:51
		//State: Playing/Paused/Stopped
		//Index: 12/52
		private void DisplayCurrentTrackDetails()
		{
			if (player.History.Count == 0) {
				console.WriteLine("No tracks loaded.");
				return;
			}
			console.WriteLine("File: " + player.CurrentTrack);
			console.Write("State: ");
			if (player.IsPlaying)
				console.WriteLine("Playing");
			else if (player.IsPaused)
				console.WriteLine("Paused");
			else if (player.IsStopped)
				console.WriteLine("Stopped");
			else
				console.WriteLine("Unknown");
			console.WriteLine("Index: " + player.CurrentTrackOrdinal + "/" + player.History.Count);
		}

		private void DisplayTracks(ISearchQuery sq)
		{
			IList<string> result = fileFinder.FindFiles(sq, new List<FileTypes>(SEARCHFOR), FileListSort.None);
			if ((null == result) || (result.Count == 0))
				console.WriteLine("No files found");
			else
				foreach (string file in result)
					console.WriteLine(file);
		}

		private void DisplayHistory()
		{
			for (int i = 0; i < player.History.Count; i++)
				if (i + 1 == player.CurrentTrackOrdinal)
					console.WriteLine(">>> " + (i + 1) + ". " + Path.GetFileName(player.History[i]) + " <<<");
				else
					console.WriteLine((i + 1) + ". " + Path.GetFileName(player.History[i]));
		}

		private void DisplayHelp()
		{
			console.WriteLine(GetFromEmbeddedFile(@"m.texts.help"));
		}
		
		private void DisplayVersion()
		{
			console.WriteLine(String.Empty);
			console.WriteLine("v0.1");
			console.WriteLine("With diesel power.");
			console.WriteLine(String.Empty);
		}
		
		private static string GetFromEmbeddedFile(string filename)
		{
			Assembly asm = Assembly.GetExecutingAssembly();
			StreamReader reader = new StreamReader(asm.GetManifestResourceStream(filename));
			return reader.ReadToEnd();
		}
        
	}
}