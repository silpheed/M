using System;
using System.IO;

namespace m
{
	public interface IFileSystemFacade
	{
		void CreateDirectory(string directory);
		void DeleteDirectory(string directory);
		bool DirectoryExists(string directory);
		void DeleteFile(string path);
		bool FileExists(string path);
		void Copy(string source, string dest);
		void WriteAllText(string path, string contents);
		string ReadAllText(string path);
		FileStream OpenRead(string path);
		FileStream CreateFile(string path);
		DateTime GetLastWriteTime(string path);
		string[] GetFiles(string path, string searchPattern, SearchOption searchOption);
		string[] GetFilesInCurrentDirectoryOnly(string path);
		string[] GetDirectoriesInCurrentDirectoryOnly(string path);
		string GetTempPath();
		string[] ReadAllLines(string path);
	}
}