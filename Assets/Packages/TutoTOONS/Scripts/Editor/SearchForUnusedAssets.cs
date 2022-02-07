using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace TutoTOONS
{
    public class SearchForUnusedAssets : EditorWindow
    {
        [MenuItem("TutoTOONS/Search For Unused Assets")]
        static void Init()
        {
            SearchForUnusedAssets window = (SearchForUnusedAssets)EditorWindow.GetWindow(typeof(SearchForUnusedAssets));
			window.titleContent = new GUIContent("Search For Unused Assets");
            window.Show();
            window.position = new Rect(50, 80, 1200, 500);

            fileTypes.Clear();

            fileTypes.Add(".jpg");
            fileTypes.Add(".jpeg");
            fileTypes.Add(".bmp");
            fileTypes.Add(".tiff");
            fileTypes.Add(".png");
            fileTypes.Add(".tga");

            //3d file formats
            fileTypes.Add(".fbx");
            fileTypes.Add(".obj");
            fileTypes.Add(".stl");

            //audio file formats
            fileTypes.Add(".mp3");
            fileTypes.Add(".ogg");
            fileTypes.Add(".wav");

            //other file formats
            fileTypes.Add(".physicMaterial");
            fileTypes.Add(".physicsMaterial2D");
            fileTypes.Add(".shader");
            fileTypes.Add(".mat");

            
        }

        List<string> listResult;
        List<bool> resultSelectors = new List<bool>();
        Vector2 scroll;
        long _localId = 0;
        string _guid = "";
        static bool createLog = false;
        static List<string> fileTypes = new List<string>();
        string files;

        void OnGUI()
        {
            GUILayout.Space(3);
            int oldValue = GUI.skin.window.padding.bottom;
            GUI.skin.window.padding.bottom = -20;
            Rect windowRect = GUILayoutUtility.GetRect(1, 17);
            windowRect.x += 4;
            windowRect.width -= 7;
            GUI.skin.window.padding.bottom = oldValue;
            GUILayout.Label("Make sure to build the project before running the search!");
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            createLog = GUILayout.Toggle(createLog, "Create build asset log in /Assets/Assets_in_build.txt?");
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(10);
            if (GUILayout.Button("Search!"))
            {
                DoSearch();
            }

            if (listResult != null)
            {
                GUILayout.BeginVertical();
                GUILayout.Space(10);
                GUILayout.Label("Found " + listResult.Count + " unused assets in the build");
                for (int i = 0; i < fileTypes.Count; i++)
                {
                    files = files + fileTypes[i] + " ";
                }
                GUILayout.Label("Searched for these files: " + files);
                GUILayout.Space(10);
                GUILayout.EndVertical();
                if (GUILayout.Button("Delete selected", GUILayout.Width(position.width)))
                {
                    for (int i = 0; i < listResult.Count; i++)
                    {
                        if (resultSelectors[i])
                        {
                            AssetDatabase.DeleteAsset(listResult[i]);
                        }
                    }
                    for (int i = 0; i < resultSelectors.Count; i++)
                    {
                        resultSelectors[i] = true;
                    }

                    DoSearch();
                    
                }
                if (GUILayout.Button("Select all", GUILayout.Width(position.width * 0.2f)))
                {
                    for (int i = 0; i < resultSelectors.Count; i++)
                    {
                        resultSelectors[i] = true;
                    }
                }
                if (GUILayout.Button("Select none", GUILayout.Width(position.width * 0.2f)))
                {
                    for (int i = 0; i < resultSelectors.Count; i++)
                    {
                        resultSelectors[i] = false;
                    }
                }
                scroll = GUILayout.BeginScrollView(scroll);
                for (int i= 0; i < listResult.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    resultSelectors[i] = GUILayout.Toggle(resultSelectors[i], "", GUILayout.Width(position.width * 0.02f));
                    GUILayout.Label(listResult[i], GUILayout.Width(position.width * 0.68f));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Select", GUILayout.Width(position.width * 0.30f)))
                    {
                        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(listResult[i]);
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
                files = "";
            }
        }

        void DoSearch()
        {
            string[] allAssets = GetAllAssets();
            string _log = GetEditorLog();
            string _settings = GetProjectSettings();
            listResult = new List<string>();
            if (_log.Length > 0)
            {
                foreach (string asset in allAssets)
                {
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(AssetDatabase.LoadMainAssetAtPath(asset), out _guid, out _localId);
                    if (!_log.Contains(asset) && !_settings.Contains(_guid) && !asset.Contains("/Resources/"))
                    {
                        listResult.Add(asset);
                        resultSelectors.Add(true);
                    }
                }
                listResult.Sort();
            }
        }

        public static string GetEditorLog()
        {
            string _appDataPath = "";
            string _logPath = "";
            string line;
            string _newlog = "";
            string _tempLog = "";

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                _appDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                _logPath = _appDataPath + "\\Unity\\Editor\\Editor.log";
            }
            else if (Application.platform == RuntimePlatform.OSXEditor)
            {
                _appDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                _logPath = _appDataPath + "/Library/Logs/Unity/Editor.log";
            }

            FileStream fs = new FileStream(_logPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            StreamReader sr = new StreamReader(fs);

            _tempLog = sr.ReadToEnd();
            sr.DiscardBufferedData();
            sr.BaseStream.Seek(0, SeekOrigin.Begin);

            if (Regex.Matches(_tempLog, "Build Report").Count == 0)
            {
                Debug.LogError("Build log not found! Are you sure the project was built before the search?");
                return "";
            }

            for (int i = 0; i < Regex.Matches(_tempLog, "Build Report").Count; i++)
            {
                while (!(line = sr.ReadLine()).Contains("Used Assets ")) ;
            }

            while (!(line = sr.ReadLine()).Contains("-----------"))
            {
                line = line.Substring(line.IndexOf("% ") + 2);
                _newlog = _newlog + "\n" + line;
            }
            if (createLog)
            {
                File.WriteAllText(Application.dataPath + "\\Assets_in_build.txt", _newlog);
            }

            return _newlog;
        }

        public static string GetProjectSettings()
        {
            string _settingsPath = Application.dataPath.Substring(0, Application.dataPath.Length - 7) + "/ProjectSettings/ProjectSettings.asset";

            FileStream fs = new FileStream(_settingsPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            StreamReader sr = new StreamReader(fs);

            return sr.ReadToEnd();
        }

        public static string[] GetAllAssets()
        {
            string[] temp = AssetDatabase.GetAllAssetPaths();
            List<string> result = new List<string>();
            foreach (string s in temp)
            {
                if (fileTypes.Any(s.Contains))
                {
                    result.Add(s);
                }
            }
            return result.ToArray();
        }
    }
}

