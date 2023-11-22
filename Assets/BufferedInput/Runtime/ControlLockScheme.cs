/*
 * Author: Porter Squires
 */

using System.Collections.Generic;
using UnityEngine;

namespace BufferedInput
{
    [CreateAssetMenu(menuName = "Buffered Input/Scheme")]
    public class ControlLockScheme : ScriptableObject
    {
        [SerializeField] public List<ActionMask> possibleMasks;

        public void SetScheme(InputControlScheme scheme)
        {
            possibleMasks = new List<ActionMask>();
            foreach (ControlSchemeMap map in scheme.maps)
            {
                foreach (ControlSchemeAction action in map.actions)
                {
                    possibleMasks.Add(action.actionMask);
                }
            }
        }

        public bool Equals(ControlLockScheme scheme)
        {
            if (possibleMasks == null || scheme.possibleMasks == null)
                return possibleMasks == scheme.possibleMasks;

            if (possibleMasks.Count != scheme.possibleMasks.Count)
                return false;

            for (int i = 0; i < possibleMasks.Count; i++)
            {
                if (!possibleMasks[i].Equals(scheme.possibleMasks[i]))
                    return false;
            }

            return true;
        }
    }
}