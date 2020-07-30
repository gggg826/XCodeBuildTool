using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Facebook.Unity.Editor
{
	public class PListDict : Dictionary<string, object>
	{
		public PListDict()
		{
		}

		public PListDict(PListDict dict)
			: base((IDictionary<string, object>)dict)
		{
		}

		public PListDict(XElement dict)
		{
			Load(dict);
		}

		public void Load(XElement dict)
		{
			IEnumerable<XElement> elements = dict.Elements();
			ParseDictForLoad(this, elements);
		}

		public void Save(string fileName, XDeclaration declaration, XDocumentType docType)
		{
			XElement xElement = new XElement("plist", ParseDictForSave(this));
			xElement.SetAttributeValue("version", "1.0");
			XDocument xDocument = new XDocument(declaration, docType);
			xDocument.Add(xElement);
			xDocument.Save(fileName);
		}

		public XElement ParseValueForSave(object node)
		{
			if (node is string)
			{
				return new XElement("string", node);
			}
			if (node is bool)
			{
				return new XElement(node.ToString().ToLower());
			}
			if (node is int)
			{
				return new XElement("integer", node);
			}
			if (node is float)
			{
				return new XElement("real", node);
			}
			if (node is IList<object>)
			{
				return ParseArrayForSave(node);
			}
			if (node is PListDict)
			{
				return ParseDictForSave((PListDict)node);
			}
			if (node == null)
			{
				return null;
			}
			throw new NotSupportedException("Unexpected type: " + node.GetType().FullName);
		}

		private void ParseDictForLoad(PListDict dict, IEnumerable<XElement> elements)
		{
			for (int i = 0; i < elements.Count(); i += 2)
			{
				XElement xElement = elements.ElementAt(i);
				XElement val = elements.ElementAt(i + 1);
				dict[xElement.Value] = ParseValueForLoad(val);
			}
		}

		private IList<object> ParseArrayForLoad(IEnumerable<XElement> elements)
		{
			List<object> list = new List<object>();
			foreach (XElement element in elements)
			{
				object item = ParseValueForLoad(element);
				list.Add(item);
			}
			return list;
		}

		private object ParseValueForLoad(XElement val)
		{
			switch (val.Name.ToString())
			{
			case "string":
				return val.Value;
			case "integer":
				return int.Parse(val.Value);
			case "real":
				return float.Parse(val.Value);
			case "true":
				return true;
			case "false":
				return false;
			case "dict":
			{
				PListDict pListDict = new PListDict();
				ParseDictForLoad(pListDict, val.Elements());
				return pListDict;
			}
			case "array":
				return ParseArrayForLoad(val.Elements());
			default:
				throw new ArgumentException("Format unsupported, Parser update needed");
			}
		}

		private XElement ParseDictForSave(PListDict dict)
		{
			XElement xElement = new XElement("dict");
			foreach (string key in dict.Keys)
			{
				xElement.Add(new XElement("key", key));
				xElement.Add(ParseValueForSave(dict[key]));
			}
			return xElement;
		}

		private XElement ParseArrayForSave(object node)
		{
			XElement xElement = new XElement("array");
			IList<object> list = (IList<object>)node;
			for (int i = 0; i < list.Count; i++)
			{
				xElement.Add(ParseValueForSave(list[i]));
			}
			return xElement;
		}
	}
}
