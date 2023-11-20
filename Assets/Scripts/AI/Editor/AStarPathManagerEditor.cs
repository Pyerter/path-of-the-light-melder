using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AStarPathManager))]
public class AStarPathManagerEditor : Editor
{

    bool experimentalFoldout = false;
    bool pathListFoldout = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        AStarPathManager myTarget = (AStarPathManager)target;

        experimentalFoldout = EditorGUILayout.Foldout(experimentalFoldout, "Experimental Options");
        if (experimentalFoldout)
        {
            EditorGUI.indentLevel++;
            myTarget.UsePreviousPathAsPriority = EditorGUILayout.Toggle("Use Informed A*: ", myTarget.UsePreviousPathAsPriority);
            EditorGUI.indentLevel--;
        }

        pathListFoldout = EditorGUILayout.Foldout(pathListFoldout, "Current Paths");
        if (pathListFoldout)
        {
            EditorGUI.indentLevel++;
            AStarPathManager.PathInstance pathInstance = myTarget.CurrentPath;
            if (pathInstance.positions != null)
            {
                if (pathInstance.positions.Count == 0)
                    EditorGUILayout.LabelField("No points in path.");
                for (int i = 0; i < pathInstance.positions.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (i == pathInstance.currentIndex)
                    {
                        EditorGUILayout.LabelField("Current Position " + i + ": ", EditorStyles.boldLabel);
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Position " + i + ":");
                    }
                    EditorGUILayout.Vector2Field("", pathInstance.positions[i]);
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.LabelField("No path instance set.");
            }
            EditorGUI.indentLevel--;
        }
    }
}
