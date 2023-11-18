/*
 * Author: Porter Squires
 */

using System.Collections.Generic;
using UnityEditor;

namespace BufferedInput
{
    [CustomEditor(typeof(StandardControlLocker))]
    public class StandardControlLockerEditor : Editor
    {
        bool[] toggles = null;
        string currentName = "";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            StandardControlLocker myTarget = (StandardControlLocker)target;
            if (myTarget.LockScheme == null)
                return;

            if (toggles == null)
            {
                toggles = new bool[myTarget.LockScheme.possibleMasks.Count];
                for (int i = 0; i < toggles.Length; i++)
                {
                    toggles[i] = myTarget.Lock.IsLocking(myTarget.LockScheme.possibleMasks[i]);
                }
            }

            if (string.IsNullOrEmpty(currentName))
                currentName = myTarget.Lock.Name;

            currentName = EditorGUILayout.TextField("Name", currentName);

            EditorGUILayout.Space(10);

            bool changedToggle = false;
            for (int i = 0; i < toggles.Length; i++)
            {
                EditorGUILayout.Space(5);
                bool temp = toggles[i];
                toggles[i] = EditorGUILayout.Toggle(myTarget.LockScheme.possibleMasks[i].name, toggles[i]);
                if (toggles[i] != temp)
                    changedToggle = true;
            }

            if (!currentName.Equals(myTarget.Lock.Name))
                changedToggle = true;

            if (changedToggle)
                UpdateToggles(myTarget, currentName);
        }

        protected void UpdateToggles(StandardControlLocker myTarget, string lockName)
        {
            List<ActionMask> usedToggles = new List<ActionMask>();
            for (int i = 0; i < toggles.Length; i++)
            {
                if (toggles[i])
                {
                    usedToggles.Add(myTarget.LockScheme.possibleMasks[i]);
                }
            }
            myTarget.Lock.SetComposite(usedToggles, lockName);
            EditorUtility.SetDirty(myTarget);
        }
    }
}