using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SignalMessaging
{
    [Serializable]
    public class SignalEvent
    {
        [SerializeField] protected string signalName;
        public string SignalName { get { return signalName; } }
        [SerializeField] protected UnityEvent<SignalData> signalDataEvent;
        public UnityEvent<SignalData> SignalDataEvent { get { return signalDataEvent; } }

        public SignalEvent(string signalName, UnityEvent<SignalData> signalDataEvent)
        {
            this.signalName = signalName;
            this.signalDataEvent = signalDataEvent;
        }
    }
}