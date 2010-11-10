using System.Collections.Generic;
using MbUnit.Framework;

namespace m.tests
{
	[TestFixture]
	public class FilenameComparerFixture
	{
		private FilenameComparer fileComparerAsc;
		private FilenameComparer fileComparerDesc;

		[SetUp]
		public void SetUp()
		{
			fileComparerAsc = new FilenameComparer(true);
			fileComparerDesc = new FilenameComparer(false);
		}

		[Test]
		public void UnevenFilenameLengthsAsc()
		{
			List<string> list = new List<string>();
			list.Add(@"C:\z dir\b trackbbb.mp3");
			list.Add(@"C:\a dir\z trackz.mp3");
			list.Add(@"C:\b dir\a trackaaaa.mp3");
			list.Add(@"C:\c dir\c trackcc.mp3");
			list.Sort(fileComparerAsc);
			Assert.AreEqual(@"C:\a dir\z trackz.mp3", list[0]);
			Assert.AreEqual(@"C:\c dir\c trackcc.mp3", list[1]);
			Assert.AreEqual(@"C:\z dir\b trackbbb.mp3", list[2]);
			Assert.AreEqual(@"C:\b dir\a trackaaaa.mp3", list[3]);
		}

		[Test]
		public void EvenFilenameLengthsAsc()
		{
			List<string> list = new List<string>();
			list.Add(@"C:\z dir\b track.mp3");
			list.Add(@"C:\a dir\z track.mp3");
			list.Add(@"C:\b dir\a track.mp3");
			list.Add(@"C:\c dir\c track.mp3");
			list.Sort(fileComparerAsc);
			Assert.AreEqual(@"C:\b dir\a track.mp3", list[0]);
			Assert.AreEqual(@"C:\z dir\b track.mp3", list[1]);
			Assert.AreEqual(@"C:\c dir\c track.mp3", list[2]);
			Assert.AreEqual(@"C:\a dir\z track.mp3", list[3]);
		}

		[Test]
		public void NullWinsAsc()
		{
			List<string> list = new List<string>();
			list.Add(@"C:\z dir\b track.mp3");
			list.Add(@"C:\a dir\z track.mp3");
			list.Add(null);
			list.Add(@"C:\c dir\c track.mp3");
			list.Sort(fileComparerAsc);
			Assert.IsNull(list[0]);
		}

		[Test]
		public void EmptyWinsAsc()
		{
			List<string> list = new List<string>();
			list.Add(@"C:\z dir\b track.mp3");
			list.Add(@"C:\a dir\z track.mp3");
			list.Add(string.Empty);
			list.Add(@"C:\c dir\c track.mp3");
			list.Sort(fileComparerAsc);
			Assert.AreEqual(string.Empty, list[0]);
		}
		
		[Test]
		public void NullBeatsEmptyAsc()
		{
			List<string> list = new List<string>();
			list.Add(@"C:\z dir\b track.mp3");
			list.Add(@"C:\a dir\z track.mp3");
			list.Add(string.Empty);
			list.Add(null);
			list.Sort(fileComparerAsc);
			Assert.IsNull(list[0]);
			Assert.AreEqual(string.Empty, list[1]);
		}

		[Test]
		public void UnevenFilenameLengthsDesc()
		{
			List<string> list = new List<string>();
			list.Add(@"C:\z dir\b trackbbb.mp3");
			list.Add(@"C:\a dir\z trackz.mp3");
			list.Add(@"C:\b dir\a trackaaaa.mp3");
			list.Add(@"C:\c dir\c trackcc.mp3");
			list.Sort(fileComparerDesc);
			Assert.AreEqual(@"C:\b dir\a trackaaaa.mp3", list[0]);
			Assert.AreEqual(@"C:\z dir\b trackbbb.mp3", list[1]);
			Assert.AreEqual(@"C:\c dir\c trackcc.mp3", list[2]);
			Assert.AreEqual(@"C:\a dir\z trackz.mp3", list[3]);
		}

	}
}