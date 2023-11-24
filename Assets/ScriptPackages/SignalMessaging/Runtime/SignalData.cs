using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SignalMessaging
{
    public struct SignalData
    {
        [SerializeField] public Dictionary<string, float> floatValues;
        [SerializeField] public Dictionary<string, int> intValues;
        [SerializeField] public Dictionary<string, string> stringValues;
        [SerializeField] public Dictionary<string, bool> boolValues;

        public SignalData(string signalName)
        {
            SignalAcceptor.ParseSignalData(signalName, out floatValues, out intValues, out stringValues, out boolValues);
        }

        public bool TryReadValue<T>(string key, out T value)
        {
            value = ReadValue<T>(key, out bool success);
            return success;
        }

        public T ReadValue<T>(string key)
        {
            return ReadValue<T>(key, out bool success);
        }

        public T ReadValue<T>(string key, out bool success)
        {
            // TODO: create a class like SignalParameter that will allow for the usage of custom parameter types
            // For example, reading a Vector2, the SignalParameter for Vector2 would provide a unique signal data symbol
            // and a parsing function.
            // To accomodate this, SignalData will need to be changed to instead store a Dictionary<string, Dictionary<string, type>>
            // where the dictionary maps a type string to dictionaries that store certain types.
            success = false;
            Type varType = typeof(T);
            if (varType == typeof(float))
            {
                float val = 0f;
                if (floatValues.TryGetValue(key, out float value))
                {
                    val = value;
                    success = true;
                }
                return (T)Convert.ChangeType(val, typeof(T));
            }
            if (varType == typeof(int))
            {
                int val = 0;
                if (intValues.TryGetValue(key, out int value))
                {
                    val = value;
                    success = true;
                }
                return (T)Convert.ChangeType(val, typeof(T));
            }
            if (varType == typeof(string))
            {
                string val = "";
                if (stringValues.TryGetValue(key, out string value))
                {
                    val = value;
                    success = true;
                }
                return (T)Convert.ChangeType(val, typeof(T));
            }
            if (varType == typeof(bool))
            {
                bool val = false;
                if (boolValues.TryGetValue(key, out bool value))
                {
                    val = value;
                    success = true;
                }
                return (T)Convert.ChangeType(val, typeof(T));
            }
            Debug.LogError("Tried reading a value from SignalData of type " + varType.ToString() + " when that type does not match float, int, string, or bool.");
            return default;
        }
    }
}