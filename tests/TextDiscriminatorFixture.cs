using System.Collections.Generic;
using MbUnit.Framework;
using Rhino.Mocks;

namespace m.tests
{
	[TestFixture]
	public class TextDiscriminatorFixture : MockingFixture
	{
		private ITextDiscriminator _textDiscriminator;
		private ISearchQueryFactory _searchQueryFactory;
		private ISearchQuery _searchQuery;
		private ICommandFactory _commandFactory;

		public override void SetUp()
		{
			_commandFactory = CreateMock<ICommandFactory>();
			_searchQueryFactory = CreateMock<ISearchQueryFactory>();
			
			_textDiscriminator = new TextDiscriminator(_commandFactory, _searchQueryFactory);
			_searchQuery = Stub<ISearchQuery>();

		}

		[Test]
		[Row("m a test", "a test")]
		[Row("m", "")]
		[Row("no prefix a test", "no prefix a test")]
		//the g tests are so that a "go to track" command that makes no sense is treated as a search
		[Row("g", "g")]
		[Row("g dawg", "g dawg")]
		public void PlaySingleShortestSearch(string input, string expectedSearch)
		{
			IFileCommand _FileCommand = Stub<IFileCommand>();

			Expect.Call(_searchQueryFactory.NewSearchQuery(expectedSearch)).Return(_searchQuery);
			Expect.Call(_commandFactory.NewFileCommand(CommandType.PlayShortestTrack, _searchQuery)).Return(_FileCommand);
			ReplayAll();
			
			IList<ICommand> result = _textDiscriminator.Interpret(input);

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			//not checking any results, just the interaction between objects
			VerifyAll();
		}

		[Test]
		[Row("r a test", "a test")]
		[Row("r", "")]
		public void PlaySingleRandomSearch(string input, string expectedSearch)
		{
			IFileCommand _FileCommand = Stub<IFileCommand>();

			Expect.Call(_searchQueryFactory.NewSearchQuery(expectedSearch)).Return(_searchQuery);
			Expect.Call(_commandFactory.NewFileCommand(CommandType.PlayRandomTrack, _searchQuery)).Return(_FileCommand);
			ReplayAll();

			IList<ICommand> result = _textDiscriminator.Interpret(input);

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			VerifyAll();
		}

		[Test]
		[Row("f a test", "a test")]
		[Row("f", "")]
		public void FindFiles(string input, string expectedSearch)
		{
			IFileCommand _FileCommand = Stub<IFileCommand>();

			Expect.Call(_searchQueryFactory.NewSearchQuery(expectedSearch)).Return(_searchQuery);
			Expect.Call(_commandFactory.NewFileCommand(CommandType.ListFiles, _searchQuery)).Return(_FileCommand);
			ReplayAll();

			IList<ICommand> result = _textDiscriminator.Interpret(input);

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			VerifyAll();
		}

		[Test]
		[Row("pl a playlist test", "a playlist test")]
		[Row("pl", "")]
		public void PlaylistSearchWithoutLoopChange(string input, string expectedSearch)
		{
			IFileCommand _FileCommand = Stub<IFileCommand>();
			
			Expect.Call(_searchQueryFactory.NewSearchQuery(expectedSearch)).Return(_searchQuery);
			Expect.Call(_commandFactory.NewFileCommand(CommandType.PlayPlaylist, _searchQuery)).Return(_FileCommand);
			
			ReplayAll();

			IList<ICommand> result = _textDiscriminator.Interpret(input);

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			VerifyAll();
		}
		
		[Test]
		[Row("plc a playlist test", LoopType.Sequential, "a playlist test")]
		[Row("plc", LoopType.Sequential, "")]
		[Row("plr a playlist test", LoopType.Random, "a playlist test")]
		[Row("plr", LoopType.Random, "")]
		[Row("pln a playlist test", LoopType.None, "a playlist test")]
		[Row("pln", LoopType.None, "")]
		public void PlaylistSearchWithLoopChange(string input, LoopType loopType, string expectedSearch)
		{
			IFileCommand _FileCommand = Stub<IFileCommand>();
			ILoopCommand _loopCommand = Stub<ILoopCommand>();

			Expect.Call(_searchQueryFactory.NewSearchQuery(expectedSearch)).Return(_searchQuery);
			Expect.Call(_commandFactory.NewLoopCommand(loopType)).Return(_loopCommand);
			Expect.Call(_commandFactory.NewFileCommand(CommandType.PlayPlaylist, _searchQuery)).Return(_FileCommand);
			
			ReplayAll();
			
			IList<ICommand> result = _textDiscriminator.Interpret(input);

			Assert.IsNotNull(result);
			Assert.AreEqual(2, result.Count);
			VerifyAll();
		}

		[Test]
		[Row("cc a query play test", LoopType.Sequential, "a query play test")]
		[Row("cc", LoopType.Sequential, "")]
		[Row("cr a query play test", LoopType.Random, "a query play test")]
		[Row("cr", LoopType.Random, "")]
		[Row("c a query play test", LoopType.Random, "a query play test")]
		[Row("c", LoopType.Random, "")]
		[Row("cn a query play test", LoopType.None, "a query play test")]
		[Row("cn", LoopType.None, "")]
		public void PlayQueryAsList(string input, LoopType loopType, string expectedSearch)
		{
			IFileCommand _FileCommand = Stub<IFileCommand>();
			ILoopCommand _loopCommand = Stub<ILoopCommand>();

			Expect.Call(_searchQueryFactory.NewSearchQuery(expectedSearch)).Return(_searchQuery);
			Expect.Call(_commandFactory.NewLoopCommand(loopType)).Return(_loopCommand);
			Expect.Call(_commandFactory.NewFileCommand(CommandType.PlayQueryAsList, _searchQuery)).Return(_FileCommand);
			
			ReplayAll();
			
			IList<ICommand> result = _textDiscriminator.Interpret(input);

			Assert.IsNotNull(result);
			Assert.AreEqual(2, result.Count);
			VerifyAll();
		}

