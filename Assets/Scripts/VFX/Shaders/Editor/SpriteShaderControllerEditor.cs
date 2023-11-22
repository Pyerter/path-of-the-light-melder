using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpriteShaderController))]
public class SpriteShaderControllerEditor : Editor
{

    protected Dictionary<string, SerializedObject> objects = new Dictionary<string, SerializedObject>();

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SpriteShaderController myTarget = (SpriteShaderController)target;

        EditorGUILayout.LabelField("Sprite Materials");
        foreach (Material mat in myTarget.Materials)
        {
            EditorGUILayout.LabelField(mat.name);
        }

        EditorGUILayout.LabelField("Materials");
        EditorGUI.indentLevel++;
        foreach (NamedMaterial mat in myTarget.MappedMaterials)
        {
            if (mat == null)
            {
                EditorGUILayout.LabelField("Material is null.");
                continue;
            }
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(mat.MaterialName + ": " + mat.Index);
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel++;
            if (objects.TryGetValue(mat.MaterialName, out SerializedObject obj))
            {
                EditorGUILayout.PropertyField(obj.FindProperty("material"));
            } else
            {
                objects.Add(mat.MaterialName, new SerializedObject(mat));
                EditorGUILayout.PropertyField(objects[mat.MaterialName].FindProperty("material"));
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
        EditorGUI.indentLevel--;
    }
}
