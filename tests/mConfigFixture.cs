using System.Collections.Generic;
using System.Xml;
using MbUnit.Framework;

namespace m.tests
{
	[TestFixture]
	public class mConfigFixture : MockingFixture
	{
		private mConfig mConfig;
		private XmlDocument configSection;

		public override void SetUp()
		{
			mConfig = new mConfig();
			configSection = new XmlDocument();
			//an xml declaration is needed to be treated as xml, but the xml section that is passed to mConfig.Create() in 
			//actual use is missing it.
//			XmlDeclaration xmlDeclaration = configSection.CreateXmlDeclaration("1.0", "utf-8", null);
//			configSection.InsertBefore(xmlDeclaration, configSection.DocumentElement);
			XmlElement root = configSection.CreateElement("m"); 
			configSection.AppendChild(root);
			
			XmlElement element1 = configSection.CreateElement("MusicDirectory");
			root.AppendChild(element1);
			element1.AppendChild(configSection.CreateTextNode("Directory One"));
			XmlElement element2 = configSection.CreateElement("MusicDirectory");
			root.AppendChild(element2);
			element2.AppendChild(configSection.CreateTextNode("Directory Two"));
			
			XmlElement element3 = configSection.CreateElement("MusicDirectory");
			root.AppendChild(element3);
			element3.AppendChild(configSection.CreateTextNode("Directory Three"));
			
			XmlElement element4 = configSection.CreateElement("SomeOtherElement");
			root.AppendChild(element4);
			element4.AppendChild(configSection.CreateTextNode("Not a directory"));
			
		}

		[Test]
		public void ExistingValues()
		{
			IList<string> result = mConfig.Create(null, null, configSection) as IList<string>;

			Assert.IsNotNull(result);
			if (result != null) {
				Assert.AreEqual(3, result.Count);
				Assert.AreEqual("Directory One", result[0]);
				Assert.AreEqual("Directory Two", result[1]);
				Assert.AreEqual("Directory Three", result[2]);
			}
		}

		[Test]
		public void EmtpyXmlConfig()
		{
			XmlDocument emptyConfigSection = new XmlDocument();

			IList<string> result = mConfig.Create(null, null, emptyConfigSection) as IList<string>;

			Assert.IsNotNull(result);
			if (result != null)
				Assert.AreEqual(0, result.Count);
		}
		



	}
}