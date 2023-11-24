/*
 * Author: Porter Squires
 */

using UnityEditor;

namespace BufferedInput {
    [CustomEditor(typeof(ControlLockManager))]
    public class ControlLockManagerEditor : Editor
    {
        bool locksFoldout = false;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ControlLockManager myTarget = (ControlLockManager)target;

            locksFoldout = EditorGUILayout.Foldout(locksFoldout, "Current Locks");
            if (locksFoldout)
            {
                EditorGUI.indentLevel++;

                if (myTarget.activeLockers.Count == 0)
                    EditorGUILayout.LabelField("No Locks");

                foreach (ControlLocker locker in myTarget.activeLockers)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(locker.Lock.Locks.name + ": " + locker.Lock.Locks.ToStringFormatted(32));
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;
            }
        }
    }
}