using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GoLessonData))]
public class GoLessonDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        if (GUILayout.Button("Open Lesson Builder"))
            GoLessonBuilderWindow.OpenWindow((GoLessonData)target);
    }
}
