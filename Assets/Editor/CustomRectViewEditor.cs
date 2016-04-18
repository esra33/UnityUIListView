using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RecTransformCompliment))]
public class CustomRectViewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RecTransformCompliment target = this.target as RecTransformCompliment;
        RectTransform rectTransform = target.transform as RectTransform;
        Transform transform = target.transform;

        EditorGUILayout.Vector3Field("Position", transform.position);
        EditorGUILayout.RectField("Rect", rectTransform.rect);
    }
}