using UnityEngine;
using UnityEditor;
using System.Collections;
using ScrollableListView;

[CustomEditor(typeof(ListView))]
public class ListViewEditor : Editor {

    public override void OnInspectorGUI()
    {
        bool changed = false;
        SerializedProperty dataSource = this.serializedObject.FindProperty("dataSource");
        SerializedProperty baseAdapter = this.serializedObject.FindProperty("baseAdapter");
        SerializedProperty contentBuffer = this.serializedObject.FindProperty("contentBuffer");
        SerializedProperty verticalLayout = this.serializedObject.FindProperty("verticalLayout");
        EditorGUILayout.BeginVertical();
        {
            changed |= EditorGUILayout.PropertyField(dataSource);
            changed |= EditorGUILayout.PropertyField(baseAdapter);
            changed |= EditorGUILayout.PropertyField(contentBuffer);
            changed |= EditorGUILayout.PropertyField(verticalLayout);
        }
        EditorGUILayout.EndVertical();

        this.serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(this.target);

        base.OnInspectorGUI();       
    }
}
