using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.IO;

namespace TutoTOONS
{
    public class AssetUsageTest : EditorWindow
    {
        string message;

        [MenuItem("TutoTOONS/Asset Usage Test")]
        private static void OpenWindow()
        {
            AssetUsageTest window = GetWindow<AssetUsageTest>();
            window.titleContent = new GUIContent("Asset Usage Test");
        }

        private void OnEnable()
        {
            if(Selection.assetGUIDs.Length == 0)
            {
                message = "No assets selected.";
                return;
            }

            // Get scene list
            string build_settings = File.ReadAllText("ProjectSettings/EditorBuildSettings.asset");
            Regex exp = new Regex(@"(?<=enabled\: 1[\n\s]+path\: )[^\n]+(?=\n)");
            MatchCollection matches = exp.Matches(build_settings);
            if (matches.Count == 0)
            {
                return;
            }
            Dictionary<string, string> scenes = new Dictionary<string, string>(matches.Count);
            //Read scene data
            for (int i = 0; i < matches.Count; i++)
            {
                string scene_path = matches[i].Groups[0].Value;
                string scene_name = scene_path.Substring(scene_path.LastIndexOf('/') + 1);
                //Debug.Log(scene_name);
                scenes[scene_name] = File.ReadAllText(scene_path);
            }

            foreach (string assetID in Selection.assetGUIDs)
            {
                bool sceneFound = false;
                string assetFilename = AssetDatabase.GUIDToAssetPath(assetID);
                message += assetFilename + " (" + assetID + ")\n";
                foreach(KeyValuePair<string, string> scene in scenes)
                {
                    if(scene.Value.IndexOf("guid: " + assetID + ",") > 0)
                    {
                        message += "  " + scene.Key + "\n";
                        sceneFound = true;
                    }
                }
                if (!sceneFound)
                {
                    message += "  (no scenes found)";
                }
                message += "\n";
            }
            message += "\n[Only checking scenes which are added to build settings. Also project needs to be saved after adding scenes.]";
        }

        private void OnGUI()
        {
            GUILayout.TextField(message);
        }
    }
}