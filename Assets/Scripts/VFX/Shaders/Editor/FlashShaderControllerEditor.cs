using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FlashShaderController))]
public class FlashShaderControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        FlashShaderController myTarget = (FlashShaderController)target;

        if (Application.isPlaying && GUILayout.Button("Trigger Flash"))
        {
            myTarget.Flash();
        }
    }
}
