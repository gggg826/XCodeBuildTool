using Facebook.Unity.Editor.iOS.Xcode;
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Facebook.Unity.Editor
{
	public class FixupFiles
	{
		private static string didFinishLaunchingWithOptions = "(?x)                                  # Verbose mode\n  (didFinishLaunchingWithOptions.+      # Find this function...\n    (?:.*\\n)+?                          # Match as few lines as possible until...\n    \\s*return\\ )NO(\\;\\n                 #   return NO;\n  \\})                                   # }";

		public static void FixColdStart(string path)
		{
			string fullPath = Path.Combine(path, Path.Combine("Classes", "UnityAppController.mm"));
			string input = Load(fullPath);
			input = Regex.Replace(input, didFinishLaunchingWithOptions, "$1YES$2");
			Save(fullPath, input);
        }

		public static void AddBuildFlag(string path)
		{
			string path2 = Path.Combine(path, Path.Combine("Unity-iPhone.xcodeproj", "project.pbxproj"));
			PBXProject pBXProject = new PBXProject();
			pBXProject.ReadFromString(File.ReadAllText(path2));
			string targetGuid = pBXProject.TargetGuidByName("Unity-iPhone");
			pBXProject.AddBuildProperty(targetGuid, "GCC_PREPROCESSOR_DEFINITIONS", " $(inherited) FBSDKCOCOAPODS=1");
			pBXProject.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-ObjC");
			pBXProject.AddFrameworkToProject(targetGuid, "Accelerate.framework", weak: true);
            pBXProject.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
            pBXProject.AddCapability(targetGuid, PBXCapabilityType.InAppPurchase);
            pBXProject.AddCapability(targetGuid, PBXCapabilityType.GameCenter);
            File.WriteAllText(path2, pBXProject.WriteToString());
        }

		protected static string Load(string fullPath)
		{
			StreamReader streamReader = new FileInfo(fullPath).OpenText();
			string result = streamReader.ReadToEnd();
			streamReader.Close();
			return result;
		}

		protected static void Save(string fullPath, string data)
		{
			StreamWriter streamWriter = new StreamWriter(fullPath, append: false);
			streamWriter.Write(data);
			streamWriter.Close();
		}

		private static int GetUnityVersionNumber()
		{
			string[] array = Application.unityVersion.Split('.');
			int num = 0;
			int num2 = 0;
			try
			{
				if (array != null && array.Length != 0 && array[0] != null)
				{
					num = Convert.ToInt32(array[0]);
				}
				if (array != null && array.Length > 1 && array[1] != null)
				{
					num2 = Convert.ToInt32(array[1]);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("Error parsing Unity version number: " + ex));
			}
			return num * 100 + num2 * 10;
		}
	}
}
