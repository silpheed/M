using System;
using System.Collections.Generic;
using System.IO;
using MbUnit.Framework;
using Rhino.Mocks;

namespace m.tests
{
	[TestFixture]
	public class FileFinderFixture : MockingFixture
	{
		private IFileFinder _fileFinder;
		private IFileSystemFacade _fileSystem;
		private IConfigSettingsFacade _configSettings;
		private ISearchQuery _searchQuery;

		public override void SetUp()
		{
			_fileSystem = CreateMock<IFileSystemFacade>();
			_configSettings = DynamicMock<IConfigSettingsFacade>();
			_searchQuery = CreateMock<ISearchQuery>();
			_fileFinder = new FileFinder(_fileSystem, _configSettings);
		}

		[Test]
		public void NoDirs()
		{
			Expect.Call(_configSettings.MusicDirectories).Return(new List<string>());
			ReplayAll();
			
			IList<string> result = _fileFinder.FindFiles(null, null);
			Assert.AreEqual(0, result.Count);
			VerifyAll();
		}

		[Test]
		public void NoFiles()
		{
			IList<FileTypes> mp3 = new List<FileTypes>(new FileTypes[] { FileTypes.MP3 });

			SetupResult.For(_configSettings.MusicDirectories).Return(new List<string>(new string[] { "somePath" }));
			SetupResult.For(_searchQuery.WantedAtoms).Return(new List<string>(new string[] { "dog", "horse" }));
			SetupResult.For(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(new string[] { });
			SetupResult.For(_searchQuery.UnwantedAtoms).Return(new List<string>(new string[] { "cat", "fish" }));

			ReplayAll();

			IList<string> result = _fileFinder.FindFiles(_searchQuery, mp3);
			Assert.AreEqual(0, result.Count);
			VerifyAll();
		}

		[Test]
		[Row( "dog" )]
		[Row( "o" )]
		public void OneHitOneDirectory(string goodOne)
		{
			IList<FileTypes> mp3 = new List<FileTypes>(new FileTypes[] { FileTypes.MP3 });

			string[] foundFiles = new string[] { "somePath/someOtherPath/a dog track.mp3", "somePath/someOtherPath/a dog and cat track.mp3"  };

			SetupResult.For(_configSettings.MusicDirectories).Return(new List<string>(new string[] { "somePath" }));
			SetupResult.For(_searchQuery.WantedAtoms).Return(new List<string>(new string[] { goodOne }));
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundFiles);
			SetupResult.For(_searchQuery.UnwantedAtoms).Return(new List<string>(new string[] { "cat", "fish" }));
			ReplayAll();

			IList<string> result = _fileFinder.FindFiles(_searchQuery, mp3);
			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.Contains("somePath/someOtherPath/a dog track.mp3"));
			VerifyAll();
		}

		[Test]
		public void MultipleHitsOneDirectory()
		{
			IList<FileTypes> mp3 = new List<FileTypes>(new FileTypes[] { FileTypes.MP3 });
			
			string[] foundFiles = new string[] { "somePath/someOtherPath/a dog track.mp3", 
				"somePath/someFishPath/a dog and cat track.mp3", 
				"somePath/someOtherPath/a dog and fish track.mp3"
			};

			SetupResult.For(_searchQuery.WantedAtoms).Return(new List<string>(new string[] { "dog" }));
			SetupResult.For(_configSettings.MusicDirectories).Return(new List<string>(new string[] { "somePath" }));
			SetupResult.For(_searchQuery.UnwantedAtoms).Return(new List<string>(new string[] { "fish" }));
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundFiles);
			ReplayAll();

			IList<string> result = _fileFinder.FindFiles(_searchQuery, mp3);
			Assert.AreEqual(2, result.Count);
			Assert.IsTrue(result.Contains("somePath/someOtherPath/a dog track.mp3"));
			Assert.IsTrue(result.Contains("somePath/someFishPath/a dog and cat track.mp3"));

