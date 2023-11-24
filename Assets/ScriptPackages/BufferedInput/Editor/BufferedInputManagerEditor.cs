/*
 * Author: Porter Squires
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BufferedInput
{
    [CustomEditor(typeof(BufferedInputManager))]
    public class BufferedInputManagerEditor : Editor
    {
        private bool controlSchemeSettingsDropdown = false;
        private bool eventsDropdown = false;
        private bool[] eventsDropdownMaps = null;

        private bool initialized = false;
        private bool stopInitializing = false;

        private Dictionary<string, SerializedProperty> serializedActions = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            BufferedInputManager myTarget = (BufferedInputManager)target;
            if (myTarget.inputControlScheme == null)
            {
                eventsDropdownMaps = null;
                initialized = false;
                // Debug.LogWarning("Found control scheme to be null, setting initialized (in editor) to false");
            }
            else if (!initialized || eventsDropdownMaps == null)
            {
                eventsDropdownMaps = new bool[myTarget.inputControlScheme.maps.Count];
                initialized = false;
                // Debug.Log("Recreated bool array for foldouts");
            }

            if (!initialized && eventsDropdownMaps != null && !stopInitializing)
            {
                serializedActions = new Dictionary<string, SerializedProperty>();
                try
                {
                    foreach (ControlSchemeMap map in myTarget.inputControlScheme.maps)
                    {
                        foreach (ControlSchemeAction action in map.actions)
                        {
                            serializedActions.Add(action.Identifier.fullName, new SerializedObject(action).FindProperty("inputEvent"));
                        }
                    }
                    initialized = true;
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Exception while initializing serialized properties for action map: " + e.GetType().ToString() + " - " + e.Message + "\n" + e.StackTrace);
                    serializedActions = null;
                    stopInitializing = true;
                }
            }

            EditorGUILayout.Space(10);

            if (Application.isEditor)
            {
                controlSchemeSettingsDropdown = EditorGUILayout.Foldout(controlSchemeSettingsDropdown || !initialized, "Control Scheme Settings (Critical & Sensitive Dropdown)");
                if (controlSchemeSettingsDropdown)
                {
                    if (GUILayout.Button("Reinitialize Control Scheme"))
                    {
                        myTarget.InitializeActionMap();
                        stopInitializing = false;
                        initialized = false;
                        eventsDropdownMaps = null;
                        if (serializedActions != null)
                            serializedActions.Clear();
                        Debug.Log("Reinitialized control scheme. Control scheme not null: " + (myTarget.inputControlScheme));
                        return;
                    }
                }
            }

            EditorGUILayout.Space(10);

            if (!initialized)
            {
                EditorGUILayout.HelpBox("Control Scheme Uninitialized", MessageType.Error);
            }
            else
            {
                eventsDropdown = EditorGUILayout.Foldout(eventsDropdown, "Buffered Input Events");
                if (eventsDropdown)
                {
                    EditorGUI.indentLevel += 1;
                    for (int i = 0; i < eventsDropdownMaps.Length; i++)
                    {
                        eventsDropdownMaps[i] = EditorGUILayout.Foldout(eventsDropdownMaps[i], myTarget.inputControlScheme.maps[i].MapName);
                        if (eventsDropdownMaps[i])
                        {
                            EditorGUI.indentLevel += 1;
                            foreach (ControlSchemeAction schemeAction in myTarget.inputControlScheme.maps[i].actions)
                            {
                                try
                                {
                                    EditorGUILayout.LabelField(schemeAction.Identifier.actionName);
                                    EditorGUILayout.PropertyField(serializedActions[schemeAction.Identifier.fullName]);
                                    serializedActions[schemeAction.Identifier.fullName].serializedObject.ApplyModifiedProperties();
                                }
                                catch (System.Exception e)
                                {
                                    EditorGUILayout.LabelField("Action Isn't Initialized in Editor: " + schemeAction.Identifier.properName);
                                    EditorGUILayout.HelpBox("Exception: " + e, MessageType.Warning);
                                }
                            }
                            EditorGUI.indentLevel -= 1;
                        }
                    }
                    EditorGUI.indentLevel -= 1;
                }
            }
        }
    }
}