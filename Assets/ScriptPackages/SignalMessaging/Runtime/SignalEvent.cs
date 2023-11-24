using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SignalMessaging
{
    [Serializable]
    public struct SignalEvent
    {
        [SerializeField] public string signalName;
        [SerializeField] public UnityEvent<string> signalEvent;

        public SignalEvent(string signalName, UnityEvent<string> signalEvent)
        {
            this.signalName = signalName;
            this.signalEvent = signalEvent;
        }
    }
}