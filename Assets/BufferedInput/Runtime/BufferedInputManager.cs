/*
 * Author: Porter Squires
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace BufferedInput
{
    [AddComponentMenu("Buffered Input/Buffered Input Manager")]
    [RequireComponent(typeof(ControlLockManager), typeof(PlayerInput))]
    public class BufferedInputManager : MonoBehaviour
    {
        [SerializeField] protected string uniqueAssetName = InputControlScheme.ASSET_NAME;

        [SerializeField] PlayerInput playerInput;
        [SerializeField] ControlLockManager controlLockManager;
        public ControlLockManager LockManager { get { return controlLockManager; } }
        [SerializeField] bool debugMessages = false;

        // name, id
        [SerializeField] public InputControlScheme inputControlScheme;
        [SerializeField] public ControlLockScheme controlLockScheme { get { return inputControlScheme != null ? inputControlScheme.LockScheme : null; } }
        protected Dictionary<string, UnityEvent<InputData>> cachedMapActions = null;

        [SerializeField][HideInInspector] Dictionary<string, Action<InputData>> inputEvents;
        [SerializeField] List<ActionMask> controlPriorityList = new List<ActionMask>();
        [SerializeField] int inputBufferDuration = 10;

        [SerializeField] bool inputBufferDebugMessages = false;
        InputDataBuffer inputBuffer = new InputDataBuffer();

        [SerializeField] public float playerVerticalAttackThreshold = 70f;

        private void Awake()
        {
            if (controlLockManager == null)
            {
                controlLockManager = GetComponent<ControlLockManager>();
            }

            if (inputControlScheme == null)
                InitializeActionMap();

            RemakeActionListeners();
            VerifyInputEvents();

            inputBuffer = new InputDataBuffer();
            inputBuffer.InitializeBuffers(inputControlScheme);
        }

        protected void RemoveExtraActionMaps()
        {
            InputControlScheme controlScheme = GetComponentInChildren<InputControlScheme>();
            while (controlScheme != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(controlScheme.gameObject);
                } else
                {
                    DestroyImmediate(controlScheme.gameObject);
                }
                controlScheme = GetComponentInChildren<InputControlScheme>();
            }
        }

        protected void CacheActionMapActions()
        {
            cachedMapActions = new Dictionary<string, UnityEvent<InputData>>();
            if (inputControlScheme != null)
            {
                foreach (ControlSchemeMap map in inputControlScheme.maps)
                {
                    foreach (ControlSchemeAction action in map.actions)
                    {
                        cachedMapActions[action.Identifier.properName] = action.InputEvent;
                    }
                }
            }
        }

        protected void ClearCachedActionMapActions()
        {
            cachedMapActions.Clear();
            cachedMapActions = null;
        }

        public void InitializeActionMap()
        {
            CacheActionMapActions();
            RemoveExtraActionMaps();
            Debug.Log("Initializing input control scheme.");
            if (!playerInput)
                playerInput = GetComponent<PlayerInput>();
            playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
            inputControlScheme = InputControlScheme.Create(transform, playerInput.actions, playerInput, uniqueAssetName);
            Debug.Log("Done initializing input control scheme: " + inputControlScheme);
            inputEvents = new Dictionary<string, Action<InputData>>();
            foreach (ControlSchemeMap map in inputControlScheme.maps)
            {
                foreach (ControlSchemeAction action in map.actions)
                {
                    action.OnInput.AddListener(AcceptInput);
                    inputEvents.Add(action.actionMask.name, (input) => action.TriggerEvent(input));
                    if (cachedMapActions != null && cachedMapActions.TryGetValue(action.Identifier.properName, out UnityEvent<InputData> inputEvent))
                    {
                        action.InputEvent = inputEvent;
                    }
                }
            }
            ClearCachedActionMapActions();
            ResetPriorityList();
            Debug.Log("Created input control scheme: " + inputControlScheme);
            // TODO: Add default directional action mask
        }

        public void RemakeActionListeners()
        {
            foreach (ControlSchemeMap map in inputControlScheme.maps)
            {
                foreach (ControlSchemeAction action in map.actions)
                {
                    action.OnInput.AddListener(AcceptInput);
                }
            }
        }

        public void VerifyInputEvents()
        {
            if (inputEvents == null)
            {
                inputEvents = new Dictionary<string, Action<InputData>>();
                foreach (ControlSchemeMap map in inputControlScheme.maps)
                {
                    foreach (ControlSchemeAction action in map.actions)
                    {
                        inputEvents.Add(action.actionMask.name, (input) => action.TriggerEvent(input));
                    }
                }
            }
        }

        public void ResetPriorityList()
        {
            controlPriorityList = new List<ActionMask>();
            foreach (ControlSchemeMap map in inputControlScheme.maps)
            {
                foreach (ControlSchemeAction action in map.actions)
                {
                    controlPriorityList.Add(action.actionMask);
                }
            }
        }

        void FixedUpdate()
        {
            // update debug messages variable
            inputBuffer.debugMessages = inputBufferDebugMessages;
            // cache the current directional input
            inputBuffer.CacheDirectional();

            // iterate over each control in the priority list (in the priority order)
            foreach (ActionMask control in controlPriorityList)
            {
                // try to get the buffer for each control (and check if buffer has any inputs)
                if (inputBuffer.TryGetBuffer(control, out List<InputPair> buffer) && buffer.Count > 0)
                {
                    // check if the input is locked
                    bool canInput = CanInput(control, out string debugStr);
                    // run the maintenance method for the buffer (merging sequential inputs, removing expired and interrupted inputs)
                    inputBuffer.MaintainBuffer(buffer, control, true);
                    // attempt to grab the earliest buffered input
                    if (inputBuffer.PeekNextInput(buffer, out InputData input))
                    {
                        // if the input is allowed and not interrupted
                        if (canInput && !input.IsInterrupted() && inputBuffer.TryPopNextInput(buffer, out input))
                        {
                            // invoke the input event
                            inputEvents[control.name].Invoke(input);
                            // assign the input process stage to be processing
                            input.ProcessStage = InputProcessStage.PROCESSING;
                            DebugAllowedInput(debugStr + ", marking input as processed, " + input.ToString(), input);
                        }
                        else
                        {
                            // if it was processing before (and isn't directional, bc directionals can't be interrupted)
                            if (input.IsProcessing() && input.Interruptible)
                            {
                                // update the processing stage to be interrupted
                                input.ProcessStage = InputProcessStage.INTERRUPTED;
                            }
                            DebugAllowedInput(debugStr + ", " + input.ToString(), input);
                        }
                    }
                    else
                        DebugAllowedInput(debugStr, null);
                }
            }
        }

        public void AcceptInput(InputData input)
        {
            TryDebug(input, "Accepted input: " + input.ToString());
            inputBuffer.AcceptInput(input, inputBufferDuration);
            inputBuffer.CleanBuffer(input.Mask);
        }

        public bool CanInput(ActionMask mask, out string debugStr)
        {
            bool controlsAllowed = controlLockManager.ControlsAllowed(mask);
            if (debugMessages)
            {
                string output = "Control \"" + mask.name + (controlsAllowed ? "\" allowed." : "\" not allowed.");
                debugStr = output;
            }
            else
            {
                debugStr = "";
            }
            return controlsAllowed;
        }

        public Vector2 GetCachedDirection()
        {
            return inputBuffer.GetCachedDirection();
        }

        public Vector2 GetCurrentDirection()
        {
            return inputBuffer.CurrentDirection();
        }

        public bool CacheDirectional()
        {
            inputBuffer.CacheDirectional();
            return true;
        }

        public Vector2 GetCachedDirection(ActionMask mask)
        {
            return inputBuffer.GetCachedDirection(mask);
        }

        public Vector2 GetCurrentDirection(ActionMask mask)
        {
            return inputBuffer.CurrentDirection(mask);
        }

        public bool CacheDirectional(ActionMask mask)
        {
            inputBuffer.CacheDirectional(mask);
            return true;
        }

        public InputData GetCachedInput(ActionMask mask)
        {
            inputBuffer.TryGetCachedInput(mask, out InputData input);
            return input;
        }

        public bool TryGetCachedInput(ActionMask mask, out InputData input)
        {
            return inputBuffer.TryGetCachedInput(mask, out input);
        }

        public bool BufferedInputExists(ActionMask mask)
        {
            return inputBuffer.TryGetBuffer(mask, out List<InputPair> buffer) && buffer.Count > 0;
        }

        public bool BufferedInputExists(ActionMask mask, out InputData input)
        {
            if (inputBuffer.TryGetBuffer(mask, out List<InputPair> buffer) && buffer.Count > 0)
            {
                input = buffer[0].input;
                return true;
            }
            input = null;
            return false;
        }

        private void DebugAllowedInput(string debugStart, InputData input)
        {
            if (!debugMessages)
                return;
            if (input == null)
                Debug.Log(debugStart);
            else if (!input.Mask.name.ToLower().Contains("ui"))
                Debug.Log(debugStart + " Input phase: " + input.Phase.ToString());
        }

        public void TryDebug(InputData input, string content)
        {
            if (debugMessages && (input == null || !input.Mask.name.ToLower().Contains("ui")))
                Debug.Log("Player Input Manager: " + content);
        }
    }
}