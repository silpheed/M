using MbUnit.Framework;

namespace m.tests
{
	[TestFixture]
	public class SearchQueryFixture
	{
		private ISearchQuery _searchQuery;

		[Test]
		[Row(null)]
		[Row("")]
		[Row(" ")]
		public void EmptyNullInput(string input)
		{
			_searchQuery = new SearchQuery(input);
			Assert.AreEqual(0, _searchQuery.WantedAtoms.Count);
			Assert.AreEqual(0, _searchQuery.UnwantedAtoms.Count);
			//Assert.AreEqual(input, _searchQuery.OriginalInput);
		}

		[Test]
		public void OneWantedNoQuotes()
		{
			_searchQuery = new SearchQuery("cat");
			Assert.AreEqual(1, _searchQuery.WantedAtoms.Count);
			Assert.AreEqual("cat", _searchQuery.WantedAtoms[0]);
			Assert.AreEqual(0, _searchQuery.UnwantedAtoms.Count);
		}

		[Test]
		public void MultipleWantedNoQuotes()
		{
			_searchQuery = new SearchQuery("cat dog goat");
			Assert.AreEqual(3, _searchQuery.WantedAtoms.Count);
			Assert.AreEqual("cat", _searchQuery.WantedAtoms[0]);
			Assert.AreEqual("dog", _searchQuery.WantedAtoms[1]);
			Assert.AreEqual("goat", _searchQuery.WantedAtoms[2]);
			Assert.AreEqual(0, _searchQuery.UnwantedAtoms.Count);
		}

		[Test]
		public void OneWantedWithQuotes()
		{
			_searchQuery = new SearchQuery("\"cat\"");
			Assert.AreEqual(1, _searchQuery.WantedAtoms.Count);
			Assert.AreEqual("cat", _searchQuery.WantedAtoms[0]);
			Assert.AreEqual(0, _searchQuery.UnwantedAtoms.Count);
		}

		[Test]
		public void MultipleWantedWithQuotes()
		{
			_searchQuery = new SearchQuery("cat \"dog goat\" horse \"flea\" snake");
			Assert.AreEqual(5, _searchQuery.WantedAtoms.Count);
			Assert.AreEqual("cat", _searchQuery.WantedAtoms[0]);
			Assert.AreEqual("dog goat", _searchQuery.WantedAtoms[1]);
			Assert.AreEqual("horse", _searchQuery.WantedAtoms[2]);
			Assert.AreEqual("flea", _searchQuery.WantedAtoms[3]);
			Assert.AreEqual("snake", _searchQuery.WantedAtoms[4]);
			Assert.AreEqual(0, _searchQuery.UnwantedAtoms.Count);
		}

		[Test]
		public void OneUnwantedNoQuotes()
		{
			_searchQuery = new SearchQuery("-cat");
			Assert.AreEqual(1, _searchQuery.UnwantedAtoms.Count);
			Assert.AreEqual("cat", _searchQuery.UnwantedAtoms[0]);
			Assert.AreEqual(0, _searchQuery.WantedAtoms.Count);
		}

		[Test]
		public void MultipleUnwantedNoQuotes()
		{
			_searchQuery = new SearchQuery("-cat -dog -goat");
			Assert.AreEqual(3, _searchQuery.UnwantedAtoms.Count);
			Assert.AreEqual("cat", _searchQuery.UnwantedAtoms[0]);
			Assert.AreEqual("dog", _searchQuery.UnwantedAtoms[1]);
			Assert.AreEqual("goat", _searchQuery.UnwantedAtoms[2]);
			Assert.AreEqual(0, _searchQuery.WantedAtoms.Count);
		}

		[Test]
		public void OneUnwantedWithQuotes()
		{
			_searchQuery = new SearchQuery("-\"cat\"");
			Assert.AreEqual(1, _searchQuery.UnwantedAtoms.Count);
			Assert.AreEqual("cat", _searchQuery.UnwantedAtoms[0]);
			Assert.AreEqual(0, _searchQuery.WantedAtoms.Count);
		}

		[Test]
		public void MultipleUnwantedWithQuotes()
		{
			_searchQuery = new SearchQuery("-cat -\"dog goat\" -horse -\"flea\" -snake");
			Assert.AreEqual(5, _searchQuery.UnwantedAtoms.Count);
			Assert.AreEqual("cat", _searchQuery.UnwantedAtoms[0]);
			Assert.AreEqual("dog goat", _searchQuery.UnwantedAtoms[1]);
			Assert.AreEqual("horse", _searchQuery.UnwantedAtoms[2]);
			Assert.AreEqual("flea", _searchQuery.UnwantedAtoms[3]);
			Assert.AreEqual("snake", _searchQuery.UnwantedAtoms[4]);
			Assert.AreEqual(0, _searchQuery.WantedAtoms.Count);
		}

		[Test]
		public void MixtureOfWantedUnwantedAndQuotes()
		{
			_searchQuery = new SearchQuery("cat -\"dog goat\" -horse \"flea snake\"");
			Assert.AreEqual(2, _searchQuery.UnwantedAtoms.Count);
			Assert.AreEqual(2, _searchQuery.WantedAtoms.Count);
			Assert.AreEqual("cat", _searchQuery.WantedAtoms[0]);
			Assert.AreEqual("flea snake", _searchQuery.WantedAtoms[1]);
			Assert.AreEqual("dog goat", _searchQuery.UnwantedAtoms[0]);
			Assert.AreEqual("horse", _searchQuery.UnwantedAtoms[1]);
		}

