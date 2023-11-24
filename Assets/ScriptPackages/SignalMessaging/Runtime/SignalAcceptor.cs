using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;

namespace SignalMessaging
{
    // This class will be used to convey signals.
    // Signals will be passed by attack data to entities.
    // Entities can then accept a signal and, if they have the event, react accordingly.
    // Signals will also be able to pass parameters.
    // For example, if I want to knockback an enemy with a certain force, I can have the
    // attack data pass the "knockback" signal with a force float parameter. Any enemy that
    // can be knocked back will accept this signal and react accordingly.
    public class SignalAcceptor : MonoBehaviour
    {
        public const string SIG_DATA_PARAM_DELIM = "?";
        public const string SIG_DATA_PARAM_OP = "&";
        public const string SIG_DATA_NAME_OP = ":";
        public const string SIG_DATA_ASSIGN_OP = "=";
        public const string SIG_DATA_FLOAT_SYMBOL = "f";
        public const string SIG_DATA_INT_SYMBOL = "i";
        public const string SIG_DATA_STRING_SYMBOL = "s";
        public const string SIG_DATA_BOOL_SYMBOL = "b";
        public static readonly string[] SIG_DATA_BOOL_SYMBOLS_TRUE = { "t", "true" };
        public static readonly string[] SIG_DATA_BOOL_SYMBOLS_FALSE = { "f", "false" };

        [SerializeField] protected List<SignalEvent> signalEvents = new List<SignalEvent>();
        public IReadOnlyList<SignalEvent> SignalEvents { get { return signalEvents.AsReadOnly(); } }

        public bool TriggerSignal(SignalData signalData)
        {
            bool triggered = false;
            foreach (SignalEvent signalEvent in signalEvents)
            {
                //Debug.Log("Attempting to trigger signal " + signalEvent.SignalName + " with signal data " + signalData.SignalName);
                if (signalData.TryTriggerEvent(signalEvent))
                    triggered = true;
            }
            return triggered;
        }
    }
}