using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace m
{
	public class mConfig : IConfigurationSectionHandler
	{
		public object Create(object parent, object configContext, XmlNode section)
		{
			IList<string> result = new List<string>();
			
					
			foreach (XmlNode dirNode in section.SelectNodes("//MusicDirectory"))
				result.Add(dirNode.SelectSingleNode("text()").Value);

			return result;
		}
	}
}