		//these next three sets of tests could be refactored into one, but that might obscure the intention of the actions
		[Test]
		[Row("n", TextDiscriminator.DEFAULTTRACKSKIP)]
		[Row("n 4", 4)]
		[Row("n -4", -4)]
		[Row("p", TextDiscriminator.DEFAULTTRACKSKIP * -1)]
		[Row("p 4", -4)]
		[Row("p -4", 4)]
		public void SkipTrack(string input, int expectedMagnitude)
		{
			IChangeStateCommand _changeStateCommand = Stub<IChangeStateCommand>();
			Expect.Call(_commandFactory.NewChangeStateCommand(CommandType.SkipTrack, expectedMagnitude)).Return(_changeStateCommand);
			ReplayAll();

			IList<ICommand> result = _textDiscriminator.Interpret(input);

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			VerifyAll();
		}

		[Test]
		[Row("g 4", 4)]
		[Row("g -4", -4)]
		[Row("g 0", 0)]
		public void SkipToTrack(string input, int expectedMagnitude)
		{
			IChangeStateCommand _changeStateCommand = Stub<IChangeStateCommand>();
			Expect.Call(_commandFactory.NewChangeStateCommand(CommandType.SkipToTrack, expectedMagnitude)).Return(
				_changeStateCommand);
			ReplayAll();

			IList<ICommand> result = _textDiscriminator.Interpret(input);

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			VerifyAll();
		}

		[Test]
		[Row("+", 1)]
		[Row("++++", 4)]
		[Row("-", -1)]
		[Row("----", -4)]
		[Row("+ abc", 1)]
		[Row("+-+- ", 0)]
		[Row("+-+a- ", 1)]
		public void VolumeChange(string input, int expectedMagnitude)
		{
			IChangeStateCommand _changeStateCommand = Stub<IChangeStateCommand>();
			Expect.Call(_commandFactory.NewChangeStateCommand(CommandType.Volume, expectedMagnitude)).Return(_changeStateCommand);
			ReplayAll();

			IList<ICommand> result = _textDiscriminator.Interpret(input);

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			VerifyAll();
		}

		[Test]
		[Row("s", TextDiscriminator.DEFAULTTIMESKIP)]
		[Row("s 20", 20)]
		[Row("s -20", -20)]
		[Row("s asada", TextDiscriminator.DEFAULTTIMESKIP)]
		[Row("s 1.7", TextDiscriminator.DEFAULTTIMESKIP)]
		public void PositionInTrackChange(string input, int expectedMagnitude)
		{
			IChangeStateCommand _changeStateCommand = Stub<IChangeStateCommand>();
			Expect.Call(_commandFactory.NewChangeStateCommand(CommandType.SkipTime, expectedMagnitude)).Return(_changeStateCommand);
			ReplayAll();

			IList<ICommand> result = _textDiscriminator.Interpret(input);

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			VerifyAll();
		}
		
		[Test]
		[Row("ln", LoopType.None)]
		[Row("ls", LoopType.Sequential)]
		[Row("lc", LoopType.Sequential)]
		[Row("l", LoopType.Sequential)]
		[Row("lr", LoopType.Random)]
		public void LoopChange(string input, LoopType loopType)
		{
			ILoopCommand _loopCommand = Stub<ILoopCommand>();
			Expect.Call(_commandFactory.NewLoopCommand(loopType)).Return(_loopCommand);
			ReplayAll();

			IList<ICommand> result = _textDiscriminator.Interpret(input);

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			VerifyAll();
		}
		
		[Test]
		[Row(null, CommandType.Ignore)]
		[Row("", CommandType.Ignore)]
		[Row(" ", CommandType.Ignore)]
		[Row("pp", CommandType.PlayPause)]
		[Row("pp sdfsdf", CommandType.PlayPause)]
		[Row("x", CommandType.Stop)]
		[Row("x sdfsdf", CommandType.Stop)]
		[Row("xx", CommandType.Exit)]
		[Row("xx sdfsdf", CommandType.Exit)]
		[Row("q", CommandType.Exit)]
		[Row("q sdfsdf", CommandType.Exit)]
		[Row("re", CommandType.Repeat)]
		[Row("re sdfsdf", CommandType.Repeat)]
		[Row("h", CommandType.ListHistory)]
		[Row("h sdfsdf", CommandType.ListHistory)]
		[Row("i", CommandType.CurrentTrackDetails)]
		[Row("i sdfsdf", CommandType.CurrentTrackDetails)]
		[Row("--help", CommandType.Help)]
		[Row("--help sdfsdf", CommandType.Help)]
		[Row("/?", CommandType.Help)]
		[Row("?", CommandType.Help)]
		public void CommandsWithNoProperties(string input, CommandType expectedCommandType)
		{
			ICommand _regularCommand = Stub<ICommand>();
			Expect.Call(_commandFactory.NewCommand(expectedCommandType)).Return(_regularCommand);
			ReplayAll();

			IList<ICommand> result = _textDiscriminator.Interpret(input);

			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			VerifyAll();
		}
	}
}