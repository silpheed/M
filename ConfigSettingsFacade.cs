using System.Collections.Generic;
using System.Configuration;

namespace m
{
	[CoverageExclude("Facade for config file operations.")]
	public class ConfigSettingsFacade : IConfigSettingsFacade
	{
		public string this[string key]
		{
			get { return ConfigurationManager.AppSettings[key]; }
		}

		public string this[int i]
		{
			get { return ConfigurationManager.AppSettings[i]; }
		}

		public IList<string> MusicDirectories
		{
			get { 
				
				return ConfigurationManager.GetSection("m") as IList<string>; }
		}
	}
}
