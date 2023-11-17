/*
 * Author: Porter Squires
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BufferedInput
{
    [AddComponentMenu("Buffered Input/Miscellaneous/Buffered Input Control Scheme")]
    public class InputControlScheme : MonoBehaviour
    {
        public const string ASSET_PATH = "Assets/Settings/BufferedInput";
        public const string ASSET_NAME = "BufferedInputControlScheme";
        public const int MAX_MASKS = 64; // longs have 64 bits
        [SerializeField][HideInInspector] public List<ControlSchemeMap> maps;
        [SerializeField] public Dictionary<string, ControlSchemeAction> actionDict;
        [SerializeField][HideInInspector] private List<ActionMask> unassignedMasks;
        [SerializeField] PlayerInput playerInput;
        [SerializeField] ControlLockScheme lockScheme;
        public ControlLockScheme LockScheme { get { return lockScheme; } }

        private void Awake()
        {
            foreach (PlayerInput.ActionEvent actionEvent in playerInput.actionEvents)
            {
                ActionEventIdentifier control = new ActionEventIdentifier(actionEvent.actionName, actionEvent.actionId);
                try
                {
                    ControlSchemeAction action = FindAction(control);
                    if (action != null)
                    {
                        actionEvent.AddListener(action.EventListener);
                        Debug.Log("Successfully added LISTENER for action --- " + action.ToString());
                    } else
                    {
                        Debug.LogWarning("Action was null for control name: " + control.properName);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Error while adding listener for Scheme Action \"" + control.ToString() + ": " + e.Message);
                    Debug.LogWarning(e.GetType().ToString() + ":::" + e.StackTrace);
                }
            }
        }

        public ControlSchemeAction FindAction(ActionEventIdentifier target)
        {
            foreach (ControlSchemeMap map in maps)
            {
                foreach (ControlSchemeAction action in map.actions)
                {
                    if (action.Identifier.Equals(target))
                        return action;
                }
            }
            return null;
        }

        public static InputControlScheme Create(Transform parent, InputActionAsset actions, PlayerInput playerInput, string uniqueAssetName)
        {
            GameObject gameObject = new GameObject();
            InputControlScheme target = gameObject.AddComponent<InputControlScheme>();
            target.Initialize(actions, playerInput, uniqueAssetName);
            gameObject.transform.parent = parent;
            gameObject.name = "Input Control Scheme - " + actions.name;
            //gameObject.hideFlags = HideFlags.HideInHierarchy;
            return target;
        }

        protected void Initialize(InputActionAsset actions, PlayerInput playerInput, string uniqueAssetName)
        {
            InitializeMaskPool();
            InitializeActionMaps(actions, playerInput);
            InitializeControlLockScheme(uniqueAssetName);
        }

        protected void InitializeActionMaps(InputActionAsset actions, PlayerInput playerInput)
        {
            this.playerInput = playerInput;
            maps = new List<ControlSchemeMap>();
            actionDict = new Dictionary<string, ControlSchemeAction>();
            foreach (PlayerInput.ActionEvent actionEvent in playerInput.actionEvents)
            {
                ActionEventIdentifier control = new ActionEventIdentifier(actionEvent.actionName, actionEvent.actionId);
                InputAction iAction = playerInput.actions.FindActionMap(control.mapName).FindAction(control.actionName);
                Debug.Log("Map: " + control.mapName + ", Action: " + control.actionName + "\nExpected control type: " + iAction.expectedControlType);
                int mapIndex = maps.FindIndex(m => m.MapName.Equals(control.mapName));
                if (mapIndex < 0) {
                    mapIndex = maps.Count;
                    maps.Add(ControlSchemeMap.Create(transform, control.mapName)); // new ControlSchemeMap(control.mapName));
                }
                ControlSchemeMap map = maps[mapIndex];
                try
                {
                    if (!GetNextMask(out ActionMask mask))
                        throw new Exception("Out of action masks!");
                    ControlSchemeAction action = ControlSchemeAction.Create(map.transform, iAction, control, mask);
                    map.actions.Add(action);
                    actionDict.TryAdd(action.Identifier.name, action);
                    actionEvent.RemoveAllListeners();
                    actionEvent.AddListener(action.EventListener);
                    Debug.Log("Successfully added action --- " + action.ToString());
                } catch (Exception e)
                {
                    Debug.LogWarning("Error while creating Control Scheme Action \"" + control.ToString() + ": " + e.Message);
                }
            }
        }

        protected void InitializeControlLockScheme(string uniqueAssetName)
        {
#if UNITY_EDITOR
            string[] existingSchemes = UnityEditor.AssetDatabase.FindAssets("t:" + uniqueAssetName);
            lockScheme = ScriptableObject.CreateInstance<ControlLockScheme>();
            lockScheme.SetScheme(this);
            string targetPath = ASSET_PATH + '/' + uniqueAssetName + ".asset";
            bool usingExisting = false;
            string[] assetPaths = new string[existingSchemes.Length];
            for (int i = 0; i < existingSchemes.Length; i++)
            {
                assetPaths[i] = UnityEditor.AssetDatabase.GUIDToAssetPath(existingSchemes[i]);
                ControlLockScheme tempLockScheme = (ControlLockScheme)UnityEditor.AssetDatabase.LoadAssetAtPath(assetPaths[i], typeof(ControlLockScheme));
                if (tempLockScheme.Equals(lockScheme))
                {
                    lockScheme = tempLockScheme;
                    usingExisting = true;
                    break;
                } else if (assetPaths[i].Equals(targetPath))
                {
                    UnityEditor.AssetDatabase.DeleteAsset(assetPaths[i]);
                }
            }

            if (!usingExisting)
            {
                try
                {
                    string[] paths = ASSET_PATH.Split('/');
                    string currentFolder = paths[0];
                    for (int i = 1; i < paths.Length; i++)
                    {
                        string nextFolder = currentFolder + '/' + paths[i];
                        if (!UnityEditor.AssetDatabase.IsValidFolder(nextFolder))
                            UnityEditor.AssetDatabase.CreateFolder(currentFolder, paths[i]);
                        currentFolder = nextFolder;
                    }
                    UnityEditor.AssetDatabase.CreateAsset(lockScheme, targetPath);
                    Debug.Log("Successfully generated control scheme to path: \"" + targetPath + "\"");
                } catch (UnityException)
                {
                    Debug.LogError("Control scheme failed to create. Error while asserting folder pathway: \"" + targetPath + "\"");
                }
            }
#endif
        }

#region Mask Pool Methods
        private void InitializeMaskPool()
        {
            unassignedMasks = new List<ActionMask>();
            for (int i = 0; i < MAX_MASKS; i++)
            {
                unassignedMasks.Add(new ActionMask(1 << i));
            }
        }
        public bool GetNextMask(out ActionMask mask)
        {
            if (unassignedMasks.Count == 0)
            {
                mask = ActionMask.NONE;
                return false;
            }
            mask = unassignedMasks[0];
            unassignedMasks.RemoveAt(0);
            return true;
        }

        public bool ReturnMask(ActionMask mask)
        {
            int possibleDup = unassignedMasks.FindIndex(m => m.Mask == mask.Mask);
            if (possibleDup < 0)
            {
                unassignedMasks.Add(mask);
                return true;
            }
            return false;
        }
#endregion
    }
}
