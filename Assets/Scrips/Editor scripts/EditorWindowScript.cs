using UnityEngine;
using UnityEditor;

public class EditorWindowScript : EditorWindow
{
    private GameObject[] prefabs;
    private Vector2 scrollPosition;

    private Vector2 prefabRectSize = new Vector2(300, 300);
    private Vector2 nextPrefabPosition = new Vector2(10, 10);

    [MenuItem("Window/Level editor")]
    public static void ShowWindow()
    {
        EditorWindowScript window = GetWindow<EditorWindowScript>("Level editor");
        window.minSize = new Vector2(800, 600);
    }

    void OnGUI()
    {
        GetPrefabs();
        ShowPrefabList();

    }

    private void GetPrefabs()
    {
        prefabs = Resources.LoadAll<GameObject>("Prefabs");
        //Debug.Log(prefabs.Length);
    }

    private void ShowPrefabList()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUI.DrawRect(new Rect(nextPrefabPosition, prefabRectSize), Color.blue);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(400));

        for (int i = 0; i < prefabs.Length / 2; i++)
        {
            EditorGUI.DrawRect(new Rect(nextPrefabPosition, prefabRectSize), Color.gray);
            nextPrefabPosition = new Vector2(nextPrefabPosition.x + prefabRectSize.x + 20, nextPrefabPosition.y);
            AssetPreview.GetMiniThumbnail(prefabs[i]);
        }

        GUILayout.Label("Scroll ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndHorizontal();
    }
}
