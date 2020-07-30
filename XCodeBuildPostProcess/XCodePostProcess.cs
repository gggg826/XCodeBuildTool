using Facebook.Unity.Settings;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Facebook.Unity.Editor
{
	public static class XCodePostProcess
	{
		[PostProcessBuild(100)]
		public static void OnPostProcessBuild(BuildTarget target, string path)
		{
			if (!FacebookSettings.IsValidAppId)
			{
				Debug.LogWarning("You didn't specify a Facebook app ID.  Please add one using the Facebook menu in the main Unity editor.");
			}
			if (target.ToString() == "iOS" || target.ToString() == "iPhone")
			{
				UpdatePlist(path);
				FixupFiles.FixColdStart(path);
				FixupFiles.AddBuildFlag(path);
                
            }
		}

		public static void UpdatePlist(string path)
		{
#if UNITY_IOS
            string appId = FacebookSettings.AppId;
			string fullPath = Path.Combine(path, "Info.plist");
			if (string.IsNullOrEmpty(appId) || appId.Equals("0"))
			{
				Debug.LogError("You didn't specify a Facebook app ID.  Please add one using the Facebook menu in the main Unity editor.");
				return;
			}
			PListParser pListParser = new PListParser(fullPath);
			pListParser.UpdateFBSettings(appId, FacebookSettings.IosURLSuffix, FacebookSettings.AppLinkSchemes[FacebookSettings.SelectedAppIndex].Schemes);
            pListParser.UpdateInfolist();
            pListParser.WriteToFile();
#endif   
        }
	}
}
