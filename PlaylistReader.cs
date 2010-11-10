using System;
using System.Collections.Generic;
using System.IO;

namespace m
{
	public class PlaylistReader : IPlaylistReader
	{
		private IFileSystemFacade fileSystem;

		public PlaylistReader(IFileSystemFacade fileSystem)
		{
			this.fileSystem = fileSystem;
		}

		public IList<string> GetTracklist(string fileName)
		{
			if (null == fileName)
				return null;
			if (string.Empty == fileName)
				return new List<string>();

			string[] linesOfFile;
			try {
				linesOfFile = fileSystem.ReadAllLines(fileName);
			}
			catch {
				return null;
			}

			FileTypes listType = GetListType(linesOfFile);
			IList<string> list;
			if (listType == FileTypes.M3U)
				list = GetTracksFromM3u(linesOfFile);
			else if (listType == FileTypes.PLS)
				list = GetTracksFromPls(linesOfFile);
			else
				list = GetTracksFromTxt(linesOfFile);

			ConvertListOfRelativeFilesToAbsolute(list, fileName);

			RemoveNonExistantTracks(list);

			return list;
		}

		public void ConvertListOfRelativeFilesToAbsolute(IList<string> list, string rootFileOrDirectory)
		{
			for (int i = 0; i < list.Count; i++) {
				try {
					if (!Path.IsPathRooted(list[i]))
						list[i] = Path.Combine(Path.GetDirectoryName(rootFileOrDirectory), list[i]);
					if (!Path.IsPathRooted(list[i])) {
						list.RemoveAt(i);
						i--;
					}
				}
				catch {
					//if the file can not be made absolute, remove it. it is of no use to us.
					list.RemoveAt(i);
					i--;
				}
			}
		}

		private void RemoveNonExistantTracks(IList<string> list)
		{
			for (int i = 0;i < list.Count;i++) {
				if (!fileSystem.FileExists(list[i])) {
					list.RemoveAt(i);
					i--;
				}
			}
		}

		private IList<string> GetTracksFromTxt(string[] linesOfFile)
		{
			IList<string> temp = new List<string>();
			foreach (string line in linesOfFile)
				if (line.Trim() != string.Empty)
					temp.Add(line);
			return temp;
		}

		private IList<string> GetTracksFromM3u(string[] linesOfFile)
		{
			IList<string> temp = new List<string>();
			foreach (string line in linesOfFile)
				if ((line.Trim() != string.Empty) && (!line.StartsWith("#EXT", StringComparison.CurrentCultureIgnoreCase)))
					temp.Add(line);
			return temp;
		}

		private IList<string> GetTracksFromPls(string[] linesOfFile)
		{
			IDictionary<int, string> fileWithPos = new Dictionary<int, string>();
			foreach (string line in linesOfFile)
				if ((line.Trim().StartsWith("File", StringComparison.CurrentCultureIgnoreCase)) && (line.Contains("="))) {
					try {
						int index = Convert.ToInt32(line.Trim().ToLower().Split('=')[0].Replace("file", ""));
						fileWithPos[index] = line.Substring(line.IndexOf('=') + 1);
					}
					catch {}
				}
			//assigning indexes based on what the .pls file tells us could leave empty spots, so remove them.
			IList<string> temp = new List<string>();
			int offset = 0;
			for (int i = 0;i < fileWithPos.Count;i++) {
				if (fileWithPos.ContainsKey(i + offset))
					temp.Add(fileWithPos[i + offset]);
				else {
					offset++;
					i--;
				}
			}
			return temp;
		}

		private FileTypes GetListType(string[] linesOfFile)
		{
			if (linesOfFile[0].StartsWith("#EXTM3U"))
				return FileTypes.M3U;
			if (linesOfFile[0].StartsWith("[playlist]"))
				return FileTypes.PLS;
			return FileTypes.TXT;
		}
	}
}