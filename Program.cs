//m
//
//m - play a song with this in the filename. if multiple files, play the shortest
//r - play a song with this in the filename. if multiple files, play random from that list
//x - stop. twice, exit
//xx or q - exit
//re - repeat
//pp - pause, or play after a pause
//pl - play a playlist with this in the filename. loop stays as it was previously
//pln - same as pl, but turn off looping
//plc - same as pl, but continuous loop
//plr - same as pll, but tracks are random
//n # - next song, random if no further songs. # is how forward to go
//p # - previous song, playlist or otherwise. # is how backward to go
//g # - go to track number #
//f - find all tracks matching this search
//ln - turn off looping
//lc or ls or l - turn on sequential looping
//lr - turn on random looping
//h - list history

//+ - volume up, ++ more, +++ even more etc.
//- - volume down
//s # - skip # seconds. can be negative
//i - info, status
//? - help

//
//anything else - m or r (config setting)

using System;
using System.Collections.Generic;
using System.Configuration;

namespace m
{
	class Program
	{
		//testable main
		public static void Run(IEnumerable<string> args, ITextDiscriminator textDiscriminator, IConsoleFacade console, IStateMachine player, IInformationDisplayer informationDisplayer)
		{
			//once off construction of input from execution args
			string input = String.Empty;
			if (args != null)
				foreach (String s in args)
					input += s + " ";

			//main loop
			bool exit = false;
			while (!exit) {
				foreach (ICommand command in textDiscriminator.Interpret(input.Trim()))
				{
					//ignore
					if (command.CommandType == CommandType.Ignore)
						continue;
					//quit
					if ((command.CommandType == CommandType.Exit) || ((command.CommandType == CommandType.Stop) && (player.IsStopped)))
						exit = true;
					//everything else is fair game
					informationDisplayer.ProcessCommand(command);
					player.ProcessCommand(command);
				}
				if (!exit)
					input = console.ReadLine();
			}
		}
		
		//run main
		[CoverageExclude("Program entry point is private, therefore not easily testable.")]
		static void Main(string[] args)
		{
			/*
			Console.WriteLine(System.AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
			Console.WriteLine(System.Security.Principal.WindowsIdentity.GetCurrent().Name);
			Console.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.Personal));
			Console.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
			Console.WriteLine(System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.PerUserRoaming).FilePath);
			Console.WriteLine(System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath);
			
						
			ExeConfigurationFileMap map = new ExeConfigurationFileMap();
			map.ExeConfigFilename = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			map.RoamingUserConfigFilename = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			
			Console.WriteLine(System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.PerUserRoaming).FilePath);
			*/
			
			
			
			//Console.WriteLine(System.Configuration.ConfigurationManager.OpenExeConfiguration(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FilePath);
			//System.Configuration.ConfigurationManager.
			
			IPlatform platform = new Platform();
			IConsoleFacade console = new ConsoleFacade();
			IAudioStreamFactory audioStreamFactory = new AudioStreamFactory(platform);
			IBackgroundWorkerFactory backgroundWorkerFactory = new BackgroundWorkerFactory();
			IConfigSettingsFacade configSettings = new ConfigSettingsFacade();
			IFileSystemFacade fileSystem = new FileSystemFacade();
			IPlaylistReader playlistReader = new PlaylistReader(fileSystem);
			IFileFinder fileFinder = new FileFinder(fileSystem, configSettings);
			IStateMachine player = new Player(fileFinder, playlistReader, audioStreamFactory, backgroundWorkerFactory, fileSystem);
			IInformationDisplayer informationDisplayer = new InformationDisplayer(fileFinder, player, console);
			ICommandFactory commandFactory = new CommandFactory();
			ISearchQueryFactory searchQueryFactory = new SearchQueryFactory();
			ITextDiscriminator textDiscriminator = new TextDiscriminator(commandFactory, searchQueryFactory);

			try {
				Run(args, textDiscriminator, console, player, informationDisplayer);
			}
			catch (Exception e) {
				console.WriteLine("A fatal error has occured: " + e);
			}
			//Free up unmanaged memory reserved by FFmpeg. This must be done only once, and no music can be played after this has happened.
			//This call should be further into the program, possibly in Player.cs, but that would require a full refactoring of Decoder.cs 
			//and all the static FFmpeg calls. It's really not worth it.
			if (platform.IsWindows)
				Tao.FFmpeg.FFmpeg.av_free_static();
		}
	}
}