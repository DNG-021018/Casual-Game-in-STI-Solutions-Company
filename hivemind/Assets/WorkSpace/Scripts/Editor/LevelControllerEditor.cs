// Assets/Editor/LevelControllerEditor.cs
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelController))]
public class LevelControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        LevelController controller = (LevelController)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Manual Testing", EditorStyles.boldLabel);

        if (GUILayout.Button("Test Manual Map (Reload Scene)"))
        {
            controller.TestManualMap();
        }
    }
}
