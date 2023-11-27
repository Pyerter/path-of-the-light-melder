using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SignalMessaging
{
    public class SignalData
    {
        [SerializeField] protected string signalName;
        public string SignalName { get { return signalName; } }
        [SerializeField] protected Dictionary<string, BaseSignalParameter> valueDict;
        public Dictionary<string, BaseSignalParameter>.KeyCollection ValueDictKeys { get { return valueDict.Keys; } }
        public Dictionary<string, BaseSignalParameter>.ValueCollection ValueDictValues { get { return  valueDict.Values; } }

        public SignalData(string signalName, List<BaseSignalParameter> parameters)
        {
            this.signalName = signalName;
            valueDict = new Dictionary<string, BaseSignalParameter>();
            foreach (BaseSignalParameter param in parameters)
            {
                valueDict.TryAdd(param.ParamName, param);
            }
        }

        public static SignalData ReadSignalData(string signal)
        {
            SignalScribe scribe = SignalScribe.Instance;
            if (scribe == null)
            {
                Debug.LogWarning("Trying to read signal data but no SignalScribe has claimed itself as the SignalScribe.Instance.");
                return null;
            }
            return scribe.ParseSignal(signal);
        }

        public bool TryReadParameter<T>(string key, out SignalParameter<T> typedParam)
        {
            key = key.ToLower();
            typedParam = default;

            bool valueExists = valueDict.TryGetValue(key, out BaseSignalParameter param);
            if (!valueExists) return false;

            bool typeMatches = BaseSignalParameter.TryGetSignalParameter<T>(param, out typedParam);
            if (!typeMatches)
                Debug.LogWarning("Trying to read value " + key + " from SignalData, but type " + typeof(T).ToString() + " does not match real type: " + param.BaseParamType.ParamType.ToString());
            return typeMatches;
        }
        
        public bool TryReadValue<T>(string key, out T value)
        {
            value = default;

            bool paramExists = TryReadParameter<T>(key, out SignalParameter<T> typedParam);
            if (!paramExists) return false;

            value = typedParam.Value;
            return true;
        }

        public T ReadValue<T>(string key)
        {
            TryReadValue<T>(key, out T value);
            return value;
        }

        public T ReadValue<T>(string key, T defaultValue)
        {
            if (TryReadValue(key, out T value))
                return value;
            return defaultValue;
        }

        public bool TryTriggerEvent(SignalEvent signalEvent)
        {
            if (SignalName.Equals(signalEvent.SignalName.ToLower()) && signalEvent.SignalDataEvent != null)
            {
                signalEvent.SignalDataEvent.Invoke(this);
                return true;
            }
            return false;
        }
    }
}