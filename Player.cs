using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.IO;

namespace m
{
	public class Player : IStateMachine
	{
		private IAudioStream audio;
		private IAudioStream badSoundAudio;

		private readonly IAudioStreamFactory audioStreamFactory;
		private readonly IFileFinder fileFinder;
		private readonly IPlaylistReader playlistReader;
		private readonly IBackgroundWorkerFactory backgroundWorkerFactory;
		private readonly IFileSystemFacade fileSystem;
				
		private bool isStopped;
		private bool isPlaying;
		private bool isPaused;

		private IBackgroundWorkerWrapper audioStreamThread;

		private bool wasPausedBeforeBadSound;
		
		private LoopType loopType;
		private List<string> history;
		
		private int upto;
		
		public Player(IFileFinder fileFinder, IPlaylistReader playlistReader, IAudioStreamFactory audioStreamFactory, 
			IBackgroundWorkerFactory backgroundWorkerFactory, IFileSystemFacade fileSystem)
		{
			this.fileFinder = fileFinder;
			this.playlistReader = playlistReader;
			this.backgroundWorkerFactory = backgroundWorkerFactory;
			this.audioStreamFactory = audioStreamFactory;
			this.fileSystem = fileSystem;
			
			history = new List<string>();
		
			isStopped = true;
			isPlaying = false;
			isPaused = false;

			wasPausedBeforeBadSound = isPaused;
			loopType = LoopType.None;
			upto = 0;
		}

		private void audio_ConstantUpdateEvent(object update)
		{
		}
		
		public void ProcessCommand(ICommand command)
		{
			//can't switch on type, so we use a series of ifs to get to the commands that we can switch on
			if (command is IFileCommand) {
				IFileCommand pfCommand = command as IFileCommand;
				switch (pfCommand.CommandType) {
					case CommandType.PlayShortestTrack:
						PlayNewTrack(fileFinder.FindFiles(pfCommand.Search, FileTypes.MP3, FileListSort.SmallestFirst));
						break;
					case CommandType.PlayRandomTrack:
						PlayNewTrack(fileFinder.FindFiles(pfCommand.Search, FileTypes.MP3, FileListSort.Random));
						break;
					case CommandType.PlayPlaylist:
						PlayPlaylist(
							fileFinder.FindFiles(pfCommand.Search, new FileTypes[] {FileTypes.M3U, FileTypes.PLS}, FileListSort.SmallestFirst));
						break;
					case CommandType.PlayQueryAsList:
						PlayGenericList(fileFinder.FindFiles(pfCommand.Search, FileTypes.MP3, FileListSort.SmallestFirst));
						break;
				}
			}
			else if (command is IChangeStateCommand) {
				IChangeStateCommand csCommand = command as IChangeStateCommand;
				switch (csCommand.CommandType)
				{
					case CommandType.SkipTime:
						throw new NotImplementedException();
						//break;
					case CommandType.SkipTrack:
						SkipTracks(csCommand.Magnitude);
						break;
					case CommandType.SkipToTrack:
						SkipToTrack(csCommand.Magnitude - 1);
						break;
					case CommandType.Volume:
						throw new NotImplementedException();
						//break;
				}
			}
			else if (command is ILoopCommand) {
				ILoopCommand lCommand = command as ILoopCommand;
				loopType = lCommand.LoopType;
			}
			else
				switch (command.CommandType) {
					case CommandType.Exit:
					case CommandType.Stop:
						Stop();
						break;

					case CommandType.Repeat:
						Repeat();
						break;

					case CommandType.PlayPause:
						PlayPause();
						break;

					case CommandType.Ignore:
						break;
				}
			//ignore any other commands
		}

		private void PlayNewTrack(IList<string> list)
		{
			if ((list == null) || (list.Count == 0)) {
				PlayUnsuccessfulSound();
				return;
			}
			
			//if the current track playing is also in the list of tracks returned by this new request,
			//we will not play it and go to the next one in the list. this is the only reason for this 
			//method to take a list.
			int i = 0;
			while ((history.Count > 0) && (history[upto] == list[i]) && (i < (list.Count - 1)))
				i++;

			upto++;

			if (!AddToHistory(list[i]))
				upto--;

			if (upto >= history.Count)
				upto = history.Count - 1;

			Play(history[upto]);
		}

		private void PlayPlaylist(IList<string> listOfLists)
		{
			if ((listOfLists == null) || (listOfLists.Count == 0)) {
				PlayUnsuccessfulSound();
				return;
			}
			IList<string> tracks = playlistReader.GetTracklist(listOfLists[0]);
			PlayGenericList(tracks);
		}
		
		private void PlayGenericList(IList<string> tracks)
		{
			if ((tracks == null) || (tracks.Count == 0)) {
				PlayUnsuccessfulSound();
				return;
			}
			ClearHistory();

			upto = 0;
			AddToHistory(tracks);
			//ADD SOMETHING FOR RANDOM HERE - or maybe not
			
			Play(history[upto]);
		}

		private void PlayUnsuccessfulSound()
		{
			string badSoundFileName = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "badsound.mp3");
			if (!fileSystem.FileExists(badSoundFileName))
				return;
			
			if (isPaused)
				wasPausedBeforeBadSound = true;
			if (isPlaying)
				PlayPause();
			
