using System.Collections.Generic;
using System.IO;

namespace m
{
	public class FilenameComparer : IComparer<string>
	{
		private readonly bool _asc;

		public FilenameComparer(bool asc)
		{
			_asc = asc;
		}

		public int Compare(string x, string y)
		{
			if (!_asc)
				return ActualCompare(x, y) * -1;
			return ActualCompare(x, y);
		}

		private int ActualCompare(string x, string y)
		{
			if (x == null)
				if (y == null)
					return 0;
				else
					return -1;
			else
			{
				if (y == null)
					return 1;
				else
				{
					int retval = Path.GetFileNameWithoutExtension(x).Length.CompareTo(Path.GetFileNameWithoutExtension(y).Length);
					if (retval != 0)
						return retval;
					else
						return Path.GetFileNameWithoutExtension(x).CompareTo(Path.GetFileNameWithoutExtension(y));
				}
			}
		}
	}
}