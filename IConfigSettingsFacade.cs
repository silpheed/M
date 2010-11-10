using System.Collections.Generic;

namespace m
{
	public interface IConfigSettingsFacade
	{
		string this[string key] { get; }
		string this[int i] { get; }

		IList<string> MusicDirectories
		{
			get;
		}
	}
}