			badSoundAudio = audioStreamFactory.NewAudioStream();
			badSoundAudio.Open(badSoundFileName);
			IBackgroundWorkerWrapper audioStreamThread = backgroundWorkerFactory.NewBackgroundWorker();
			audioStreamThread.DoWork += PlayUnsuccessfulSound;
			audioStreamThread.RunWorkerCompleted += UnsuccessfulSoundHasStopped;
			audioStreamThread.RunWorkerAsync();
		}

		private void PlayUnsuccessfulSound(object sender, DoWorkEventArgs e)
		{
			if (null != badSoundAudio)
				badSoundAudio.Play();
		}

		private void UnsuccessfulSoundHasStopped(object sender, RunWorkerCompletedEventArgs e)
		{
			if (null != badSoundAudio)
				badSoundAudio.Stop();
			if ((isPaused) && (!wasPausedBeforeBadSound))
				PlayPause();
			wasPausedBeforeBadSound = false;
			//badSoundAudio = null;
		}

		private void AddToHistory(IList<string> filenames)
		{
			//need to add files in reverse order, as each insertion will push back the previous.
			for (int i = filenames.Count - 1; i >= 0; i--)
				AddToHistory(filenames[i]);
		}

		private bool AddToHistory(string filename)
		{
			//upto is usually incremented before this method is called
			//don't add a track to history if it's already the last track we just heard
			if ((history.Count == 0) || ((upto == history.Count) && (history[upto - 1] != filename))) {
				history.Add(filename);
				return true;
			}
			//except when we're adding a playlist, then upto is zeroed
			if (upto == 0) {
				history.Insert(upto, filename);
				return true;
			}
			if (history[upto - 1] != filename) {
				history.Insert(upto, filename);
				return true;
			}
			return false;
		}

		private void ClearHistory()
		{
			history.Clear();
		}

		private void Play(string path)
		{
			if (!isStopped)
				Stop();
			audio = audioStreamFactory.NewAudioStream();
			audio.ConstantUpdateEvent += audio_ConstantUpdateEvent;
			audio.Open(path);
			audioStreamThread = backgroundWorkerFactory.NewBackgroundWorker();
			audioStreamThread.DoWork += PlayInNewThread;
			audioStreamThread.RunWorkerCompleted += AudioStreamHasStopped;
			audioStreamThread.RunWorkerAsync();
			isPlaying = true;
			isPaused = false;
			isStopped = false;
		}

		private void AudioStreamHasStopped(object sender, RunWorkerCompletedEventArgs e)
		{
			EndOfTrack();
		}
		
		private void PlayInNewThread(object sender, DoWorkEventArgs e)
		{
			if (null != audio)
				audio.Play();
		}

		private void Stop()
		{
			isStopped = true;
			isPlaying = false;
			isPaused = false;
			if (null == audio)
				return;

			audio.Stop();
			audio = null;

			int i = 0;
			while ((null != audioStreamThread) && (audioStreamThread.IsBusy)) {
				Thread.Sleep(10);
				i++;
				if (i > 100)
					throw new Exception("Track did not stop after 1 second.");
			}
		}

		private void EndOfTrack()
		{
			if (isStopped)
				return;
			Stop();
			
			//sort this out, would like this on SkipTrack too
			int next = upto;
			if ((loopType == LoopType.Random) && (history.Count > 1)) {
				Random rnd = new Random();
				while (next == upto)
					next = rnd.Next(history.Count);
				SkipToTrack(next);
			}
			else if ((loopType != LoopType.None) && (history.Count == 1))
				SkipToTrack(next);
			else
				SkipTracks(1);
		}

		private void PlayPause()
		{
			if (isStopped)
				return;
			audio.PlayPause();
			isStopped = false;
			isPlaying = isPlaying == false;
			isPaused = isPaused == false;
		}

		private void Repeat()
		{
			if (history.Count > 0)
				Play(history[history.Count - 1]);
		}

		private void SkipToTrack(int trackNum)
		{
			if ((trackNum >= history.Count) || (trackNum < 0))
				return;
			Play(history[trackNum]);
			upto = trackNum;
		}

		private void SkipTracks(int magnitude)
		{
			if ((magnitude == 0) || (history.Count == 0))
				return;
			int goTo = upto + magnitude;

			if (loopType != LoopType.None) {
				while (goTo > (history.Count - 1))
					goTo -= history.Count;
				while (goTo < 0)
					goTo += history.Count;
			
			}
			goTo = Math.Min(history.Count - 1, goTo);
			goTo = Math.Max(0, goTo);
			if (upto == goTo)
				return;
			SkipToTrack(goTo);
		}

		/*
		private void SkipTime(int secs)
		{
			throw new NotImplementedException();
		}
		*/

		public bool IsStopped
		{
			get { return isStopped; }
		}

		public bool IsPlaying
		{
			get { return isPlaying; }
		}

		public bool IsPaused
		{
			get { return isPaused; }
		}

		public int CurrentTrackOrdinal
		{
			get {
				if (history.Count > upto)
					return upto + 1;
				return 0;
			}
		}

		public string CurrentTrack
		{
			get {
				if (history.Count > upto)
					return history[upto];
				return null;
			}
		}

		public LoopType CurrentLoopType
		{
			get { return loopType; }
		}

		public IList<string> History
		{
			get { return history; }
		}

	}
}