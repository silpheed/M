using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MbUnit.Framework;
using Rhino.Mocks;

namespace m.tests
{
	[TestFixture]
	public class PlaylistReaderFixture : MockingFixture
	{
		private IFileSystemFacade fileSystem;
		private IPlaylistReader playlistReader;

		public override void SetUp()
		{
			fileSystem = CreateMock<IFileSystemFacade>();
			playlistReader = new PlaylistReader(fileSystem);
		}

		[Test]
		public void NullReturnsNull()
		{
			Assert.IsNull(playlistReader.GetTracklist(null));
		}

		[Test]
		public void EmptyReturnsEmptyList()
		{
			Assert.AreEqual(0, playlistReader.GetTracklist(string.Empty).Count);
		}

		//can't use Tests as they don't like string[] for input
		[Test]
		public void DetectAndReadM3u()
		{
			Expect.Call(fileSystem.ReadAllLines(null)).IgnoreArguments().Return(GetM3u());
			SetupResult.For(fileSystem.FileExists(null)).IgnoreArguments().Return(true);
			ReplayAll();

			IList<string> result = playlistReader.GetTracklist(Path.Combine(Path.Combine("c:","tunes"),"playlist.m3u"));
			Assert.AreEqual(36, result.Count);
			Assert.Contains(result[0], "01 Ghosts I.mp3");
			Assert.Contains(result[35], "36 Ghosts IV.mp3");

			VerifyAll();
		}

		[Test]
		public void DetectAndReadPls()
		{
			Expect.Call(fileSystem.ReadAllLines(null)).IgnoreArguments().Return(GetPls());
			SetupResult.For(fileSystem.FileExists(null)).IgnoreArguments().Return(true);
			ReplayAll();

			IList<string> result = playlistReader.GetTracklist(Path.Combine(Path.Combine("c:", "tunes"), "playlist.pls"));
			Assert.AreEqual(13, result.Count);
			Assert.Contains(result[0], "Dandy Warhols - Godless.mp3");
			Assert.Contains(result[12], "Dandy Warhols - The Gospel.mp3");

			VerifyAll();
		}

		[Test]
		public void DetectAndReadPlaintext()
		{
			Expect.Call(fileSystem.ReadAllLines(null)).IgnoreArguments().Return(GetPlaintext());
			SetupResult.For(fileSystem.FileExists(null)).IgnoreArguments().Return(true);
			ReplayAll();

			IList<string> result = playlistReader.GetTracklist(Path.Combine(Path.Combine("c:", "tunes"), "playlist.wtf"));
			Assert.AreEqual(17, result.Count);
			Assert.Contains(result[0], "Elton John - 01 - Funeral For A Friend (Love Lies Bleeding).mp3");
			Assert.Contains(result[16], "Elton John - 17 - Harmony.mp3");

			VerifyAll();
		}

		[Test]
		public void CantReadListFile()
		{
			Expect.Call(fileSystem.ReadAllLines(null)).IgnoreArguments().Throw(new FileNotFoundException());
			ReplayAll();

			IList<string> result = playlistReader.GetTracklist(Path.Combine(Path.Combine("c:", "tunes"), "playlist.wtf"));
			Assert.IsNull(result);
			VerifyAll();
		}

		[Test]
		public void PlsWithBadLine()
		{
			string[] pls = GetPls();
			pls[7] = "Fileerfs=Bad.mp3";
			Expect.Call(fileSystem.ReadAllLines(null)).IgnoreArguments().Return(pls);
			SetupResult.For(fileSystem.FileExists(null)).IgnoreArguments().Return(true);
			ReplayAll();

			IList<string> result = playlistReader.GetTracklist(Path.Combine(Path.Combine("c:", "tunes"), "playlist.pls"));
			Assert.AreEqual(13, result.Count);
			foreach (string track in result)
				Assert.IsFalse(track.Contains("Bad.mp3"));
			VerifyAll();
		}
		//no need for an M3uWithBadLine(), m3u files are just plaintext files with extra information
	
		[Test]
		public void DiscardNonExistantTracks()
		{
			Expect.Call(fileSystem.ReadAllLines(null)).IgnoreArguments().Return(GetPlaintext());
			Expect.Call(fileSystem.FileExists(null)).IgnoreArguments().Repeat.Times(3).Return(true);
			Expect.Call(fileSystem.FileExists(null)).IgnoreArguments().Repeat.Once().Return(false);
			Expect.Call(fileSystem.FileExists(null)).IgnoreArguments().Repeat.Times(13).Return(true);
			
			ReplayAll();

			IList<string> result = playlistReader.GetTracklist(Path.Combine(Path.Combine("c:", "tunes"), "playlist.wtf"));
			Assert.AreEqual(16, result.Count);
			foreach (string track in result)
				Assert.IsFalse(track.Contains("Elton John - 04 - Goodbye Yellow Brick Road.mp3"));
			VerifyAll();
		}
		
		[Test]
		public void DiscardUrls()
		{
			Expect.Call(fileSystem.ReadAllLines(null)).IgnoreArguments().Return(GetM3uWithUrls());
			Expect.Call(fileSystem.FileExists(null)).IgnoreArguments().Return(true);
			Expect.Call(fileSystem.FileExists(null)).IgnoreArguments().Return(false);
			Expect.Call(fileSystem.FileExists(null)).IgnoreArguments().Return(true);
			Expect.Call(fileSystem.FileExists(null)).IgnoreArguments().Return(true);
			ReplayAll();

			IList<string> result = playlistReader.GetTracklist(Path.Combine(Path.Combine("c:", "tunes"), "playlist.wtf"));
			Assert.AreEqual(3, result.Count);
			Assert.Contains(result[0], "01 Ghosts I.mp3");
			Assert.Contains(result[2], "04 Ghosts I.mp3");

			VerifyAll();
		}

		[Test]
		public void CantMakeAbsoluteBadRelativeFile()
		{
			IList<string> relatives = new List<string>(new string[] { "track 01.mp3", "track 02.mp3", null, "track 04.mp3" });
			playlistReader.ConvertListOfRelativeFilesToAbsolute(relatives, Path.Combine(Path.Combine("c:", "tunes"), "file.wtf"));
			Assert.AreEqual(3,relatives.Count);
		}

		[Test]
		public void CantMakeAbsoluteBadRoot()
		{
			IList<string> relatives = new List<string>(new string[] { "track 01.mp3", "track 02.mp3", "track 03.mp3", "track 04.mp3" });
			playlistReader.ConvertListOfRelativeFilesToAbsolute(relatives, "dummydir");
			Assert.AreEqual(0, relatives.Count);
		}

		private static string GetFromEmbeddedFile(string filename)
		{
			Assembly asm = Assembly.GetExecutingAssembly();
			StreamReader reader = new StreamReader(asm.GetManifestResourceStream(filename));
			return reader.ReadToEnd();
		}

		private static string[] GetM3u()
		{
			return GetFromEmbeddedFile(@"m.tests.playlists.m3u").Split(Environment.NewLine.ToCharArray());
		}

		private static string[] GetPls()
		{
			return GetFromEmbeddedFile(@"m.tests.playlists.pls").Split(Environment.NewLine.ToCharArray());
		}

		private static string[] GetPlaintext()
		{
			return GetFromEmbeddedFile(@"m.tests.playlists.plaintext").Split(Environment.NewLine.ToCharArray());
		}

		private static string[] GetM3uWithUrls()
		{
			return GetFromEmbeddedFile(@"m.tests.playlists.m3uwithurl").Split(Environment.NewLine.ToCharArray());
		}
	}
}