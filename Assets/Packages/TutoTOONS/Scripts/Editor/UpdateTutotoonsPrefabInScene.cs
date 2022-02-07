using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace TutoTOONS
{
    public class UpdateTutotoonsPrefabInScene : EditorWindow
    {
        private const string PREFAB_PATH = "Assets/Packages/TutoTOONS/TutoTOONS.prefab";

        public string[] options = new string[] { "All Scenes", "First Scene Only" };
        public int index = 0;

        [MenuItem("TutoTOONS/Add or Update Tutotoons prefab in scenes")]
        private static void OpenMenu()
        {
            UpdateTutotoonsPrefabInScene window = (UpdateTutotoonsPrefabInScene)EditorWindow.GetWindow(typeof(UpdateTutotoonsPrefabInScene));
            window.titleContent = new GUIContent("Add or Update TutoTOONS prefab");
            window.Show();
            window.position = new Rect(50, 80, 400, 300);
        }

        private void OnGUI()
        {
            int current_scene_index = EditorSceneManager.GetActiveScene().buildIndex;
            index = EditorGUILayout.Popup(index, options);

            if (index == 0)
            {
                GUILayout.Label("Adds or updates prefab in every scene");
            }
            if (index == 1)
            {
                GUILayout.Label("Updates prefab in first scene and removes it from all other scenes");
            }
            if (GUILayout.Button("Update!"))
            {
                AddUpdatePrefab();
                if (current_scene_index != -1)
                {
                    EditorSceneManager.OpenScene(EditorBuildSettings.scenes[current_scene_index].path, OpenSceneMode.Single);
                }
            }
        }

        private void AddUpdatePrefab()
        {
            GameObject _prefab_tuto = AssetDatabase.LoadAssetAtPath(PREFAB_PATH, typeof(GameObject)) as GameObject;

            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                string _scene_path = EditorBuildSettings.scenes[i].path;
                Scene _opened_scene = EditorSceneManager.OpenScene(_scene_path, OpenSceneMode.Single);

                foreach (TutoTOONS _prefab_in_scene in FindObjectsOfType<TutoTOONS>(true))
                {
                    DestroyImmediate(_prefab_in_scene.gameObject);
                }

                if (index == 0 || index == 1 && i == 0)
                {
                    PrefabUtility.InstantiatePrefab(_prefab_tuto);
                }

                EditorSceneManager.MarkSceneDirty(_opened_scene);
                EditorSceneManager.SaveOpenScenes();
            }
        }
    }
}