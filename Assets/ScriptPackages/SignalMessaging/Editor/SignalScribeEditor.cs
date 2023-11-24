using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace SignalMessaging
{
    [CustomEditor(typeof(SignalScribe))]
    public class SignalScribeEditor : Editor
    {
        SignalScribeManager scribeManager;
        Scene currentScene;
        bool currentSceneLoaded = false;
        bool currentSceneValid = false;
        bool hasScribeManager = false;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SignalScribe myTarget = (SignalScribe)target;
            myTarget.CheckInitialize();

            if (!currentSceneLoaded)
            {
                Scene currentScene = SceneManager.GetActiveScene();
                if (!currentScene.isSubScene && currentScene.rootCount > 0)
                {
                    GameObject[] objects = currentScene.GetRootGameObjects();
                    foreach (GameObject obj in objects)
                    {
                        if (obj.TryGetComponent<SignalScribeManager>(out scribeManager))
                        {
                            hasScribeManager = true;
                        }
                    }
                    currentSceneValid = true;
                }
                currentSceneLoaded = true;
            }

            if (currentSceneValid && !hasScribeManager)
            {
                EditorGUILayout.HelpBox("No ScribeManager in current scene, signals will not work. Click \"Create ScribeManager\"", MessageType.Warning);
                if (GUILayout.Button("Create ScribeManager"))
                {
                    try
                    {
                        GameObject go = new GameObject("Scribe Manager");
                        //go.hideFlags = HideFlags.HideInInspector;
                        SignalScribeManager scribeManager = go.AddComponent<SignalScribeManager>();
                        hasScribeManager = scribeManager.Init(myTarget);
                    }
                    catch
                    {

                    }
                }
            }
        }
    }
}
