namespace m
{
	public interface ISearchQueryFactory
	{
		ISearchQuery NewSearchQuery(string rawSearchString);
	}
}