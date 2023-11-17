/*
 * Author: Porter Squires
 */

using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace BufferedInput
{
    [Serializable]
    [AddComponentMenu("Buffered Input/Miscellaneous/Control Scheme Action")]
    public class ControlSchemeAction : MonoBehaviour //: UnityEngine.Object //: ScriptableObject
    {
        [SerializeField] private bool interruptible;
        [SerializeField] public bool Interruptible { get { return interruptible; } }

        [SerializeField] private InputValueTypes valueType;
        public InputValueTypes ValueType { get { return valueType; } }
        [SerializeField] private ActionEventIdentifier identifier;
        public ActionEventIdentifier Identifier { get { return identifier; } }
        [SerializeField] public ActionMask actionMask;
        [SerializeField] private bool actionMaskInitialized = false;
        [SerializeField] private UnityEvent<InputData> inputEvent;
        [SerializeField] public UnityEvent<InputData> InputEvent { get { return inputEvent; } set { inputEvent = value; } }
        [SerializeField][HideInInspector] private UnityEvent<InputData> onInput;
        [SerializeField] public UnityEvent<InputData> OnInput { get { return onInput; } set { onInput = value; } }

        public static ControlSchemeAction Create(Transform target, InputAction refAction, ActionEventIdentifier identifier, ActionMask mask)
        {
            ControlSchemeAction action = target.gameObject.AddComponent<ControlSchemeAction>();
            action.Initialize(refAction, identifier, mask);
            return action;
        }

        //public ControlSchemeAction(InputAction refAction, ActionEventIdentifier identifier)
        protected void Initialize(InputAction refAction, ActionEventIdentifier identifier, ActionMask mask)
        {
            if (refAction.type == InputActionType.Button || refAction.expectedControlType == "Button")
                valueType = InputValueTypes.BUTTON;
            else if (refAction.expectedControlType == "Vector2")
                valueType = InputValueTypes.VECTOR2;
            else
                throw new InvalidOperationException("The InputAction given as reference does not contain a Button or Vector2 action type.");

            this.identifier = identifier;
            InitializeActionMask(mask);
            inputEvent = new UnityEvent<InputData>();
            onInput = new UnityEvent<InputData>();
        }

        public bool InitializeActionMask(ActionMask mask)
        {
            if (!actionMaskInitialized)
            {
                this.actionMask = mask;
                this.actionMask.name = identifier.properName;
                actionMaskInitialized = true;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            return "Control Scheme Action: " + identifier.name + ", value type: " + valueType.ToString() + ", mask: " + actionMask.ToString();
        }

        public InputData AcceptInput(InputAction.CallbackContext ctxt)
        {
            Vector2 vec = Vector2.zero;
            InputStage phase = ctxt.started || ctxt.performed ? InputStage.HELD : InputStage.RELEASED;
            if (valueType == InputValueTypes.VECTOR2)
            {
                vec = ctxt.ReadValue<Vector2>();
                //Debug.Log("Read vector input: " + vec.ToString());
            }
            InputData input = new InputData(actionMask, valueType, interruptible, phase, vec);
            return input;
        }

        public void EventListener(InputAction.CallbackContext ctxt)
        {
            // Debug.Log("Listened to input: " + ctxt);
            InputData input = AcceptInput(ctxt);
            // Debug.Log("Created input data: " + input);
            onInput?.Invoke(input);
        }

        public void TriggerEvent(InputData input)
        {
            inputEvent?.Invoke(input);
        }
    }

    [Serializable]
    public struct ActionMask
    {
        public static int MAX_MASKS = 64;
        public static ActionMask ALL = new ActionMask(~0, "ALL");
        public static ActionMask NONE = new ActionMask(0, "NONE");

        [SerializeField] private long mask;
        [SerializeField] public string name;
        public long Mask { get { return mask; } }
        public bool Assigned { get { return this.name.Equals("Unassigned"); } }
        public ActionMask(long mask)
        {
            this.mask = mask;
            this.name = "Unassigned";
        }
        public ActionMask(long mask, string name)
        {
            this.mask = mask;
            this.name = name;
        }
        public ActionMask(ActionMask actionMask)
        {
            this.mask = actionMask.mask;
            this.name = actionMask.name;
        }

        public void Unassign()
        {
            this.name = "Unassigned";
        }

        public override string ToString()
        {
            return "" + mask + " = " + Convert.ToString(mask, 2).PadLeft(64, '0');
        }

        public bool Equals(ActionMask am)
        {
            return am.mask == mask && am.name.Equals(name);
        }
    }

    [Serializable]
    public struct ActionEventIdentifier
    {
        [SerializeField] public string fullName;
        [SerializeField] public string name;
        [SerializeField] public string id;
        [SerializeField] public string mapName;
        [SerializeField] public string actionName;
        [SerializeField] public string properName;
        public ActionEventIdentifier(string fullName, string id)
        {
            this.fullName = fullName;
            int cutoff = fullName.IndexOf('[');
            this.name = cutoff > 0 ? fullName.Substring(0, cutoff) : fullName;
            int mapSplit = this.name.IndexOf("/");
            this.mapName = this.name.Substring(0, mapSplit);
            this.actionName = this.name.Substring(mapSplit + 1);
            this.id = id;
            this.properName = this.mapName + "/" + this.actionName;
        }
        public override string ToString()
        {
            return "Action Event Control: " + name + " ----- id: " + id + ", full name: " + fullName;
        }

        public bool Equals(ActionEventIdentifier identifier)
        {
            return fullName.Equals(identifier.fullName);
        }
    }

    public enum InputValueTypes
    {
        BUTTON,
        VECTOR2
    }
}