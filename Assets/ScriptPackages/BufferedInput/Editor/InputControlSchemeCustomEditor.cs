/*
 * Author: Porter Squires
 */

using UnityEditor;

namespace BufferedInput {
    [CustomEditor(typeof(InputControlScheme))]
    public class InputControlSchemeCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            InputControlScheme myTarget = (InputControlScheme)target;

            if (myTarget.maps == null)
            {
                EditorGUILayout.HelpBox("Maps variable is null.", MessageType.Error);
            } else
            {
                foreach (ControlSchemeMap map in myTarget.maps)
                {
                    EditorGUILayout.LabelField("Map: " + map.MapName);
                    EditorGUI.indentLevel += 1;
                    if (map.actions == null)
                    {
                        EditorGUILayout.HelpBox("Map actions is null.", MessageType.Error);
                    }
                    else
                    {
                        foreach (ControlSchemeAction action in map.actions)
                        {
                            EditorGUILayout.LabelField("Action: " + action.Identifier.properName);
                        }
                    }
                    EditorGUI.indentLevel -= 1;
                }
            }
        }
    }
}