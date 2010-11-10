using System;
using System.IO;

namespace m
{
	[CoverageExclude("Facade for file system operations.")]
	public class FileSystemFacade : IFileSystemFacade
	{
		public void CreateDirectory(string directory)
		{
			try {
				Directory.CreateDirectory(directory);
			}
			catch {}
		}

		public void DeleteDirectory(string directory)
		{
			Directory.Delete(directory, true);
		}

		public bool DirectoryExists(string directory)
		{
			return Directory.Exists(directory);
		}

		public void DeleteFile(string path)
		{
			try {
				File.SetAttributes(path, FileAttributes.Normal);
				File.Delete(path);
			}
			catch {}
		}

		public bool FileExists(string path)
		{
			return File.Exists(path);
		}

		public void Copy(string source, string dest)
		{
			File.Copy(source, dest, true);
		}

		public void WriteAllText(string path, string contents)
		{
			File.WriteAllText(path, contents);
		}

		public string ReadAllText(string path)
		{
			return File.ReadAllText(path);
		}

		public string[] ReadAllLines(string path)
		{
			return File.ReadAllLines(path);
		}

		public FileStream OpenRead(string path)
		{
			return File.OpenRead(path);
		}

		public FileStream CreateFile(string path)
		{
			return File.Create(path);
		}

		public DateTime GetLastWriteTime(string path)
		{
			return File.GetLastWriteTime(path);
		}

		public string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
		{
			return Directory.GetFiles(path, searchPattern, searchOption);
		}

		public string[] GetFilesInCurrentDirectoryOnly(string path)
		{
			return Directory.GetFiles(path);
		}

		public string[] GetDirectoriesInCurrentDirectoryOnly(string path)
		{
			return Directory.GetDirectories(path);
		}

		public string GetTempPath()
		{
			return Path.GetTempPath();
		}

	}
}
