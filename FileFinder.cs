using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace m
{
	public class FileFinder : IFileFinder
	{
		private readonly IFileSystemFacade fileSystem;
		private readonly IConfigSettingsFacade configSettings;

		public FileFinder(IFileSystemFacade fileSystem, IConfigSettingsFacade configSettings)
		{
			this.fileSystem = fileSystem;
			this.configSettings = configSettings;
		}

		public IList<string> FindFiles(ISearchQuery searchQuery, FileTypes fileType)
		{
			return FindFiles(searchQuery, new List<FileTypes>(new FileTypes[] { fileType }));
		}

		public IList<string> FindFiles(ISearchQuery searchQuery, IList<FileTypes> fileTypes)
		{
			return FindFiles(searchQuery, fileTypes, FileListSort.None);
		}
		
		public IList<string> FindFiles(ISearchQuery searchQuery, FileTypes fileType, FileListSort fileListSort)
		{
			return FindFiles(searchQuery, new List<FileTypes>(new FileTypes[] { fileType }), fileListSort);
		}
		
		public IList<string> FindFiles(ISearchQuery searchQuery, IList<FileTypes> fileTypes, FileListSort fileListSort)
		{
			List<string> result = new List<string>();
			
			if (configSettings.MusicDirectories.Count == 0)
				return result;
			StringBuilder search = new StringBuilder();

			foreach (string wa in searchQuery.WantedAtoms) {
				search.Append("*");
				search.Append(wa);
			}
			search.Append("*.");

			foreach (FileTypes fileTypeEnum in fileTypes) {
				string fileType;
				if (FileTypes.ALL == fileTypeEnum)
					fileType = "*";
				else
					fileType = fileTypeEnum.ToString();

				fileType = fileType.ToLower();
				
				foreach (string musicDir in configSettings.MusicDirectories) {
					string[] found;
					try {
						//PROBLEM! http://www.mono-project.com/IOMap also man mono
						found = fileSystem.GetFiles(musicDir, search + fileType, SearchOption.AllDirectories);
					}
					catch {
						found = new string[0];
					}
					foreach (string hit in found) {
						//don't add it twice, and discard any extension-greater-than-three-characters microsoft retardedness:
						//http://msdn2.microsoft.com/en-us/library/ms143316(VS.80).aspx
						if ((result.Contains(hit)) ||
							((Path.GetExtension(hit).ToLower() != ("." + fileType).ToLower()) && (FileTypes.ALL != fileTypeEnum)))
						continue;

						//also don't add it if it contains unwanted atoms
						bool add = true;
						foreach (string uwa in searchQuery.UnwantedAtoms)
							if (Regex.IsMatch(Path.GetFileNameWithoutExtension(hit), Regex.Escape(uwa), RegexOptions.IgnoreCase)) {
								add = false;
								break;
							}
						if (add)
							result.Add(hit);
					}
				}
			}
			switch (fileListSort) {
				case FileListSort.SmallestFirst:
					result.Sort(new FilenameComparer(true));
					break;
				case FileListSort.LargestFirst:
					result.Sort(new FilenameComparer(false));
					break;
				case FileListSort.Random:
					Randomise(result);
					break;
			}

			return result;
		}

		[CoverageExclude("This method is random, and so is its code coverage. Can not guarantee the same coverage from test to test")]
		private static void Randomise(IList<string> list)
		{
			Random rng = new Random(DateTime.Now.Millisecond);	
			for (int i = list.Count - 1; i > 0; i--)
			{
				int swapIndex = rng.Next(i + 1);
				if (swapIndex != i)
				{
					string tmp = list[swapIndex];
					list[swapIndex] = list[i];
					list[i] = tmp;
				}
			}
		}

	}
}