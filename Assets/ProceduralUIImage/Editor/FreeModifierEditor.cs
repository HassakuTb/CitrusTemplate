using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI.ProceduralImage;

[CustomEditor(typeof(FreeModifier), true)]
[CanEditMultipleObjects]
public class FreeModifierEditor : Editor
{
    protected SerializedProperty radiusProperty;
    protected void OnEnable()
    {
        radiusProperty = serializedObject.FindProperty("radius");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        GUILayout.Space(8);
        RadiusGUI();
        serializedObject.ApplyModifiedProperties();
    }

    protected void RadiusGUI()
    {
        Vector4 radius = radiusProperty.vector4Value;
        GUILayout.BeginHorizontal();
        {
            radius.x = EditorGUILayout.FloatField("Upper Left:",radius.x);
            radius.y = EditorGUILayout.FloatField("Upper Right:", radius.y);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(8);
        GUILayout.BeginHorizontal();
        {
            radius.w = EditorGUILayout.FloatField("Lower Left:", radius.w);
            radius.z = EditorGUILayout.FloatField("Lower Right:", radius.z);
        }
        GUILayout.EndHorizontal();
        radiusProperty.vector4Value = radius;
    }
}
