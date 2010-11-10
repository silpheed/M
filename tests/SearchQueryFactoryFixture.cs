using MbUnit.Framework;

namespace m.tests
{
	[TestFixture]
	public class SearchQueryFactoryFixture
	{
		private ISearchQueryFactory searchQueryFactory;

		[SetUp]
		public void SetUp()
		{
			searchQueryFactory = new SearchQueryFactory();
		}

		[Test]
		public void ObjectCreatedIsSameAsAConstructedObject()
		{
			ISearchQuery sqFromFactory = searchQueryFactory.NewSearchQuery("");
			ISearchQuery sqConstructedDirectly = new SearchQuery("");

			//not worth implementing SearchQuery.Equals() just for testing
			Assert.IsNotNull(sqFromFactory);
			Assert.AreEqual(typeof(SearchQuery), sqFromFactory.GetType());
			Assert.AreEqual(sqConstructedDirectly.WantedAtoms.Count, sqFromFactory.WantedAtoms.Count);
			Assert.AreEqual(sqConstructedDirectly.UnwantedAtoms.Count, sqFromFactory.UnwantedAtoms.Count);

			for (int i = 0; i < sqConstructedDirectly.WantedAtoms.Count; i++)
				Assert.AreEqual(sqConstructedDirectly.WantedAtoms[i],sqFromFactory.WantedAtoms[i]);

			for (int i = 0; i < sqConstructedDirectly.UnwantedAtoms.Count; i++)
				Assert.AreEqual(sqConstructedDirectly.UnwantedAtoms[i], sqFromFactory.UnwantedAtoms[i]);
		}
	
		[Test]
		public void NullStillCreatesObject()
		{
			ISearchQuery sq = searchQueryFactory.NewSearchQuery(null);
			Assert.IsNotNull(sq);
		}
	}
}
