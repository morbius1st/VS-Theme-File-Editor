using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace UtilityLibrary
{
	public class CsXmlUtilities
	{
		// scan an xml file for a specific element and return its value
		public static string ScanXmlForElementValue(string PathAndFile, string elementName,
			int index = 0)
		{
			if (!File.Exists(PathAndFile))
			{
				return null;
			}

			int idx = 0;


			using (XmlReader reader = XmlReader.Create(PathAndFile))
			{
				while (reader.Read())
				{
					if (reader.IsStartElement(elementName))
					{
						if (idx++ != index) continue;

						return reader.ReadString();
					}
				}
			}

			return "";
		}

		public static int CountXmlElements(string PathAndFile, string elementName)
		{
			if (!File.Exists(PathAndFile))
			{
				return -1;
			}

			int idx = 0;

			using (XmlReader reader = XmlReader.Create(PathAndFile))
			{
				while (reader.Read())
				{
					if (reader.IsStartElement(elementName)) idx++;
				}
			}

			return idx;
		}

		public static bool WriteXmlFile<Tdata> (string filePath, Tdata data)
		{
			DataContractSerializer ds = new DataContractSerializer(typeof(Tdata));

			XmlWriterSettings xs = new XmlWriterSettings() { Indent = true };

			try
			{
				using (XmlWriter xw = XmlWriter.Create(filePath, xs))
				{
					ds.WriteObject(xw, data);
				}
			}
			catch
			{
				return false;
			}

			return true;
		}

		public static bool ReadXmlFile<Tdata>(string filePath, out Tdata data)
			where Tdata : new()
		{
			DataContractSerializer ds = new DataContractSerializer(typeof(Tdata));

			data = new Tdata();

			try
			{
				using (FileStream fs = new FileStream(filePath, FileMode.Open))
				{
					data = (Tdata) ds.ReadObject(fs);
				}
			}
			catch
			{
				return false;
			}

			return true;
		}
	}
}