using System;

namespace m
{
	[CoverageExclude("Facade for console reading and writing.")]
	class ConsoleFacade : IConsoleFacade
	{
		public void Write(string value)
		{
			Console.Write(value);
		}

		public void WriteLine(string value)
		{
			Console.WriteLine(value);
		}

		public string ReadLine()
		{
			return Console.ReadLine();
		}
	}
}