			VerifyAll();
		}

		[Test]
		[Row("dog")]
		[Row("d")]
		public void MultipleHitsMultipleDirectories(string goodOne)
		{
			IList<FileTypes> mp3 = new List<FileTypes>(new FileTypes[] { FileTypes.MP3 });

			string[] foundFilesDir1 = new string[] { "somePath/someOtherPath/a dog track.mp3", "somePath/someOtherPath/a dog and cat track.mp3" };
			string[] foundFilesDir2 = new string[] { "" };
			string[] foundFilesDir3 = new string[] { "morePaths/a dog track.mp3", "morePaths/a dog and cat track.mp3" };

			SetupResult.For(_searchQuery.WantedAtoms).Return(new List<string>(new string[] { goodOne }));
			SetupResult.For(_configSettings.MusicDirectories).Return(new List<string>(new string[] { "somePath1", "somePath2", "somePath3" }));
			SetupResult.For(_searchQuery.UnwantedAtoms).Return(new List<string>(new string[] { "cat", "fish" }));
			
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundFilesDir1);
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundFilesDir2);
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundFilesDir3);
			ReplayAll();

			IList<string> result = _fileFinder.FindFiles(_searchQuery, mp3);
			Assert.AreEqual(2, result.Count);
			Assert.IsTrue(result.Contains("somePath/someOtherPath/a dog track.mp3"));
			Assert.IsTrue(result.Contains("morePaths/a dog track.mp3"));
			
			VerifyAll();
		}
		
		[Test]
		[Row("a dog", "horse")]
		public void MultipleHitsMultipleDirectoriesQuotedGood(string goodOne, string goodTwo)
		{
			IList<FileTypes> mp3 = new List<FileTypes>(new FileTypes[] { FileTypes.MP3 });

			string[] foundFilesDir1 = new string[] { "somePath/someOtherPath/a dog and horse track.mp3", "somePath/someOtherPath/a doghorse.mp3" };
			string[] foundFilesDir2 = new string[] { };
			string[] foundFilesDir3 = new string[] { "morePaths/a dog and cat and horse track.mp3", "morePaths/a dog and horse track.mp3waa" };

			SetupResult.For(_searchQuery.WantedAtoms).Return(new List<string>(new string[] { goodOne, goodTwo }));
			SetupResult.For(_configSettings.MusicDirectories).Return(new List<string>(new string[] { "somePath1", "somePath2", "somePath3" }));
			SetupResult.For(_searchQuery.UnwantedAtoms).Return(new List<string>(new string[] { "cat", "fish" }));
			
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundFilesDir1);
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundFilesDir2);
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundFilesDir3);
			ReplayAll();

			IList<string> result = _fileFinder.FindFiles(_searchQuery, mp3);
			Assert.AreEqual(2, result.Count);
			Assert.IsTrue(result.Contains("somePath/someOtherPath/a dog and horse track.mp3"));
			Assert.IsTrue(result.Contains("somePath/someOtherPath/a doghorse.mp3"));
			

			VerifyAll();
		}
		
		[Test]
		[Row("")]
		[Row(null)]
		public void EmptySearchFindsEverything(string goodOne)
		{
			IList<FileTypes> mp3 = new List<FileTypes>(new FileTypes[] { FileTypes.MP3 });

			string[] foundFilesDir1 = new string[] { "somePath/someOtherPath/a dog track.mp3", "somePath/someOtherPath/a dog and cat track.mp3" };
			string[] foundFilesDir2 = new string[] { "aSecondPath/hitless.mp3" };
			string[] foundFilesDir3 = new string[] { "morePaths/a dog track.mp3", "morePaths/a dog and cat track.mp3", "morePaths/a horse track.mp3" };

			SetupResult.For(_searchQuery.WantedAtoms).Return(new List<string>(new string[] { goodOne }));
			SetupResult.For(_configSettings.MusicDirectories).Return(new List<string>(new string[] { "somePath1", "somePath2", "somePath3" }));
			SetupResult.For(_searchQuery.UnwantedAtoms).Return(new List<string>(new string[] { }));
			
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundFilesDir1);
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundFilesDir2);
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundFilesDir3);
			ReplayAll();

			IList<string> result = _fileFinder.FindFiles(_searchQuery, mp3);
			Assert.AreEqual(6, result.Count);
			
			VerifyAll();
		}

		[Test]
		public void ReturnOnlyRequestedExtension()
		{
			IList<FileTypes> mp3 = new List<FileTypes>(new FileTypes[] { FileTypes.MP3 });

			string[] foundFilesDir1 = new string[] { "somePath/someOtherPath/a dog track.cat", "somePath/someOtherPath/a dog and cat track.mp3" };
			string[] foundFilesDir2 = new string[] { "aSecondPath/hitless.mp3mp3" };
			string[] foundFilesDir3 = new string[] { "morePaths/a dog track.mp3", "morePaths/a dog and cat track.mp333", "morePaths/a horse track.mp3" };

			SetupResult.For(_searchQuery.WantedAtoms).Return(new List<string>(new string[] { }));
			SetupResult.For(_configSettings.MusicDirectories).Return(new List<string>(new string[] { "somePath1", "somePath2", "somePath3" }));
			SetupResult.For(_searchQuery.UnwantedAtoms).Return(new List<string>(new string[] { }));

			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundFilesDir1);
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundFilesDir2);
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundFilesDir3);
			ReplayAll();

			IList<string> result = _fileFinder.FindFiles(_searchQuery, mp3);

			Assert.AreEqual(3, result.Count);
			Assert.IsTrue(result.Contains("somePath/someOtherPath/a dog and cat track.mp3"));
			Assert.IsTrue(result.Contains("morePaths/a dog track.mp3"));
			Assert.IsTrue(result.Contains("morePaths/a horse track.mp3"));

			VerifyAll();
		}

		[Test]
		public void MultipleExtensions()
		{
			IList<FileTypes> mp3m3u = new List<FileTypes>(new FileTypes[] { FileTypes.MP3, FileTypes.M3U });

			string[] foundFilesDir1 = new string[] { "somePath/someOtherPath/a dog track.cat", "somePath/someOtherPath/a dog and cat track.mp3" };
			string[] foundFilesDir2 = new string[] { "aSecondPath/hitless.mp3mp3" };
			string[] foundFilesDir3 = new string[] { "morePaths/a dog and cat track.mp333", "morePaths/a horse track.mp3" };

			string[] foundPlaylistsDir1 = new string[] { };
			string[] foundPlaylistsDir2 = new string[] { };
			string[] foundPlaylistsDir3 = new string[] { "morePaths/a dog tracklist.m3u" };

			SetupResult.For(_searchQuery.WantedAtoms).Return(new List<string>(new string[] { }));
			SetupResult.For(_configSettings.MusicDirectories).Return(new List<string>(new string[] { "somePath1", "somePath2", "somePath3" }));
			SetupResult.For(_searchQuery.UnwantedAtoms).Return(new List<string>(new string[] { }));

			//mp3
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundFilesDir1);
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundFilesDir2);
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundFilesDir3);

			//m3u
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundPlaylistsDir1);
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundPlaylistsDir2);
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundPlaylistsDir3);
			ReplayAll();

			IList<string> result = _fileFinder.FindFiles(_searchQuery, mp3m3u);
			Assert.AreEqual(3, result.Count);
			Assert.IsTrue(result.Contains("somePath/someOtherPath/a dog and cat track.mp3"));
			Assert.IsTrue(result.Contains("morePaths/a horse track.mp3"));
			Assert.IsTrue(result.Contains("morePaths/a dog tracklist.m3u"));
			
			VerifyAll();
		}

		[Test]
		public void AllExtensions()
		{
			IList<FileTypes> all = new List<FileTypes>(new FileTypes[] { FileTypes.ALL });

			string[] foundFilesDir1 = new string[] { "somePath/someOtherPath/a dog track.cat", "somePath/someOtherPath/a dog and cat track.mp3" };
			string[] foundFilesDir2 = new string[] { "aSecondPath/hitless.mp3mp3" };
			string[] foundFilesDir3 = new string[] { "morePaths/a dog and cat track.mp333", "morePaths/a horse track.mp3" };

			SetupResult.For(_searchQuery.WantedAtoms).Return(new List<string>(new string[] { }));
			SetupResult.For(_configSettings.MusicDirectories).Return(new List<string>(new string[] { "somePath1", "somePath2", "somePath3" }));
			SetupResult.For(_searchQuery.UnwantedAtoms).Return(new List<string>(new string[] { }));

			//all
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundFilesDir1);
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundFilesDir2);
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundFilesDir3);

			ReplayAll();

			IList<string> result = _fileFinder.FindFiles(_searchQuery, all);
			Assert.AreEqual(5, result.Count);
			Assert.IsTrue(result.Contains("somePath/someOtherPath/a dog and cat track.mp3"));
			Assert.IsTrue(result.Contains("morePaths/a horse track.mp3"));
			Assert.IsTrue(result.Contains("morePaths/a dog and cat track.mp333"));
			Assert.IsTrue(result.Contains("somePath/someOtherPath/a dog track.cat"));
			Assert.IsTrue(result.Contains("aSecondPath/hitless.mp3mp3"));
			
			VerifyAll();
		}

		[Test]
		public void BadDirectory()
		{
			SetupResult.For(_configSettings.MusicDirectories).Return(new List<string>(new string[] { "somePath" }));
			SetupResult.For(_searchQuery.WantedAtoms).Return(new List<string>(new string[] { "dog" }));
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Throw(new Exception());
			SetupResult.For(_searchQuery.UnwantedAtoms).Return(new List<string>(new string[] { "cat", "fish" }));
			ReplayAll();

			IList<string> result = _fileFinder.FindFiles(_searchQuery, FileTypes.MP3);
			Assert.AreEqual(0, result.Count);

			VerifyAll();
		}
		
		[Test]
		[Row(FileListSort.SmallestFirst, "a dog track.mp3", "a dog and cat track.mp3", "a dog and fish track.mp3")]
		[Row(FileListSort.LargestFirst, "a dog and fish track.mp3", "a dog and cat track.mp3", "a dog track.mp3")]
		[Row(FileListSort.None, "a dog track.mp3", "a dog and fish track.mp3", "a dog and cat track.mp3")]
		public void VariousResultSorts(FileListSort fileListSort, string first, string second, string third)
		{
			string[] foundFiles = new string[] { "a dog track.mp3", 
				"a dog and fish track.mp3",
				"a dog and cat track.mp3"
			};

			SetupResult.For(_searchQuery.WantedAtoms).Return(new List<string>(new string[] { }));
			SetupResult.For(_configSettings.MusicDirectories).Return(new List<string>(new string[] { "somePath" }));
			SetupResult.For(_searchQuery.UnwantedAtoms).Return(new List<string>(new string[] { }));
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundFiles);
			ReplayAll();

			IList<string> result = _fileFinder.FindFiles(_searchQuery, FileTypes.MP3, fileListSort);
			Assert.AreEqual(3, result.Count);
			Assert.AreEqual(first, result[0]);
			Assert.AreEqual(second, result[1]);
			Assert.AreEqual(third, result[2]);

			VerifyAll();
		}

		[Test]
		public void RandomResultSort()
		{
			string[] foundFiles = new string[] { "a dog track.mp3", 
				"a dog and fish track.mp3",
				"a dog and cat track.mp3"
			};

			SetupResult.For(_searchQuery.WantedAtoms).Return(new List<string>(new string[] {}));
			SetupResult.For(_configSettings.MusicDirectories).Return(new List<string>(new string[] {"somePath"}));
			SetupResult.For(_searchQuery.UnwantedAtoms).Return(new List<string>(new string[] {}));
			Expect.Call(_fileSystem.GetFiles(null, null, SearchOption.AllDirectories)).IgnoreArguments().Return(foundFiles);
			ReplayAll();

			IList<string> result = _fileFinder.FindFiles(_searchQuery, FileTypes.MP3, FileListSort.Random);
			Assert.AreEqual(3, result.Count);
		//can't test much to do with randomness, just make sure that no results disappear

			VerifyAll();
		}

	}
}