		[Test]
		public void MinusInQuotesTreatedLiterally()
		{
			_searchQuery = new SearchQuery("-cat \"dog -goat\" -horse -\"flea\" -snake");
			Assert.AreEqual(4, _searchQuery.UnwantedAtoms.Count);
			Assert.AreEqual(1, _searchQuery.WantedAtoms.Count);
			Assert.AreEqual("dog -goat", _searchQuery.WantedAtoms[0]);
			Assert.AreEqual("cat", _searchQuery.UnwantedAtoms[0]);
			Assert.AreEqual("horse", _searchQuery.UnwantedAtoms[1]);
			Assert.AreEqual("flea", _searchQuery.UnwantedAtoms[2]);
			Assert.AreEqual("snake", _searchQuery.UnwantedAtoms[3]);
		}

		[Test]
		public void SpaceAfterOpenQuoteTreatedLiterally()
		{
			_searchQuery = new SearchQuery("\" dog goat\"");
			Assert.AreEqual(0, _searchQuery.UnwantedAtoms.Count);
			Assert.AreEqual(1, _searchQuery.WantedAtoms.Count);
			Assert.AreEqual(" dog goat", _searchQuery.WantedAtoms[0]);
		}

		[Test]
		public void MinusPreceededByLetterInWantedTreatedLiterally()
		{
			_searchQuery = new SearchQuery("cat-dog");
			Assert.AreEqual(0, _searchQuery.UnwantedAtoms.Count);
			Assert.AreEqual(1, _searchQuery.WantedAtoms.Count);
			Assert.AreEqual("cat-dog", _searchQuery.WantedAtoms[0]);
		}

		[Test]
		public void MinusPreceededByLetterInUnwantedTreatedLiterally()
		{
			_searchQuery = new SearchQuery("-cat-dog");
			Assert.AreEqual(1, _searchQuery.UnwantedAtoms.Count);
			Assert.AreEqual(0, _searchQuery.WantedAtoms.Count);
			Assert.AreEqual("cat-dog", _searchQuery.UnwantedAtoms[0]);
		}

		[Test]
		public void UnclosedQuoteReachingEndOfLineTreatedAsClosed()
		{
			_searchQuery = new SearchQuery("cat \"dog horse");
			Assert.AreEqual(0, _searchQuery.UnwantedAtoms.Count);
			Assert.AreEqual(2, _searchQuery.WantedAtoms.Count);
			Assert.AreEqual("cat", _searchQuery.WantedAtoms[0]);
			Assert.AreEqual("dog horse", _searchQuery.WantedAtoms[1]);
		}

		[Test]
		public void TwoMinusesTreatedAsUnwantedWithMinus()
		{
			_searchQuery = new SearchQuery("cat --dog horse");
			Assert.AreEqual(1, _searchQuery.UnwantedAtoms.Count);
			Assert.AreEqual(2, _searchQuery.WantedAtoms.Count);
			Assert.AreEqual("-dog", _searchQuery.UnwantedAtoms[0]);
			Assert.AreEqual("cat", _searchQuery.WantedAtoms[0]);
			Assert.AreEqual("horse", _searchQuery.WantedAtoms[1]);
		}

		[Test]
		public void IsolatedMinusIgnored()
		{
			_searchQuery = new SearchQuery("cat - horse");
			Assert.AreEqual(0, _searchQuery.UnwantedAtoms.Count);
			Assert.AreEqual(2, _searchQuery.WantedAtoms.Count);
			Assert.AreEqual("cat", _searchQuery.WantedAtoms[0]);
			Assert.AreEqual("horse", _searchQuery.WantedAtoms[1]);
		}

		[Test]
		[Row("\"\"")]
		[Row("-\"\"")]
		public void EmptyQuotesIgnored(string input)
		{
			_searchQuery = new SearchQuery("cat " + input + " horse");
			Assert.AreEqual(0, _searchQuery.UnwantedAtoms.Count);
			Assert.AreEqual(2, _searchQuery.WantedAtoms.Count);
			Assert.AreEqual("cat", _searchQuery.WantedAtoms[0]);
			Assert.AreEqual("horse", _searchQuery.WantedAtoms[1]);
		}

		/*
		//implemented Equals, then decided that it wasn't worth it
		[Test]
		public void EqualsTest()
		{
			SearchQuery sq1 = new SearchQuery("a");
			SearchQuery sq2 = new SearchQuery("a");
			SearchQuery sq3 = sq1;
			Assert.IsTrue(sq1.Equals(sq2));
			Assert.IsTrue(sq1.Equals(sq3));
			Assert.IsTrue(sq1.Equals(sq3 as object));
			sq1 = new SearchQuery("-a");
			sq2 = new SearchQuery("-a");
			Assert.IsTrue(sq1.Equals(sq2));
			Assert.IsTrue(sq1 == sq2);
		}

		[Test]
		public void NotEqualsTest()
		{
			SearchQuery sq1 = new SearchQuery("a");
			SearchQuery sq2 = new SearchQuery("-a");
			SearchQuery sq3 = sq2;
			SearchQuery sq4 = new SearchQuery("a -b");
			SearchQuery sq5 = new SearchQuery("-a b");
			SearchQuery sq6 = new SearchQuery("-b b");
			Assert.IsFalse(sq1.Equals(sq2));
			Assert.IsFalse(sq1.Equals(sq3));
			Assert.IsFalse(sq4.Equals(sq5));
			Assert.IsFalse(sq5.Equals(sq6));
			Assert.IsTrue(sq4 != sq5);
			Assert.IsFalse(sq1.Equals(null));
		}

		[Test]
		public void HashCodeIsUnique()
		{
			SearchQuery sq1 = new SearchQuery("a");
			SearchQuery sq2 = new SearchQuery("a");
			Assert.IsTrue(sq1.GetHashCode() != sq2.GetHashCode());
		}
		*/
	}
}