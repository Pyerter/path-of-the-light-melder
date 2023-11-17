/*
 * Author: Porter Squires
 */

using System.Collections.Generic;
using UnityEngine;
using System;

namespace BufferedInput
{
    [Serializable]
    [AddComponentMenu("Buffered Input/Miscellaneous/Control Scheme Map")]
    public class ControlSchemeMap : MonoBehaviour
    {
        [SerializeField] private string mapName;
        [SerializeField] public List<ControlSchemeAction> actions;
        public string MapName { get { return mapName; } }

        public static ControlSchemeMap Create(Transform parent, string mapName)
        {
            GameObject gameObject = new GameObject();
            ControlSchemeMap target = gameObject.AddComponent<ControlSchemeMap>();
            target.Initialize(mapName);
            gameObject.transform.parent = parent;
            gameObject.name = "Input Control Scheme Map - " + mapName;
            //gameObject.hideFlags = HideFlags.HideInHierarchy;
            return target;
        }

        protected void Initialize(string mapName)
        {
            this.mapName = mapName;
            actions = new List<ControlSchemeAction>();
        }
    }
}