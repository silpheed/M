using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Gallio.Framework;
using MbUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace m.tests
{
	[TestFixture]
	public class PlayerFixture : MockingFixture
	{
		private IStateMachine player;
		private IFileFinder fileFinder;
		private IPlaylistReader playlistReader;
		private IAudioStreamFactory audioStreamFactory;
		private IBackgroundWorkerFactory backgroundWorkerFactory;
		private IFileSystemFacade fileSystem;
		
		public override void SetUp()
		{
			fileFinder = CreateMock<IFileFinder>();
			playlistReader = CreateMock<IPlaylistReader>();
			audioStreamFactory = CreateMock<IAudioStreamFactory>();
			backgroundWorkerFactory = CreateMock<IBackgroundWorkerFactory>();
			fileSystem = DynamicMock<IFileSystemFacade>();
			player = new Player(fileFinder, playlistReader, audioStreamFactory, backgroundWorkerFactory, fileSystem);
		}

		private void CheckPlayerState(bool playing, bool paused, bool stopped)
		{
			Assert.AreEqual(playing, player.IsPlaying);
			Assert.AreEqual(paused, player.IsPaused);
			Assert.AreEqual(stopped, player.IsStopped);
		}

		private void CheckPlayerState(bool playing, bool paused, bool stopped, string currentTrack)
		{
			CheckPlayerState(playing, paused, stopped);
			Assert.AreEqual(currentTrack, player.CurrentTrack);
		}

		private IFileCommand SetupSinglePlay()
		{
			return SetupGenericPlay(CommandType.PlayShortestTrack, true);
		}

		private IFileCommand SetupSingleRandomPlay()
		{
			return SetupGenericPlay(CommandType.PlayRandomTrack, true);
		}

		private IFileCommand SetupPlaylistPlay()
		{
			return SetupGenericPlay(CommandType.PlayPlaylist, true);
		}

		private IFileCommand SetupSingleUnsucessfulPlay()
		{
			return SetupGenericPlay(CommandType.PlayShortestTrack, false);
		}

		private IFileCommand SetupWholeQueryPlay()
		{
			return SetupGenericPlay(CommandType.PlayQueryAsList, true);
		}

		private IFileCommand SetupGenericPlay(CommandType commandtype, bool withResults)
		{
			IFileCommand fileCommand = DynamicMock<IFileCommand>();
			ISearchQuery searchQuery = DynamicMock<ISearchQuery>();
			SetupResult.For(fileCommand.CommandType).Return(commandtype);
			SetupResult.For(fileCommand.Search).Return(searchQuery);
			if (withResults) {
				SetupResult.For(searchQuery.WantedAtoms).Return(new List<string>(new string[] {"good"}));
				SetupResult.For(searchQuery.UnwantedAtoms).Return(new List<string>(new string[] {"bad"}));
			}
			else {
				SetupResult.For(searchQuery.WantedAtoms).Return(new List<string>());
				SetupResult.For(searchQuery.UnwantedAtoms).Return(new List<string>());
			}

			ReplaySome(fileCommand, searchQuery);
			return fileCommand;
		}

		private void FindTracks(params string[] tracks)
		{
			Expect.Call(fileFinder.FindFiles(null, FileTypes.MP3, FileListSort.Random)).IgnoreArguments().Return(
				new List<string>( tracks ));
		}

		private IAudioStream SetupAudioStream()
		{
			IAudioStream audioStream = CreateMock<IAudioStream>();
			Expect.Call(audioStreamFactory.NewAudioStream()).Return(audioStream);
			Expect.Call(audioStream.Open(null)).IgnoreArguments().Return(true);
			audioStream.ConstantUpdateEvent += null;
			LastCall.On(audioStream).IgnoreArguments();
			return audioStream;
		}

		private IBackgroundWorkerWrapper SetupBackgroundWorker()
		{
			IBackgroundWorkerWrapper backgroundWorker = DynamicMock<IBackgroundWorkerWrapper>();
			Expect.Call(backgroundWorkerFactory.NewBackgroundWorker()).Return(backgroundWorker);
			return backgroundWorker;
		}

		private void SetupUnsuccessfulEvent()
		{
			IEventRaiser badDoWorkEvent;
			IEventRaiser badWorkerCompletedEvent;
			SetupUnsuccessfulEvent(out badDoWorkEvent, out badWorkerCompletedEvent);
		}

		//private IBackgroundWorkerWrapper SetupUnsuccessfulEvent(out IEventRaiser badDoWorkEvent, out IEventRaiser badWorkerCompletedEvent)
		private void SetupUnsuccessfulEvent(out IEventRaiser badDoWorkEvent, out IEventRaiser badWorkerCompletedEvent)
		{
			IAudioStream badEventAudio = CreateMock<IAudioStream>();
			Expect.Call(audioStreamFactory.NewAudioStream()).Return(badEventAudio);
			Expect.Call(badEventAudio.Open(null)).IgnoreArguments().Return(true);
			IBackgroundWorkerWrapper badBackgroundWorker = CreateMock<IBackgroundWorkerWrapper>();
			Expect.Call(backgroundWorkerFactory.NewBackgroundWorker()).Return(badBackgroundWorker);
			badBackgroundWorker.DoWork += null;
			badDoWorkEvent = LastCall.On(badBackgroundWorker).IgnoreArguments().GetEventRaiser();
			badBackgroundWorker.RunWorkerCompleted += null;
			badWorkerCompletedEvent = LastCall.On(badBackgroundWorker).IgnoreArguments().GetEventRaiser();
			badBackgroundWorker.RunWorkerAsync();
			ReplaySome(badEventAudio, badBackgroundWorker);
		}

		[Test]
		public void PlaysSingleTrack()
		{
			IFileCommand singlePlayCommand = SetupSinglePlay();
			FindTracks("a track.mp3");
			SetupAudioStream();
			SetupBackgroundWorker();

			ReplayAll();

			player.ProcessCommand(singlePlayCommand);
			VerifyAll();
			CheckPlayerState(true, false, false, "a track.mp3");
		}

		[Test]
		public void PlaysRandomTrack()
		{
			IFileCommand singlePlayCommand = SetupSingleRandomPlay();
			FindTracks("a track.mp3", "another track.mp3", "more tracks.mp3");
			SetupAudioStream();
			SetupBackgroundWorker();

			ReplayAll();

			player.ProcessCommand(singlePlayCommand);
			VerifyAll();
			CheckPlayerState(true, false, false, "a track.mp3");
		}

		[Test]
		public void PauseAfterPlay()
		{
			IFileCommand singlePlayCommand = SetupSinglePlay();
			FindTracks("a track.mp3");
			IAudioStream audioStream = SetupAudioStream();
			SetupBackgroundWorker();
			
			audioStream.PlayPause();
			ReplayAll();

			player.ProcessCommand(singlePlayCommand);
			CheckPlayerState(true, false, false, "a track.mp3");
			player.ProcessCommand(new Command(CommandType.PlayPause));
			
			VerifyAll();
			
			CheckPlayerState(false, true, false, "a track.mp3");
		}

		[Test]
		public void PlayAfterPause()
		{
			IFileCommand singlePlayCommand = SetupSinglePlay();
			FindTracks("a track.mp3");
			IAudioStream audioStream = SetupAudioStream();
			SetupBackgroundWorker();

			audioStream.PlayPause();
			LastCall.On(audioStream).Repeat.Twice();
			ReplayAll();

			player.ProcessCommand(singlePlayCommand);
			CheckPlayerState(true, false, false, "a track.mp3");
			player.ProcessCommand(new Command(CommandType.PlayPause));
			CheckPlayerState(false, true, false, "a track.mp3");
			player.ProcessCommand(new Command(CommandType.PlayPause));
			
			VerifyAll();

			CheckPlayerState(true, false, false, "a track.mp3");
		}

		[Test]
		public void CurrentTrackWhenNothingPlayedIsNull()
		{
			Assert.IsNull(player.CurrentTrack);
		}
/*
		[Test]
		[ExpectedException(typeof(NotImplementedException))]
		public void SkipTimeNotImplemented()
		{
			IChangeStateCommand skipTimeCommand = CreateMock<IChangeStateCommand>();
			SetupResult.For(skipTimeCommand.CommandType).Return(CommandType.SkipTime);
			SetupResult.For(skipTimeCommand.Magnitude).Return(0);
			ReplayAll();

			player.ProcessCommand(skipTimeCommand);
			VerifyAll();
		}

		[Test]
		[ExpectedException(typeof(NotImplementedException))]
		public void VolumeNotImplemented()
		{
			IChangeStateCommand volumeCommand = CreateMock<IChangeStateCommand>();
			SetupResult.For(volumeCommand.CommandType).Return(CommandType.Volume);
			SetupResult.For(volumeCommand.Magnitude).Return(0);
			ReplayAll();

			player.ProcessCommand(volumeCommand);
			VerifyAll();
		}
*/
		[Test]
		public void StopAfterNullPlay()
		{
			IFileCommand singlePlayCommand = SetupSinglePlay();
			FindTracks("a track.mp3");
			IAudioStream audioStream = SetupAudioStream();
			SetupBackgroundWorker();

			ICommand stopCommand = CreateMock<ICommand>();
			SetupResult.For(stopCommand.CommandType).Return(CommandType.Stop);

			audioStream.Stop();
			ReplayAll();

			player.ProcessCommand(singlePlayCommand);
			CheckPlayerState(true, false, false, "a track.mp3");
			
			player.ProcessCommand(stopCommand);
			
			VerifyAll();

			CheckPlayerState(false, false, true, "a track.mp3");
		}

		[Test]
		public void StopWithNoHistoryDoesNothing()
		{
			ICommand stopCommand = CreateMock<ICommand>();
			SetupResult.For(stopCommand.CommandType).Return(CommandType.Stop);
			ReplayAll();

			CheckPlayerState(false, false, true);
			player.ProcessCommand(stopCommand);
			CheckPlayerState(false, false, true);

			VerifyAll();
		}

		[Test]
		[ExpectedException(typeof(Exception))]
		public void ForcedStopAfterOneSecond()
		{
			IFileCommand singlePlayCommand = SetupSinglePlay();
			FindTracks("a track.mp3");
			IAudioStream audioStream = SetupAudioStream();

			IBackgroundWorkerWrapper backgroundWorker = SetupBackgroundWorker();
			SetupResult.For(backgroundWorker.IsBusy).Return(true);

			ICommand stopCommand = CreateMock<ICommand>();
			SetupResult.For(stopCommand.CommandType).Return(CommandType.Stop);

			audioStream.Stop();
			ReplayAll();

			player.ProcessCommand(singlePlayCommand);
			CheckPlayerState(true, false, false, "a track.mp3");

			player.ProcessCommand(stopCommand);

			VerifyAll();
		}
		/*
		private void SimulatedDelay(object sender, DoWorkEventArgs e)
		{
			Thread.Sleep(200);
		}
		*/
		/*
		IBackgroundWorkerWrapper backgroundWorker = SetupBackgroundWorker();
			backgroundWorker.RunWorkerCompleted += null;
			
			IEventRaiser runWorkerCompletedRaiser = LastCall.On(backgroundWorker).IgnoreArguments().GetEventRaiser();
			audioStream.Stop();
			LastCall.On(audioStream).Repeat.Any();
		*/	
			




		[Test]
		public void InitialStateIsStopped()
		{
			CheckPlayerState(false, false, true);
		}

		[Test]
		[Row("track 1.mp3", "track 2.mp3")]
		[Row("track 1.mp3", "track 1.mp3")]
		public void TryingToPlaySameFilePlaysNextFile(string track1, string track2)
		{
			IFileCommand singlePlayCommandOne = SetupSinglePlay();
			FindTracks(track1);
			IAudioStream audioStream = SetupAudioStream();
			SetupBackgroundWorker();

			audioStream.Stop();
			
			IFileCommand singlePlayCommandTwo = SetupSinglePlay();
			FindTracks(track1, track2);
			SetupAudioStream();
			SetupBackgroundWorker();

			ReplayAll();

			player.ProcessCommand(singlePlayCommandOne);
			CheckPlayerState(true, false, false, track1);
			player.ProcessCommand(singlePlayCommandTwo);
			CheckPlayerState(true, false, false, track2);
			VerifyAll();
		}

		[Test]
		public void IgnoreIsIgnored()
		{
			ICommand ignoreCommand = CreateMock<ICommand>();
			SetupResult.For(ignoreCommand.CommandType).Return(CommandType.Ignore);
			ReplayAll();

			player.ProcessCommand(ignoreCommand);
			VerifyAll();
		}

		[Test]
		public void CanRepeatCurrentTrack()
		{
			IFileCommand singlePlayCommand = SetupSinglePlay();
			FindTracks("a track.mp3");
			IAudioStream audioStream = SetupAudioStream();
			SetupBackgroundWorker();

			ICommand repeatCommand = CreateMock<ICommand>();
			SetupResult.For(repeatCommand.CommandType).Return(CommandType.Repeat);

			audioStream.Stop();
			
			SetupAudioStream();
			SetupBackgroundWorker();
			ReplayAll();

			player.ProcessCommand(singlePlayCommand);
			CheckPlayerState(true, false, false, "a track.mp3");
			player.ProcessCommand(repeatCommand);

			VerifyAll();

			CheckPlayerState(true, false, false, "a track.mp3");
		}

		[Test]
		public void RepeatWhenNoCurrentTrackDoesNothing()
		{
			ICommand repeatCommand = CreateMock<ICommand>();
			SetupResult.For(repeatCommand.CommandType).Return(CommandType.Repeat);

			ReplayAll();

			CheckPlayerState(false, false, true, null);
			player.ProcessCommand(repeatCommand);

			VerifyAll();

			CheckPlayerState(false, false, true, null);
		}

		[Test]
		public void CanPlayPlaylist()
		{
			IFileCommand playlistCommand = SetupPlaylistPlay();
			Expect.Call(fileFinder.FindFiles(playlistCommand.Search, new FileTypes[] { FileTypes.M3U, FileTypes.PLS }, FileListSort.SmallestFirst)).Return(
				new List<string>( new string[] { "a playlist.list" } ));
			Expect.Call(playlistReader.GetTracklist("a playlist.list")).Return(
				new List<string>(new string[] { "track 1.mp3", "track 2.mp3", "track 3.mp3" }));
			SetupAudioStream();
			SetupBackgroundWorker();

			ReplayAll();

			player.ProcessCommand(playlistCommand);
			
			VerifyAll();
			CheckPlayerState(true, false, false, "track 1.mp3");
		}

		[Test]
		public void CanMoveForwardThroughHistory()
		{
			IFileCommand playlistCommand = SetupPlaylistPlay();
			Expect.Call(fileFinder.FindFiles(playlistCommand.Search, new FileTypes[] { FileTypes.M3U, FileTypes.PLS }, FileListSort.SmallestFirst)).Return(
				new List<string>(new string[] { "a playlist.list" }));
			Expect.Call(playlistReader.GetTracklist("a playlist.list")).Return(
				new List<string>(new string[] { "track 1.mp3", "track 2.mp3", "track 3.mp3" }));
			IAudioStream firstTrackAudioStream = SetupAudioStream();
			SetupBackgroundWorker();

			IChangeStateCommand nextTrackCommand = CreateMock<IChangeStateCommand>();
			SetupResult.For(nextTrackCommand.CommandType).Return(CommandType.SkipTrack);
			SetupResult.For(nextTrackCommand.Magnitude).Return(1);

			firstTrackAudioStream.Stop();
			
			SetupAudioStream();
			SetupBackgroundWorker();

			ReplayAll();

			player.ProcessCommand(playlistCommand);
			CheckPlayerState(true, false, false, "track 1.mp3");
			player.ProcessCommand(nextTrackCommand);
			CheckPlayerState(true, false, false, "track 2.mp3");
			VerifyAll();
		}

		[Test]
		public void CanMoveBackwardThroughHistory()
		{
			IFileCommand singlePlayCommandOne = SetupSinglePlay();
			FindTracks("track 1.mp3");
			IAudioStream audioStreamOne = SetupAudioStream();
			SetupBackgroundWorker();

			IFileCommand singlePlayCommandTwo = SetupSinglePlay();
			FindTracks("track 2.mp3");

			audioStreamOne.Stop();
			
			IAudioStream audioStreamTwo = SetupAudioStream();
			SetupBackgroundWorker();

			IChangeStateCommand previousTrackCommand = CreateMock<IChangeStateCommand>();
			SetupResult.For(previousTrackCommand.CommandType).Return(CommandType.SkipTrack);
			SetupResult.For(previousTrackCommand.Magnitude).Return(-1);
	
			audioStreamTwo.Stop();
			
			SetupAudioStream();
			SetupBackgroundWorker();

			ReplayAll();

			player.ProcessCommand(singlePlayCommandOne);
			CheckPlayerState(true, false, false, "track 1.mp3");
			player.ProcessCommand(singlePlayCommandTwo);
			CheckPlayerState(true, false, false, "track 2.mp3");
			player.ProcessCommand(previousTrackCommand);
			VerifyAll();
			
			CheckPlayerState(true, false, false, "track 1.mp3");
		}

		[Test]
		public void MovingBackwardBeyondHistoryWithNoLoopMovesToFirstTrack()
		{
			IFileCommand playlistCommand = SetupPlaylistPlay();
			Expect.Call(fileFinder.FindFiles(playlistCommand.Search, new FileTypes[] { FileTypes.M3U, FileTypes.PLS }, FileListSort.SmallestFirst)).Return(
				new List<string>(new string[] { "a playlist.list" }));
			Expect.Call(playlistReader.GetTracklist("a playlist.list")).Return(
				new List<string>(new string[] { "track 1.mp3", "track 2.mp3", "track 3.mp3" }));
			IAudioStream firstTrackAudioStream = SetupAudioStream();
			SetupBackgroundWorker();

			IChangeStateCommand forwardTrackCommand = CreateMock<IChangeStateCommand>();
			SetupResult.For(forwardTrackCommand.CommandType).Return(CommandType.SkipTrack);
			SetupResult.For(forwardTrackCommand.Magnitude).Return(2);
			firstTrackAudioStream.Stop();
			
			IAudioStream thirdTrackAudioStream = SetupAudioStream();
			SetupBackgroundWorker();

			IChangeStateCommand backTrackCommand = CreateMock<IChangeStateCommand>();
			SetupResult.For(backTrackCommand.CommandType).Return(CommandType.SkipTrack);
			SetupResult.For(backTrackCommand.Magnitude).Return(-7);
			thirdTrackAudioStream.Stop();
			
			SetupAudioStream();
			SetupBackgroundWorker();

			ReplayAll();

			player.ProcessCommand(playlistCommand);
			player.ProcessCommand(forwardTrackCommand);
			CheckPlayerState(true, false, false, "track 3.mp3");
			player.ProcessCommand(backTrackCommand);
			CheckPlayerState(true, false, false, "track 1.mp3");
			VerifyAll();
		}

		[Test]
		public void MovingForwardBeyondHistoryWithNoLoopMovesToFinalTrack()
		{
			IFileCommand playlistCommand = SetupPlaylistPlay();
			Expect.Call(fileFinder.FindFiles(playlistCommand.Search, new FileTypes[] { FileTypes.M3U, FileTypes.PLS }, FileListSort.SmallestFirst)).Return(
				new List<string>(new string[] { "a playlist.list" }));
			Expect.Call(playlistReader.GetTracklist("a playlist.list")).Return(
				new List<string>(new string[] { "track 1.mp3", "track 2.mp3", "track 3.mp3" }));
			IAudioStream firstTrackAudioStream = SetupAudioStream();
			SetupBackgroundWorker();

			IChangeStateCommand forwardTrackCommand = CreateMock<IChangeStateCommand>();
			SetupResult.For(forwardTrackCommand.CommandType).Return(CommandType.SkipTrack);
			SetupResult.For(forwardTrackCommand.Magnitude).Return(7);
			firstTrackAudioStream.Stop();

			SetupAudioStream();
			SetupBackgroundWorker();
			ReplayAll();

			player.ProcessCommand(playlistCommand);
			player.ProcessCommand(forwardTrackCommand);
			CheckPlayerState(true, false, false, "track 3.mp3");
			VerifyAll();
		}

		[Test]
		public void MovingForwardBeyondHistoryWithNoLoopWhenOnCurrentTrackDoesNothing()
		{
			IFileCommand singlePlayCommandOne = SetupSinglePlay();
			FindTracks("track 1.mp3");
			SetupAudioStream();
			SetupBackgroundWorker();

			IChangeStateCommand moveTrackCommand = CreateMock<IChangeStateCommand>();
			SetupResult.For(moveTrackCommand.CommandType).Return(CommandType.SkipTrack);
			SetupResult.For(moveTrackCommand.Magnitude).Return(1);
			ReplayAll();

			player.ProcessCommand(singlePlayCommandOne);
			player.ProcessCommand(moveTrackCommand);
			VerifyAll();
		}

		[Test]
		public void MovingZeroTracksDoesNothing()
		{
			IFileCommand singlePlayCommandOne = SetupSinglePlay();
			FindTracks("track 1.mp3");
			SetupAudioStream();
			SetupBackgroundWorker();

			IChangeStateCommand moveTrackCommand = CreateMock<IChangeStateCommand>();
			SetupResult.For(moveTrackCommand.CommandType).Return(CommandType.SkipTrack);
			SetupResult.For(moveTrackCommand.Magnitude).Return(0);
			ReplayAll();

			player.ProcessCommand(singlePlayCommandOne);
			player.ProcessCommand(moveTrackCommand);
			VerifyAll();
		}

		[Test]
		[Row(CommandType.SkipTrack, 7)]
		[Row(CommandType.SkipTrack, -7)]
		[Row(CommandType.SkipToTrack, 7)]
		[Row(CommandType.SkipToTrack, -7)]
		public void MovingTracksWithNoHistoryDoesNothing(CommandType commandType, int magnitude)
		{
			IChangeStateCommand moveTrackCommand = CreateMock<IChangeStateCommand>();
			SetupResult.For(moveTrackCommand.CommandType).Return(commandType);
			SetupResult.For(moveTrackCommand.Magnitude).Return(magnitude);
			ReplayAll();

			player.ProcessCommand(moveTrackCommand);
			VerifyAll();
		}

		[Test]
		public void CanMoveToArbitraryTracks()
		{
			IFileCommand playlistCommand = SetupPlaylistPlay();
			Expect.Call(fileFinder.FindFiles(playlistCommand.Search, new FileTypes[] { FileTypes.M3U, FileTypes.PLS }, FileListSort.SmallestFirst)).Return(
				new List<string>(new string[] { "a playlist.list" }));
			Expect.Call(playlistReader.GetTracklist("a playlist.list")).Return(
				new List<string>(new string[] { "track 1.mp3", "track 2.mp3", "track 3.mp3", "track 4.mp3", "track 5.mp3" }));
			IAudioStream firstTrackAudioStream = SetupAudioStream();
			SetupBackgroundWorker();

			IChangeStateCommand skipToTrackCommand = CreateMock<IChangeStateCommand>();
			SetupResult.For(skipToTrackCommand.CommandType).Return(CommandType.SkipToTrack);
			SetupResult.For(skipToTrackCommand.Magnitude).Return(4);
			firstTrackAudioStream.Stop();

			SetupAudioStream();
			SetupBackgroundWorker();
			ReplayAll();

			player.ProcessCommand(playlistCommand);
			player.ProcessCommand(skipToTrackCommand);
			CheckPlayerState(true, false, false, "track 4.mp3");
			VerifyAll();
		}

		[Test]
		[Row(4)]
		[Row(-4)]
		public void MovingToTrackOutsideOfHistoryDoesNothing(int skipTo)
		{
			IFileCommand singlePlayCommand = SetupSinglePlay();
			FindTracks("a track.mp3");
			SetupAudioStream();
			SetupBackgroundWorker();

			IChangeStateCommand skipToTrackCommand = CreateMock<IChangeStateCommand>();
			SetupResult.For(skipToTrackCommand.CommandType).Return(CommandType.SkipToTrack);
			SetupResult.For(skipToTrackCommand.Magnitude).Return(skipTo);
			ReplayAll();

			player.ProcessCommand(singlePlayCommand);
			player.ProcessCommand(skipToTrackCommand);

			VerifyAll();
			CheckPlayerState(true, false, false, "a track.mp3");
		}

		[Test]
		public void RepeatedTrackIsNotAddedToHistory()
		{
			IFileCommand singlePlayCommand = SetupSinglePlay();
			FindTracks("a track.mp3");
			IAudioStream audioStream = SetupAudioStream();
			SetupBackgroundWorker();

			ICommand repeatCommand = CreateMock<ICommand>();
			SetupResult.For(repeatCommand.CommandType).Return(CommandType.Repeat);

			audioStream.Stop();
			
			SetupAudioStream();
			SetupBackgroundWorker();
			ReplayAll();

			player.ProcessCommand(singlePlayCommand);
			player.ProcessCommand(repeatCommand);

			VerifyAll();
			Assert.AreEqual(1,player.History.Count);
		}

		[Test]
		public void WorkerThreadPlaysAudio()
		{
			IFileCommand singlePlayCommand = SetupSinglePlay();
			FindTracks("a track.mp3");
			IAudioStream audioStream = SetupAudioStream();
			IBackgroundWorkerWrapper backgroundWorker = SetupBackgroundWorker();
			backgroundWorker.DoWork += null;
			
			IEventRaiser doWorkRaiser = LastCall.On(backgroundWorker).IgnoreArguments().GetEventRaiser();
			backgroundWorker.RunWorkerAsync();
			audioStream.Play();
			ReplayAll();

			player.ProcessCommand(singlePlayCommand);
			doWorkRaiser.Raise(backgroundWorker, new DoWorkEventArgs(null));
		
			VerifyAll();
		}

		[Test]
		public void LoopsCanBeSet()
		{
			ILoopCommand randomLoop = CreateMock<ILoopCommand>();
			SetupResult.For(randomLoop.CommandType).Return(CommandType.Loop);
			SetupResult.For(randomLoop.LoopType).Return(LoopType.Random);

			ILoopCommand sequentialLoop = CreateMock<ILoopCommand>();
			SetupResult.For(sequentialLoop.CommandType).Return(CommandType.Loop);
			SetupResult.For(sequentialLoop.LoopType).Return(LoopType.Sequential);

			ILoopCommand noLoop = CreateMock<ILoopCommand>();
			SetupResult.For(noLoop.CommandType).Return(CommandType.Loop);
			SetupResult.For(noLoop.LoopType).Return(LoopType.None);
			ReplayAll();
			
			Assert.AreEqual(LoopType.None, player.CurrentLoopType);
			player.ProcessCommand(randomLoop);
			Assert.AreEqual(LoopType.Random, player.CurrentLoopType);
			player.ProcessCommand(sequentialLoop);
			Assert.AreEqual(LoopType.Sequential, player.CurrentLoopType);
			player.ProcessCommand(noLoop);
			Assert.AreEqual(LoopType.None, player.CurrentLoopType);
			VerifyAll();
		}

		[Test]
		[Row(7)]
		[Row(-5)]
		public void MovingBeyondHistoryWithSequentialLoopWrapsToWithinHistory(int skipTracks)
		{
			ILoopCommand loopCommand = CreateMock<ILoopCommand>();
			SetupResult.For(loopCommand.CommandType).Return(CommandType.Loop);
			SetupResult.For(loopCommand.LoopType).Return(LoopType.Sequential);

			IFileCommand playlistCommand = SetupPlaylistPlay();
			Expect.Call(fileFinder.FindFiles(playlistCommand.Search, new FileTypes[] { FileTypes.M3U, FileTypes.PLS }, FileListSort.SmallestFirst)).Return(
				new List<string>(new string[] { "a playlist.list" }));
			Expect.Call(playlistReader.GetTracklist("a playlist.list")).Return(
				new List<string>(new string[] { "track 1.mp3", "track 2.mp3", "track 3.mp3" }));
			IAudioStream firstTrackAudioStream = SetupAudioStream();
			SetupBackgroundWorker();

			IChangeStateCommand forwardTrackCommand = CreateMock<IChangeStateCommand>();
			SetupResult.For(forwardTrackCommand.CommandType).Return(CommandType.SkipTrack);
			SetupResult.For(forwardTrackCommand.Magnitude).Return(skipTracks);
			firstTrackAudioStream.Stop();

			SetupAudioStream();
			SetupBackgroundWorker();
			ReplayAll();

			player.ProcessCommand(loopCommand);
			player.ProcessCommand(playlistCommand);
			player.ProcessCommand(forwardTrackCommand);
			CheckPlayerState(true, false, false, "track 2.mp3");
			VerifyAll();
		}

		[Test]
		public void NextTrackStartsWhenCurrentTrackStops()
		{
			IFileCommand playlistCommand = SetupPlaylistPlay();
			Expect.Call(fileFinder.FindFiles(playlistCommand.Search, new FileTypes[] { FileTypes.M3U, FileTypes.PLS }, FileListSort.SmallestFirst)).Return(
				new List<string>(new string[] { "a playlist.list" }));
			Expect.Call(playlistReader.GetTracklist("a playlist.list")).Return(
				new List<string>(new string[] { "track 1.mp3", "track 2.mp3", "track 3.mp3" }));
			
			IAudioStream audioStream = SetupAudioStream();
			IBackgroundWorkerWrapper backgroundWorker = SetupBackgroundWorker();
			backgroundWorker.RunWorkerCompleted += null;
			
			IEventRaiser runWorkerCompletedRaiser = LastCall.On(backgroundWorker).IgnoreArguments().GetEventRaiser();
			audioStream.Stop();
			LastCall.On(audioStream).Repeat.Any();
			
			SetupAudioStream();
			SetupBackgroundWorker();
			
			ReplayAll();

			player.ProcessCommand(playlistCommand);
			CheckPlayerState(true, false, false, "track 1.mp3");
			runWorkerCompletedRaiser.Raise(backgroundWorker, new RunWorkerCompletedEventArgs(null, null, false));
			VerifyAll();
			CheckPlayerState(true, false, false, "track 2.mp3");
		}

		[Test]
		[Row(false)]
		[Row(true)]
		public void RandomTrackThatIsNotCurrentTrackStartsWhenCurrentTrackStopsWithRandomLoopSet(bool unlessThereIsOnlyOneTrack)
		{
			ILoopCommand randomLoopCommand = CreateMock<ILoopCommand>();
			SetupResult.For(randomLoopCommand.LoopType).Return(LoopType.Random);

			IFileCommand playlistCommand = SetupPlaylistPlay();
			Expect.Call(fileFinder.FindFiles(playlistCommand.Search, new FileTypes[] { FileTypes.M3U, FileTypes.PLS }, FileListSort.SmallestFirst)).Return(
				new List<string>(new string[] { "a playlist.list" }));
			if (!unlessThereIsOnlyOneTrack)
				Expect.Call(playlistReader.GetTracklist("a playlist.list")).Return(
					new List<string>(new string[] { "track 1.mp3", "track 2.mp3", "track 3.mp3" }));
			else
				Expect.Call(playlistReader.GetTracklist("a playlist.list")).Return(
					new List<string>(new string[] { "track 1.mp3" }));

			IAudioStream audioStream = SetupAudioStream();
			IBackgroundWorkerWrapper backgroundWorker = SetupBackgroundWorker();
			backgroundWorker.RunWorkerCompleted += null;
			IEventRaiser runWorkerCompletedRaiser = LastCall.On(backgroundWorker).IgnoreArguments().GetEventRaiser();
			
			audioStream.Stop();
			LastCall.On(audioStream).Repeat.Any();

			SetupAudioStream();
			SetupBackgroundWorker();

			ReplayAll();

			player.ProcessCommand(randomLoopCommand);
			player.ProcessCommand(playlistCommand);
			CheckPlayerState(true, false, false, "track 1.mp3");
			runWorkerCompletedRaiser.Raise(backgroundWorker, new RunWorkerCompletedEventArgs(null, null, false));
			
			CheckPlayerState(true, false, false);
			if (!unlessThereIsOnlyOneTrack)
				Assert.AreNotEqual("track 1.mp3", player.CurrentTrack);
			else
				Assert.AreEqual("track 1.mp3", player.CurrentTrack);
			VerifyAll();
		}

		[Test]
		public void NoNewTrackStartsWhenFinalTrackStopsWithNoLoop()
		{
			IFileCommand singlePlayCommand = SetupSinglePlay();
			FindTracks("a track.mp3");
			
			IAudioStream audioStream = SetupAudioStream();
			IBackgroundWorkerWrapper backgroundWorker = SetupBackgroundWorker();
			backgroundWorker.RunWorkerCompleted += null;
			
			IEventRaiser runWorkerCompletedRaiser = LastCall.On(backgroundWorker).IgnoreArguments().GetEventRaiser();
			audioStream.Stop();
			
			ReplayAll();

			player.ProcessCommand(singlePlayCommand);
			CheckPlayerState(true, false, false, "a track.mp3");
			runWorkerCompletedRaiser.Raise(backgroundWorker, new RunWorkerCompletedEventArgs(null, null, false));
			VerifyAll();
			CheckPlayerState(false, false, true, "a track.mp3");
		}

		[Test]
		public void CanLoadAListOfTracksGeneratedFromSearchQuery()
		{
			IFileCommand wholeQueryPlayCommand = SetupWholeQueryPlay();
			FindTracks("track 1.mp3", "track 2.mp3", "track 3.mp3");

			SetupAudioStream();
			SetupBackgroundWorker();
			
			ReplayAll();

			player.ProcessCommand(wholeQueryPlayCommand);
			VerifyAll();
			CheckPlayerState(true, false, false, "track 1.mp3");
			Assert.AreEqual(3, player.History.Count);
		}

		[Test]
		public void PausingOrUnpausingWhileStoppedDoesNothing()
		{
			ICommand playPauseCommand = CreateMock<ICommand>();
			SetupResult.For(playPauseCommand.CommandType).Return(CommandType.PlayPause);
			ReplayAll();

			CheckPlayerState(false, false, true, null);
			player.ProcessCommand(playPauseCommand);
			CheckPlayerState(false, false, true, null);
			VerifyAll();
		}

		[Test]
		public void LoadingAnEmptyListOfTracksGeneratedFromSearchQueryDoesNothing()
		{
			IFileCommand wholeQueryPlayCommand = SetupWholeQueryPlay();
			FindTracks();
			SetupUnsuccessfulEvent();
			ReplayAll();

			player.ProcessCommand(wholeQueryPlayCommand);
			VerifyAll();
			CheckPlayerState(false, false, true, null);
			Assert.AreEqual(0, player.History.Count);
		}
		//^^more tests like the above for the new "play whole query" thing

		[Test]
		public void LoadingAnEmptyPlaylistDoesNothing()
		{
			IFileCommand playlistCommand = SetupPlaylistPlay();
			Expect.Call(fileFinder.FindFiles(null, new FileTypes[] { FileTypes.M3U, FileTypes.PLS }, FileListSort.SmallestFirst)).IgnoreArguments().Return(
				new List<string>());
			
			SetupUnsuccessfulEvent();
			ReplayAll();

			player.ProcessCommand(playlistCommand);
			VerifyAll();
			CheckPlayerState(false, false, true, null);
			Assert.AreEqual(0, player.History.Count);
		}
		

		[Test]
		public void BadSoundCanPlay()
		{
			IFileCommand unsuccessfulPlayCommand = SetupSingleUnsucessfulPlay();
			FindTracks();

			//can't use SetupUnsuccessfulEvent() as we are testing the process itself
			IAudioStream badEventAudio = CreateMock<IAudioStream>();
			Expect.Call(audioStreamFactory.NewAudioStream()).Return(badEventAudio);
			Expect.Call(badEventAudio.Open(null)).IgnoreArguments().Return(true);
			IBackgroundWorkerWrapper badBackgroundWorker = CreateMock<IBackgroundWorkerWrapper>();
			Expect.Call(backgroundWorkerFactory.NewBackgroundWorker()).Return(badBackgroundWorker);
			badBackgroundWorker.DoWork += null;
			IEventRaiser badDoWorkRaiser = LastCall.On(badBackgroundWorker).IgnoreArguments().GetEventRaiser();
			badBackgroundWorker.RunWorkerCompleted += null;
			LastCall.IgnoreArguments();
			badBackgroundWorker.RunWorkerAsync();
			badEventAudio.Play();
			ReplayAll();

			player.ProcessCommand(unsuccessfulPlayCommand);
			badDoWorkRaiser.Raise(badBackgroundWorker, new DoWorkEventArgs(null));
			VerifyAll();
		}

		//this one is for playing bad sounds while already playing another track, one after that for while paused
		[Test]
		public void BadSoundCanPlayWhileATrackIsPlaying()
		{
			IFileCommand singlePlayCommand = SetupSinglePlay();
			FindTracks("a track.mp3");
			IAudioStream audioStream = SetupAudioStream();
			SetupBackgroundWorker();


			IFileCommand unsuccessfulPlayCommand = SetupSingleUnsucessfulPlay();
			FindTracks();

			audioStream.PlayPause();

			IAudioStream badEventAudio = CreateMock<IAudioStream>();
			Expect.Call(audioStreamFactory.NewAudioStream()).Return(badEventAudio);
			Expect.Call(badEventAudio.Open(null)).IgnoreArguments().Return(true);
			IBackgroundWorkerWrapper badBackgroundWorker = CreateMock<IBackgroundWorkerWrapper>();
			Expect.Call(backgroundWorkerFactory.NewBackgroundWorker()).Return(badBackgroundWorker);
			badBackgroundWorker.DoWork += null;
			IEventRaiser badDoWorkRaiser = LastCall.On(badBackgroundWorker).IgnoreArguments().GetEventRaiser();
			badBackgroundWorker.RunWorkerCompleted += null;
			IEventRaiser badWorkerCompletedRaiser = LastCall.On(badBackgroundWorker).IgnoreArguments().GetEventRaiser();
			LastCall.IgnoreArguments();
			badBackgroundWorker.RunWorkerAsync();
			badEventAudio.Play();
			badEventAudio.Stop();
			audioStream.PlayPause();

			ReplayAll();

			player.ProcessCommand(singlePlayCommand);
			player.ProcessCommand(unsuccessfulPlayCommand);
			badDoWorkRaiser.Raise(badBackgroundWorker, new DoWorkEventArgs(null));
			badWorkerCompletedRaiser.Raise(badBackgroundWorker, new RunWorkerCompletedEventArgs(null, null, false));
			VerifyAll();
		}

		[Test]
		public void BadSoundCanPlayWhileATrackIsPaused()
		{
			IFileCommand singlePlayCommand = SetupSinglePlay();
			FindTracks("a track.mp3");
			IAudioStream audioStream = SetupAudioStream();
			SetupBackgroundWorker();

			audioStream.PlayPause();

			IFileCommand unsuccessfulPlayCommand = SetupSingleUnsucessfulPlay();
			FindTracks();

			IAudioStream badEventAudio = CreateMock<IAudioStream>();
			Expect.Call(audioStreamFactory.NewAudioStream()).Return(badEventAudio);
			Expect.Call(badEventAudio.Open(null)).IgnoreArguments().Return(true);
			IBackgroundWorkerWrapper badBackgroundWorker = CreateMock<IBackgroundWorkerWrapper>();
			Expect.Call(backgroundWorkerFactory.NewBackgroundWorker()).Return(badBackgroundWorker);
			badBackgroundWorker.DoWork += null;
			IEventRaiser badDoWorkRaiser = LastCall.On(badBackgroundWorker).IgnoreArguments().GetEventRaiser();
			badBackgroundWorker.RunWorkerCompleted += null;
			IEventRaiser badWorkerCompletedRaiser = LastCall.On(badBackgroundWorker).IgnoreArguments().GetEventRaiser();
			LastCall.IgnoreArguments();
			badBackgroundWorker.RunWorkerAsync();
			badEventAudio.Play();
			badEventAudio.Stop();
			
			ReplayAll();

			player.ProcessCommand(singlePlayCommand);
			player.ProcessCommand(new Command(CommandType.PlayPause));

			player.ProcessCommand(unsuccessfulPlayCommand);
			badDoWorkRaiser.Raise(badBackgroundWorker, new DoWorkEventArgs(null));
			badWorkerCompletedRaiser.Raise(badBackgroundWorker, new RunWorkerCompletedEventArgs(null, null, false));
			VerifyAll();
		}

		[Test]
		public void CurrentTrackOrdinalWhenNoHistoryIsZero()
		{
			Assert.AreEqual(0, player.CurrentTrackOrdinal);
		}

		[Test]
		public void CurrentTrackOrdinalCanBeRead()
		{
			IChangeStateCommand nextCommand = new ChangeStateCommand(CommandType.SkipTrack, 1);
		
			IFileCommand playlistCommand = SetupPlaylistPlay();
			Expect.Call(fileFinder.FindFiles(playlistCommand.Search, new FileTypes[] { FileTypes.M3U, FileTypes.PLS }, FileListSort.SmallestFirst)).Return(
				new List<string>(new string[] { "a playlist.list" }));
			Expect.Call(playlistReader.GetTracklist("a playlist.list")).Return(
				new List<string>(new string[] { "track 1.mp3", "track 2.mp3", "track 3.mp3" }));
			
			IAudioStream audioStream = SetupAudioStream();
			SetupBackgroundWorker();
			
			audioStream.Stop();
			LastCall.On(audioStream).Repeat.Any();

			SetupAudioStream();
			SetupBackgroundWorker();
			
			ReplayAll();

			player.ProcessCommand(playlistCommand);
			player.ProcessCommand(nextCommand);
			Assert.AreEqual(2, player.CurrentTrackOrdinal);
			VerifyAll();
		}

		[Test]
		public void AddingATrackAddsItToTheCurrentPosition()
		{
			IFileCommand playlistCommand = SetupPlaylistPlay();
			Expect.Call(fileFinder.FindFiles(playlistCommand.Search, new FileTypes[] { FileTypes.M3U, FileTypes.PLS }, FileListSort.SmallestFirst)).Return(
				new List<string>(new string[] { "a playlist.list" }));
			Expect.Call(playlistReader.GetTracklist("a playlist.list")).Return(
				new List<string>(new string[] { "track 1.mp3", "track 2.mp3", "track 3.mp3", "track 4.mp3", "track 5.mp3" }));
			IAudioStream firstTrackAudioStream = SetupAudioStream();
			SetupBackgroundWorker();
            
			IChangeStateCommand skipToThreeCommand = CreateMock<IChangeStateCommand>();
			SetupResult.For(skipToThreeCommand.CommandType).Return(CommandType.SkipToTrack);
			SetupResult.For(skipToThreeCommand.Magnitude).Return(3);
			firstTrackAudioStream.Stop();

			IAudioStream secondTrackAudioStream = SetupAudioStream();
			SetupBackgroundWorker();

			IFileCommand singlePlayCommand = SetupSinglePlay();
			FindTracks("a new track.mp3");

			secondTrackAudioStream.Stop();

			IAudioStream thirdTrackAudioStream = SetupAudioStream();
			SetupBackgroundWorker();


			IChangeStateCommand skipForwardOneCommand = CreateMock<IChangeStateCommand>();
			SetupResult.For(skipForwardOneCommand.CommandType).Return(CommandType.SkipTrack);
			SetupResult.For(skipForwardOneCommand.Magnitude).Return(1);
			thirdTrackAudioStream.Stop();

			SetupAudioStream();
			SetupBackgroundWorker();

			ReplayAll();

			player.ProcessCommand(playlistCommand);
			player.ProcessCommand(skipToThreeCommand);
			CheckPlayerState(true, false, false, "track 3.mp3");
			player.ProcessCommand(singlePlayCommand);
			CheckPlayerState(true, false, false, "a new track.mp3");
			player.ProcessCommand(skipForwardOneCommand);
			CheckPlayerState(true, false, false, "track 4.mp3");
			VerifyAll();
		}


	}
}