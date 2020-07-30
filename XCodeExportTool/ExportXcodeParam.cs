using UnityEngine;
using System.IO;
using UnityEditor;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;


[Serializable]
public class ExportXcodeConfiguration
{
    public string Name;
    public PLATFOEMTYPE Platform;
    public bool OutLine;
    public bool EnableGuest;
    public string CompanyName;
    public string ProductName;
    public string ApplicationIdentifier;
    public string ClientVersion;
    public string VersionCode;
    public LanguageType DefualtLanguage;
    public Texture2D Splash;

    public ExportXcodeConfiguration()
    {
        Name = "new";
        CompanyName = "";
        ProductName = "";
        ApplicationIdentifier = "";
        ClientVersion = "";
        VersionCode = "";
    }

    public ExportXcodeConfiguration Clone()
    {
        ExportXcodeConfiguration result = new ExportXcodeConfiguration();
        result.Name = this.Name;
        result.Platform = this.Platform;
        result.CompanyName = this.CompanyName;
        result.ProductName = this.ProductName;
        result.ApplicationIdentifier = this.ApplicationIdentifier;
        result.ClientVersion = this.ClientVersion;
        result.VersionCode = this.VersionCode;
        result.DefualtLanguage = this.DefualtLanguage;
        result.Splash = this.Splash;
        result.OutLine = this.OutLine;
        result.EnableGuest = this.EnableGuest;
        return result;
    }
}
public class ExportXcodeParam : ScriptableObject
{
    private const string SOURCEFULLPATH = "Assets/Editor/XCodeExportTool/ExportXcodeParam.asset";
    private static ExportXcodeParam m_Instance;
    public static ExportXcodeParam Instance
    {
        get
        {
            if (m_Instance == null)
            {
                if (!File.Exists(Path.GetFullPath(SOURCEFULLPATH)))
                {
                    CreateScriptableObject();
                }
                m_Instance = (ExportXcodeParam)AssetDatabase.LoadAssetAtPath(SOURCEFULLPATH, typeof(ExportXcodeParam));
            }
            if(m_Instance.Configs.Count == 0)
            {
                m_Instance.Configs.Add(new ExportXcodeConfiguration());
            }
            return m_Instance as ExportXcodeParam;
        }
    }
    
    private static void CreateScriptableObject()
    {
        ExportXcodeParam source = CreateInstance<ExportXcodeParam>();
        AssetDatabase.CreateAsset(source, SOURCEFULLPATH);
    }
    
    public List<ExportXcodeConfiguration> Configs = new List<ExportXcodeConfiguration>();
    
}

