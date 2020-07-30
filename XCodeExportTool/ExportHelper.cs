using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class ExportHelper
{
    private const string Key_BuildPackageOnScriptReloaded = "BuildPackageOnScriptReloaded";
    private const string Key_XcodeExportPath = "XcodeExportPath";
    private const string ExportBasePath = "D:/XcodeExport/";

    private static ExportXcodeConfiguration m_ExportParam;
    public static void ExportXCodeProject(ExportXcodeConfiguration config)
    {
        m_ExportParam = config;
        EditorPrefs.SetBool(Key_BuildPackageOnScriptReloaded, false);

        string date = DateTime.Now.ToString("yy_MM_dd");
        string exportPath = $"{ExportBasePath}{m_ExportParam.Name}_{date}_{m_ExportParam.ClientVersion}_{m_ExportParam.VersionCode}_{GetSVNRevision()}";
        EditorPrefs.SetString(Key_XcodeExportPath, exportPath);

        DeleteDirectoryContentEx($"{Application.dataPath}/Script/BattleNew/Global");
        DeleteDirectoryContentEx($"{Application.dataPath}/Script/BattleNew/NativeDll");
        DeleteDirectoryContentEx($"{Application.dataPath}/Plugins/iOS/Cardboard");
        ModifyPlayerSettingForiOS(m_ExportParam);
        ModifyGlobalFile(m_ExportParam.EnableGuest);
        ModifyDataDefineCSFile(m_ExportParam);
    }

    private static void ModifyPlayerSettingForiOS(ExportXcodeConfiguration config)
    {
        //宏定义设置
        //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "ISSDK;IRIS");

        PlayerSettings.companyName = config.CompanyName;
        PlayerSettings.productName = config.ProductName;
        PlayerSettings.applicationIdentifier = config.ApplicationIdentifier;
        PlayerSettings.bundleVersion = config.ClientVersion;
        PlayerSettings.iOS.buildNumber = config.VersionCode;
        Texture2D splash_tx = config.Splash;
        PlayerSettings.SplashScreen.show = false;
        PlayerSettings.iOS.SetiPadLaunchScreenType(iOSLaunchScreenType.Default);
        PlayerSettings.iOS.SetiPhoneLaunchScreenType(iOSLaunchScreenType.Default);

        PlayerSettings.virtualRealitySplashScreen = splash_tx;

        //Selection.activeObject = Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings");
        SerializedObject serializeObj = new SerializedObject(Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings"));
        SerializedProperty iOSSplashTexture = serializeObj.FindProperty("iPhoneSplashScreen");
        iOSSplashTexture.objectReferenceValue = splash_tx;
        serializeObj.ApplyModifiedProperties();
    }

    private static void ModifyGlobalFile(bool enableGuest)
    {
        string fileFullPath = $"{Application.dataPath}/Script/Data/Global.cs";

        string CSFileStr;
        using (StreamReader sr = new StreamReader(fileFullPath))
        {
            CSFileStr = sr.ReadToEnd();
        }

        string source, target, flag;

        //是否开启游客登录
        flag = "public static bool IsOpenGuest = ";
        source = $"{flag}[\\w]*;";
        target = $"{flag}{enableGuest.ToString().ToLower()};";
        RegexReplace(source, target, fileFullPath);

        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset(fileFullPath);
    }

    /// <summary>
    /// 修改Datadefine中的打包参数
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="outline"></param>
    /// <param name="language"></param>
    private static void ModifyDataDefineCSFile(ExportXcodeConfiguration config)
    {
        string fileFullPath = $"{Application.dataPath}/Script/Data/DataDefine.cs";

        //SVNHelper.ProcessCommand("TortoiseProc.exe", $"/command:revert /path:{fileFullPath} /closeonend:1");
        ProcessCommand("cmd.exe", $"svn revert --recursive {fileFullPath}");

        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset(fileFullPath);

        //EditorUtility.DisplayDialog("", "请等待编译完成", "继续");

        EditorPrefs.SetBool(Key_BuildPackageOnScriptReloaded, true);

        string CSFileStr;
        using (StreamReader sr = new StreamReader(fileFullPath))
        {
            CSFileStr = sr.ReadToEnd();
        }

        string source, target, flag;

        //修改渠道
        flag = "public static PLATFOEMTYPE platformType = PLATFOEMTYPE";
        source = $"{flag}.*;";
        target = $"{flag}.{config.Platform.ToString()};";
        RegexReplace(source, target, fileFullPath);

        //修改内外网
        flag = "public static bool isOutLine = ";
        source = $"{flag}.*;";
        target = $"{flag}{config.OutLine.ToString().ToLower()};";
        RegexReplace(source, target, fileFullPath);

        //修改默认语言
        flag = "public static LanguageType languageType = LanguageType";
        source = $"{flag}.*;";
        target = $"{flag}.{config.DefualtLanguage.ToString()};";
        RegexReplace(source, target, fileFullPath);


        //修改版本号
        flag = "public const string ClientVersion = ";
        source = $"{flag}.*;";
        target = $"{flag}\"{config.ClientVersion}\";";
        RegexReplace(source, target, fileFullPath);

        //修改构建号
        flag = "public const string VersionCode = ";
        source = $"{flag}.*;";
        target = $"{flag}\"{config.VersionCode}\";";
        RegexReplace(source, target, fileFullPath);

        //修改语言选项
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("public static KeyValuePair<LanguageType, string>[] languagesUsed =");
        sb.AppendLine("    {");
        switch (config.Platform)
        {
            case PLATFOEMTYPE.FunRock:
                sb.AppendLine("        new KeyValuePair<LanguageType, string>(LanguageType.arabic,  \"العربية\"),// 阿拉伯语");
                break;
            case PLATFOEMTYPE.NONE:
                sb.AppendLine("        new KeyValuePair<LanguageType, string>(LanguageType.chinese, \"简体中文\"),//中文");
                break;
            default:
                sb.AppendLine("        new KeyValuePair<LanguageType, string>(LanguageType.english, \"English\"),//英文");
                break;
        }
        sb.AppendLine("    };");

        flag = "public static KeyValuePair<LanguageType, string>";
        source = $"{flag}([\\d\\D]*)}};";
        target = sb.ToString();
        RegexReplace(source, target, fileFullPath);

        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset(fileFullPath);
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        if (EditorPrefs.GetBool(Key_BuildPackageOnScriptReloaded))
        {
            EditorPrefs.SetBool(Key_BuildPackageOnScriptReloaded, false);
            BuildXCode();
        }
    }

    private static void BuildXCode()
    {
        Debug.LogError("ScriptsReloaded!!");

        BuildPlayerOptions option = new BuildPlayerOptions();
        option.scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
        option.locationPathName = EditorPrefs.GetString(Key_XcodeExportPath);
        option.target = BuildTarget.iOS;
        Debug.Log($"Export path : {option.locationPathName}");

        if(Directory.Exists(option.locationPathName))
        {
            Directory.Delete(option.locationPathName, true);
        }

        Directory.CreateDirectory(option.locationPathName);

        PackagerTool.Packager.BuildiPhoneResource();

        BuildPipeline.BuildPlayer(option);
    }

    private static void UploadConfigAsset()
    {
        string fileFullPath = $"{Application.dataPath}/Editor/XCodeExportTool/ExportXcodeParam.asset";
        ProcessCommand("cmd.exe", $"svn ci -m {"Export XCode Project"} {fileFullPath}") ;
    }

    /// <summary>
    /// 删除指定目录下所有内容
    /// </summary>
    /// <param name="dirPath"></param>
    public static void DeleteDirectoryContentEx(string dirPath)
    {
        if (Directory.Exists(dirPath))
        {
            Directory.Delete(dirPath, true);
        }
    }

    private static void RegexReplace(string source, string target, string fileFullPath, RegexOptions op = RegexOptions.IgnoreCase)
    {
        Regex r = new Regex(source, op);
        try
        {
            string inputText = string.Empty;
            using (StreamReader sr = new StreamReader(fileFullPath))
            {
                inputText = sr.ReadToEnd();
            }

            if (r.IsMatch(inputText))
            {
                string result = r.Replace(inputText, target);

                using (StreamWriter sw = new StreamWriter(fileFullPath))
                {
                    sw.Write(result);
                }
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e.Message);
        }
    }

    public static int GetSVNRevision()
    {
        string revision = null;
        System.Diagnostics.ProcessStartInfo start = new System.Diagnostics.ProcessStartInfo("svn", "info");
        start.CreateNoWindow = true;
        start.RedirectStandardInput = true;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        start.UseShellExecute = false;
        start.WorkingDirectory = $"{Application.dataPath}/../";
        System.Diagnostics.Process process = System.Diagnostics.Process.Start(start);
        System.IO.StreamReader reader = process.StandardOutput;
        string line = reader.ReadLine();
        if (string.IsNullOrEmpty(line))
        {
            return 0;
        }
        if (line.StartsWith("Revision:"))
            revision = line.Substring("Revision:".Length);
        while (!reader.EndOfStream)
        {
            line = reader.ReadLine();
            if (line.StartsWith("Revision:"))
                revision = line.Substring("Revision:".Length);
        }
        process.WaitForExit();
        process.Close();
        reader.Close();

        if (revision != null)
        {
            return int.Parse(revision.Trim());
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="command"></param>
    /// <param name="argument"></param>
    public static void ProcessCommand(string command, string argument)
    {
        System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(command);
        info.Arguments = argument;
        info.CreateNoWindow = false;
        info.ErrorDialog = true;
        info.UseShellExecute = true;
        if (info.UseShellExecute)
        {
            info.RedirectStandardOutput = false;
            info.RedirectStandardError = false;
            info.RedirectStandardInput = false;
        }
        else
        {
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.RedirectStandardInput = true;
            info.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;
            info.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
        }
        System.Diagnostics.Process process = System.Diagnostics.Process.Start(info);
        if (!info.UseShellExecute)
        {
            Debug.Log(process.StandardOutput);
            Debug.LogError(process.StandardError);
        }
        process.WaitForExit();
        process.Close();
    }
}
