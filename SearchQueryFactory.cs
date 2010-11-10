namespace m
{
	public class SearchQueryFactory : ISearchQueryFactory
	{
		public ISearchQuery NewSearchQuery(string rawSearchString)
		{
			return new SearchQuery(rawSearchString);
		}
	}
}