using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace SignalMessaging
{
    public class SignalScribeManager : MonoBehaviour
    {
        protected static SignalScribeManager instance;
        public static SignalScribeManager Instance { get { if (instance == null) instance = FindObjectOfType<SignalScribeManager>(); return instance; } }

        [SerializeField] protected SignalScribe scribe;
        public SignalScribe Scribe { get { if (scribe == null) scribe = SignalScribe.Instance; return scribe; } }

        public bool Init(SignalScribe scribe)
        {
            if (this.scribe == null)
            {
                this.scribe = scribe;
                return true;
            }
            return false;
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            } else if (instance != this)
            {
                Debug.LogWarning("SignalScribeManager already exists, destroying new one...");
                Destroy(gameObject);
            }

            SignalScribe scribe = Scribe;
            Scribe.ApplySettingsToInstance();
            scribe.CheckInitialize();
        }
    }
}