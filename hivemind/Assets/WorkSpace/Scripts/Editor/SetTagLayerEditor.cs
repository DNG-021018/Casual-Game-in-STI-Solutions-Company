using UnityEngine;
using UnityEditor;

public class SetTagLayerEditor : EditorWindow
{
    private GameObject parentObject;
    private int selectedTagIndex = 0;
    private int selectedLayer = 0;

    private string[] tags;
    private string[] layers;

    [MenuItem("Tools/Set Tag & Layer")]
    public static void ShowWindow()
    {
        GetWindow<SetTagLayerEditor>("Set Tag & Layer");
    }

    private void OnEnable()
    {
        tags = UnityEditorInternal.InternalEditorUtility.tags;
        layers = UnityEditorInternal.InternalEditorUtility.layers;
    }

    private void OnGUI()
    {
        GUILayout.Label("Set Tag for Parent & Layer for Children", EditorStyles.boldLabel);

        parentObject = (GameObject)EditorGUILayout.ObjectField("Parent Object", parentObject, typeof(GameObject), true);

        if (parentObject == null)
        {
            EditorGUILayout.HelpBox("Please assign a parent GameObject.", MessageType.Info);
            return;
        }

        selectedTagIndex = EditorGUILayout.Popup("Tag for Parent", selectedTagIndex, tags);
        selectedLayer = EditorGUILayout.Popup("Layer for Children", selectedLayer, layers);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Apply"))
        {
            ApplyTagAndLayer();
        }
    }

    private void ApplyTagAndLayer()
    {
        Undo.RegisterFullObjectHierarchyUndo(parentObject, "Set Tag & Layer");

        // Set tag cho thằng cha
        parentObject.tag = tags[selectedTagIndex];

        // Set layer cho tất cả thằng con
        foreach (Transform child in parentObject.GetComponentsInChildren<Transform>(true))
        {
            if (child != parentObject.transform) // Bỏ qua thằng cha
            {
                child.gameObject.layer = LayerMask.NameToLayer(layers[selectedLayer]);
            }
        }

        Debug.Log($"Set tag '{tags[selectedTagIndex]}' for parent and layer '{layers[selectedLayer]}' for children.");
    }
}
