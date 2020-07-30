using Facebook.Unity.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Facebook.Unity.Editor
{
	internal class PListParser
	{
		private const string LSApplicationQueriesSchemesKey = "LSApplicationQueriesSchemes";

		private const string CFBundleURLTypesKey = "CFBundleURLTypes";

		private const string CFBundleURLSchemesKey = "CFBundleURLSchemes";

		private const string CFBundleURLName = "CFBundleURLName";

		private const string FacebookCFBundleURLName = "facebook-unity-sdk";

		private const string FacebookAppIDKey = "FacebookAppID";

		private const string FacebookAppIDPrefix = "fb";

		private const string AutoLogAppEventsEnabled = "FacebookAutoLogAppEventsEnabled";

		private const string AdvertiserIDCollectionEnabled = "FacebookAdvertiserIDCollectionEnabled";

		private static readonly IList<object> FacebookLSApplicationQueriesSchemes = new List<object>
		{
			"fbapi",
			"fb-messenger-api",
			"fbauth2",
			"fbshareextension"
		};

		private static readonly PListDict FacebookUrlSchemes = new PListDict
		{
			{
				"CFBundleURLName",
				"facebook-unity-sdk"
			}
		};

        private static readonly PListDict DescriptionSchemes = new PListDict
        {
            {
                "NSCameraUsageDescription",
                "是否允许此App使用你的相机？"
            },
            {
                "NSMicrophoneUsageDescription",
                "是否允许此App使用你的麦克风？"
            },
            {
                "NSCalendarsUsageDescription",
                "是否允许此App使用日历？"
            },
            {
                "NSPhotoLibraryUsageDescription",
                "是否允许此App访问你的媒体资料库？"
            },
            {
                "NSBluetoothPeripheralUsageDescription",
                "是否许允此App使用蓝牙？"
            },
            {
                "NSLocationUsageDescription",
                "是否许允此App访问位置？"
            },
            {
                "NSLocationWhenInUseUsageDescription",
                "是否许允此App使用期间访问位置？"
            },
            {
                "NSLocationAlwaysUsageDescription",
                "是否许允此App始终访问位置？"
            },
            {
                "ITSAppUsesNonExemptEncryption",
                false
            }
        };

        private string filePath;

		public PListDict XMLDict
		{
			get;
			set;
		}

		public PListParser(string fullPath)
		{
			filePath = fullPath;
			XmlReaderSettings settings = new XmlReaderSettings
			{
				ProhibitDtd = false
			};
			XmlReader xmlReader = XmlReader.Create(filePath, settings);
			XElement dict = XDocument.Load(xmlReader).Element("plist").Element("dict");
			XMLDict = new PListDict(dict);
			xmlReader.Close();
		}

		public void UpdateFBSettings(string appID, string urlSuffix, ICollection<string> appLinkSchemes)
		{
#if UNITY_IOS
			XMLDict["FacebookAppID"] = appID;
			XMLDict["FacebookAutoLogAppEventsEnabled"] = FacebookSettings.AutoLogAppEventsEnabled;
			XMLDict["FacebookAdvertiserIDCollectionEnabled"] = FacebookSettings.AdvertiserIDCollectionEnabled;
			SetCFBundleURLSchemes(XMLDict, appID, urlSuffix, appLinkSchemes);
			WhitelistFacebookApps(XMLDict);
#endif
		}
        public void UpdateInfolist()
        {
            if (PListParser.ContainsKeyWithValueType(this.XMLDict, "UIApplicationExitsOnSuspend", typeof(bool)))
            {
                this.XMLDict.Remove("UIApplicationExitsOnSuspend");
            }
            PListDict desPlist = new PListDict(DescriptionSchemes);
            foreach (var item in desPlist)
            {
                if (!this.XMLDict.ContainsKey(item.Key))
                {
                    this.XMLDict.Add(item.Key, item.Value);
                }
            }
        }

        public void WriteToFile()
		{
			string publicId = "-//Apple//DTD PLIST 1.0//EN";
			string systemId = "http://www.apple.com/DTDs/PropertyList-1.0.dtd";
			string internalSubset = null;
			XDeclaration declaration = new XDeclaration("1.0", Encoding.UTF8.EncodingName, null);
			XDocumentType docType = new XDocumentType("plist", publicId, systemId, internalSubset);
			XMLDict.Save(filePath, declaration, docType);
		}

		private static void WhitelistFacebookApps(PListDict plistDict)
		{
			if (!ContainsKeyWithValueType(plistDict, "LSApplicationQueriesSchemes", typeof(IList<object>)))
			{
				plistDict["LSApplicationQueriesSchemes"] = FacebookLSApplicationQueriesSchemes;
				return;
			}
			IList<object> list = (IList<object>)plistDict["LSApplicationQueriesSchemes"];
			foreach (object facebookLSApplicationQueriesScheme in FacebookLSApplicationQueriesSchemes)
			{
				if (!list.Contains(facebookLSApplicationQueriesScheme))
				{
					list.Add(facebookLSApplicationQueriesScheme);
				}
			}
		}

		private static void SetCFBundleURLSchemes(PListDict plistDict, string appID, string urlSuffix, ICollection<string> appLinkSchemes)
		{
			IList<object> plistSchemes = (IList<object>)(ContainsKeyWithValueType(plistDict, "CFBundleURLTypes", typeof(IList<object>)) ? ((IList<object>)plistDict["CFBundleURLTypes"]) : (plistDict["CFBundleURLTypes"] = new List<object>()));
			List<object> schemesCollection = (List<object>)(GetFacebookUrlSchemes(plistSchemes)["CFBundleURLSchemes"] = new List<object>());
			AddAppID(schemesCollection, appID, urlSuffix);
			AddAppLinkSchemes(schemesCollection, appLinkSchemes);
		}

		private static PListDict GetFacebookUrlSchemes(ICollection<object> plistSchemes)
		{
			foreach (object plistScheme in plistSchemes)
			{
				PListDict pListDict = plistScheme as PListDict;
				if (pListDict != null && pListDict.TryGetValue("CFBundleURLName", out string value) && value == "facebook-unity-sdk")
				{
					return pListDict;
				}
			}
			PListDict pListDict2 = new PListDict(FacebookUrlSchemes);
			plistSchemes.Add(pListDict2);
			return pListDict2;
		}

		private static void AddAppID(ICollection<object> schemesCollection, string appID, string urlSuffix)
		{
			string text = "fb" + appID;
			if (!string.IsNullOrEmpty(urlSuffix))
			{
				text += urlSuffix;
			}
			schemesCollection.Add(text);
		}

		private static void AddAppLinkSchemes(ICollection<object> schemesCollection, ICollection<string> appLinkSchemes)
		{
			foreach (string appLinkScheme in appLinkSchemes)
			{
				schemesCollection.Add(appLinkScheme);
			}
		}

		private static bool ContainsKeyWithValueType(IDictionary<string, object> dictionary, string key, Type type)
		{
			if (dictionary.ContainsKey(key) && type.IsAssignableFrom(dictionary[key].GetType()))
			{
				return true;
			}
			return false;
		}
	}
}
