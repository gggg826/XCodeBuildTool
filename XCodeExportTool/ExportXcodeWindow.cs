using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Runtime.Serialization.Formatters.Binary;

public class ExportXcodeWindow : EditorWindow
{
#if UNITY_IOS
    private const int m_AreaBorder = 10;

    private static List<string> m_ConfigsNames;
    private static Dictionary<string, ExportXcodeConfiguration> m_Configs;
    private static int m_CurrentSelectIndex;
    private static ExportXcodeConfiguration m_CurrentConfig;
    private bool m_ShowSaveAsView;
    private string m_SaveAsName = "Type a name";
    private static string LastButtonIndxkey = "ExportXcodeButtonIndex";

    [MenuItem("Tools/Export-XCode")]
    private static void ExportXcode()
    {
        ExportXcodeWindow window = EditorWindow.GetWindow<ExportXcodeWindow>();
        //window.minSize = new Vector2(360f, 390f);
        ReloadConfigs();
        window.Show();
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void ReloadConfigs()
    {
        if (m_ConfigsNames == null)
        {
            m_ConfigsNames = new List<string>();
        }
        else
        {
            m_ConfigsNames.Clear();
        }
        if (m_Configs == null)
        {
            m_Configs = new Dictionary<string, ExportXcodeConfiguration>();
        }
        else
        {
            m_Configs.Clear();
        }

        List<ExportXcodeConfiguration> configs = ExportXcodeParam.Instance.Configs;
        //m_CurrentSelectIndex = 0;

        for (int i = 0, count = configs.Count; i < count; i++)
        {
            m_ConfigsNames.Add(configs[i].Name);
            m_Configs.Add(configs[i].Name, configs[i]);
        }

        m_CurrentSelectIndex = EditorPrefs.GetInt(LastButtonIndxkey, 0);
        //m_CurrentConfig = DeepClone<ExportXcodeConfiguration>(m_Configs[m_ConfigsNames[m_CurrentSelectIndex]]);
        m_CurrentConfig = m_Configs[m_ConfigsNames[m_CurrentSelectIndex]].Clone();
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(m_AreaBorder, m_AreaBorder, position.width - m_AreaBorder * 2, position.height - m_AreaBorder *2));

       if(m_ShowSaveAsView)
        {
            DrawSaveAsView();
        }
       else
        {
            DrawConfigsDetails();
        }

        GUILayout.EndArea();
    }
    
    private void DrawConfigsDetails()
    {
        int selectedndex = GUILayout.Toolbar(m_CurrentSelectIndex, m_ConfigsNames.ToArray(), "LargeButton");
        if (m_CurrentSelectIndex != selectedndex)
        {
            m_CurrentSelectIndex = selectedndex;
            //m_CurrentConfig = DeepClone<ExportXcodeConfiguration>(m_Configs[m_ConfigsNames[m_CurrentSelectIndex]]);
            m_CurrentConfig = m_Configs[m_ConfigsNames[m_CurrentSelectIndex]].Clone();
        }
        m_SaveAsName = m_CurrentConfig.Name;
        GUILayout.Space(20);
        m_CurrentConfig.Platform = (PLATFOEMTYPE)EditorGUILayout.EnumPopup("Platform", m_CurrentConfig.Platform);
        EditorGUILayout.Space();
        m_CurrentConfig.OutLine = EditorGUILayout.Toggle("OutLine", m_CurrentConfig.OutLine);
        EditorGUILayout.Space();
        m_CurrentConfig.EnableGuest = EditorGUILayout.Toggle("EnableGuest", m_CurrentConfig.EnableGuest);
        EditorGUILayout.Space();
        m_CurrentConfig.CompanyName = EditorGUILayout.TextField("CompanyName", m_CurrentConfig.CompanyName);
        EditorGUILayout.Space();
        m_CurrentConfig.ProductName = EditorGUILayout.TextField("ProductName", m_CurrentConfig.ProductName);
        EditorGUILayout.Space();
        m_CurrentConfig.ApplicationIdentifier = EditorGUILayout.TextField("App-Identifier", m_CurrentConfig.ApplicationIdentifier);
        EditorGUILayout.Space();
        m_CurrentConfig.ClientVersion = EditorGUILayout.TextField("ClientVersion", m_CurrentConfig.ClientVersion);
        EditorGUILayout.Space();
        m_CurrentConfig.VersionCode = EditorGUILayout.TextField("VersionCode", m_CurrentConfig.VersionCode);
        EditorGUILayout.Space();
        m_CurrentConfig.DefualtLanguage = (LanguageType)EditorGUILayout.EnumPopup("DefualtLanguage", m_CurrentConfig.DefualtLanguage);
        EditorGUILayout.Space();
        m_CurrentConfig.Splash = (Texture2D)EditorGUILayout.ObjectField("Splash", m_CurrentConfig.Splash, typeof(Texture2D), false);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save Configuration"))
        {
            SaveConfig(m_SaveAsName);
            ReloadConfigs();
        }
        GUILayout.Space(3);
        if (GUILayout.Button("Save As ..."))
        {
            m_ShowSaveAsView = true;
        }
        GUILayout.Space(3);
        if (GUILayout.Button("Delete Configuration"))
        {
            List<ExportXcodeConfiguration> configs = ExportXcodeParam.Instance.Configs;
            int count = configs.Count;
            if(count <= 1)
            {
                EditorUtility.DisplayDialog("Warning", "至少保留一个配置方案", "OK");
                return;
            }
            for (int i = 0; i < count; i++)
            {
                if (configs[i].Name.Equals(m_ConfigsNames[m_CurrentSelectIndex]))
                {
                    configs.RemoveAt(i);
                    break;
                }
            }
            m_CurrentSelectIndex = 0;
            ReloadConfigs();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);
        Color temp = GUI.color;
        GUI.color = Color.cyan;
        if (GUILayout.Button("Start Export", GUILayout.MinHeight(50)))
        {
            SaveConfig(m_SaveAsName);
            ReloadConfigs();
            ExportHelper.ExportXCodeProject(m_CurrentConfig);
            //this.Close();
        }
        GUI.color = temp;
    }

    private void DrawSaveAsView()
    {
        GUILayout.Space(20);
        m_SaveAsName = EditorGUILayout.DelayedTextField("Save Name", m_SaveAsName);
        EditorGUILayout.HelpBox("输完名字要敲回车键", MessageType.Warning);
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("Back"))
        {
            m_ShowSaveAsView = false;
        }
        GUILayout.Space(3);
        if (GUILayout.Button("Save"))
        {
            SaveConfig(m_SaveAsName);
            ReloadConfigs();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void SaveConfig(string configName)
    {
        List<ExportXcodeConfiguration> configs = ExportXcodeParam.Instance.Configs;
        for (int i = 0; i < configs.Count; i++)
        {
            if (configs[i].Name.Equals(configName))
            {
                configs[i] = m_CurrentConfig;
                m_ShowSaveAsView = false;
                EditorPrefs.SetInt(LastButtonIndxkey, m_CurrentSelectIndex);
                return;
            }
        }
        ExportXcodeConfiguration newConfig = m_CurrentConfig;
        newConfig.Name = configName;
        configs.Add(newConfig);
        m_ShowSaveAsView = false;
        EditorPrefs.SetInt(LastButtonIndxkey, configs.Count - 1);
    }


    public static T  DeepClone<T>(object source)
    {
        T objectReturn = default(T);
        using (MemoryStream stream = new MemoryStream())
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, source);
                stream.Position = 0;
                objectReturn = (T)formatter.Deserialize(stream);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
        return objectReturn;
    }
#endif
}
