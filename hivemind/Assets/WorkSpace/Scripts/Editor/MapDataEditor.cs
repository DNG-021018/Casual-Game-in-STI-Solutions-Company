using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(MapData))]
public class MapDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapData mapData = (MapData)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Auto Init LevelDetail from MapPrefab"))
        {
            mapData.InitLevelDetail();
            EditorUtility.SetDirty(mapData);
            AssetDatabase.SaveAssets();
            Debug.Log("MapData updated and saved successfully.");
        }
    }
}