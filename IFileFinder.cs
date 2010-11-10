using System.Collections.Generic;

namespace m
{
	public interface IFileFinder
	{
		IList<string> FindFiles(ISearchQuery searchQuery, FileTypes fileType, FileListSort fileListSort);
		IList<string> FindFiles(ISearchQuery searchQuery, IList<FileTypes> fileTypes, FileListSort fileListSort);
		IList<string> FindFiles(ISearchQuery searchQuery, FileTypes fileType);
		IList<string> FindFiles(ISearchQuery searchQuery, IList<FileTypes> fileTypes);
	}

	public enum FileTypes
	{
		MP3,
		M3U,
		PLS,
		TXT,
		ALL
	}

	public enum FileListSort
	{
		None,
		Random,
		SmallestFirst,
		LargestFirst
	}
}