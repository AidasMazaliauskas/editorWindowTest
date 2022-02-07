using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace TutoTOONS 
{
    public class SetImageMaxSize : EditorWindow
    {
        [MenuItem("TutoTOONS/Set Image Max Size")]

        static void Init()
        {
            SetImageMaxSize window = (SetImageMaxSize)EditorWindow.GetWindow(typeof(SetImageMaxSize));
			window.titleContent = new GUIContent("Set Image Max Size");
            window.Show();
            window.position = new Rect(50, 80, 800, 500);

            fileTypes.Clear();

            fileTypes.Add(".jpg");
            fileTypes.Add(".jpeg");
            fileTypes.Add(".bmp");
            fileTypes.Add(".tiff");
            fileTypes.Add(".png");
            fileTypes.Add(".tga");

        }

        static List<string> listResult;
        static List<string> fileTypes = new List<string>();
        static string files;
        static List<int> maxSizes;
        Vector2 scroll;

        private void OnGUI()
        {
            if (GUILayout.Button("Search!"))
            {
                DoSearch();
            }

            if (listResult != null && maxSizes != null)
            {
                GUILayout.BeginVertical();
                GUILayout.Space(10);
                GUILayout.Label("Found " + listResult.Count + " images in project");
                for (int i = 0; i < fileTypes.Count; i++)
                {
                    files = files + fileTypes[i] + " ";
                }
                GUILayout.Label("Searched for these files: " + files);
                GUILayout.Space(10);
                GUILayout.EndVertical();
                if (GUILayout.Button("Set max sizes for textures", GUILayout.Width(position.width)))
                {
                    SetTextureMaxSizes();
                    DoSearch();
                }
                if (GUILayout.Button("Reset max sizes to 2048", GUILayout.Width(position.width)))
                {
                    SetTextureMaxSizes(2048);
                    DoSearch();
                }

                scroll = GUILayout.BeginScrollView(scroll);
                for (int i = 0; i < listResult.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(listResult[i], GUILayout.Width(position.width * 0.68f));
                    GUILayout.Label(maxSizes[i].ToString(), GUILayout.Width(position.width * 0.1f));
                    if (GUILayout.Button("Select", GUILayout.Width(position.width * 0.22f)))
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
            listResult = GetAllImages().ToList();
            maxSizes = GetTexMaxSizes();
        }

        public static string[] GetAllImages()
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

        static void SetTextureMaxSizes()
        {
            List<int> _sizes = new List<int>();
            _sizes.Add(32);
            _sizes.Add(64);
            _sizes.Add(128);
            _sizes.Add(256);
            _sizes.Add(512);
            _sizes.Add(1024);

            for (int i = 0; i < listResult.Count; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Progress", i + "/" + listResult.Count, (float)i / (float)listResult.Count)) break;
                TextureImporter textureImporter = AssetImporter.GetAtPath(listResult[i]) as TextureImporter;
                Texture2D _tex = (Texture2D)AssetDatabase.LoadAssetAtPath(listResult[i], typeof(Texture2D));
                if (_tex.width >= _tex.height)
                {
                    for (int j = _sizes.Count-1; j > 0; j--)
                    {
                        if (_tex.width >= _sizes[j] && j >= 1)
                        {
                            textureImporter.maxTextureSize = _sizes[j];
                            break;
                        }
                    }
                }
                else
                {
                    for (int j = _sizes.Count - 1; j > 0; j--)
                    {
                        if (_tex.height >= _sizes[j] && j >= 1)
                        {
                            textureImporter.maxTextureSize = _sizes[j];
                            break;
                        }
                    }
                }
                AssetDatabase.ImportAsset(listResult[i]);
            }
            EditorUtility.ClearProgressBar();
        }

        static void SetTextureMaxSizes(int _size)
        {
            for (int i = 0; i < listResult.Count; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Progress", i + "/" + listResult.Count, (float)i / (float)listResult.Count)) break;
                TextureImporter textureImporter = AssetImporter.GetAtPath(listResult[i]) as TextureImporter;
                textureImporter.maxTextureSize = _size;
                AssetDatabase.ImportAsset(listResult[i]);
            }
            EditorUtility.ClearProgressBar();
        }

        static List<int> GetTexMaxSizes()
        {
            List<int> _textures = new List<int>();
            foreach (string assetPath in listResult.ToList())
            {
                TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (textureImporter != null)
                {
                    _textures.Add(textureImporter.maxTextureSize);
                }
                else
                {
                    listResult.Remove(assetPath);
                }
            }
            return _textures;
        }
    }

}
