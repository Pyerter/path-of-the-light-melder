using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MaskedLocks
{
    [CustomEditor(typeof(MaskedLock))]
    public class MaskedLockEditor : Editor
    {
        bool currentKeysFoldout = false;
        bool keyToggleFoldout = true;

        public override void OnInspectorGUI()
        {
            bool markedDirty = false;
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                markedDirty = true;
            }

            MaskedLock myTarget = (MaskedLock)target;

            EditorGUILayout.LabelField("Number of Locks", myTarget.Count.ToString());
            EditorGUILayout.LabelField("Current mask", myTarget.Key.mask.ToString());
            EditorGUILayout.LabelField("Current Key", myTarget.Key.ToString());

            currentKeysFoldout = EditorGUILayout.Foldout(currentKeysFoldout, "Current Key Locks");
            if (currentKeysFoldout)
            {
                EditorGUI.indentLevel++;
                IReadOnlyList<MaskedLockKey> targetKeys = myTarget.Keys;
                for (int i = 0; i < myTarget.Keys.Count; i++)
                {
                    EditorGUILayout.LabelField(targetKeys[i].ToString());
                }
                EditorGUI.indentLevel--;
            }

            if (myTarget.LockScheme != null)
            {
                keyToggleFoldout = EditorGUILayout.Foldout(keyToggleFoldout, "Keys");
                if (keyToggleFoldout)
                {
                    EditorGUI.indentLevel++;
                    IReadOnlyList<MaskedLockKey> schemeKeys = myTarget.LockScheme.LockKeys;
                    for (int i = 0; i < schemeKeys.Count; i++)
                    {
                        MaskedLockKey currentKey = schemeKeys[i];
                        bool containsKey = myTarget.ContainsKey(currentKey.key);
                        bool keyToggle = EditorGUILayout.Toggle(currentKey.key, containsKey);
                        if (keyToggle != containsKey)
                        {
                            if (keyToggle)
                            {
                                myTarget.TryAddKey(currentKey.key);
                            }
                            else
                            {
                                myTarget.TryRemoveKey(currentKey.key);
                            }
                            markedDirty = true;
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }

            if (markedDirty)
            {
                myTarget.MarkCompositeKeyDirty();
                MaskedLockKey compKey = myTarget.CompositeKey;
                myTarget.ValidateLockKeys();
                EditorUtility.SetDirty(myTarget);
            }
        }
    }
}