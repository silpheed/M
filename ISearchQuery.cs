using System.Collections.Generic;

namespace m
{
	public interface ISearchQuery
	{
		IList<string> WantedAtoms { get; }
		IList<string> UnwantedAtoms { get; }
	}
}