using System.Collections.Generic;

namespace m
{
	public interface IPlaylistReader
	{
		IList<string> GetTracklist(string fileName);
		void ConvertListOfRelativeFilesToAbsolute(IList<string> list, string rootFileOrDirectory);
	}
}