using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;

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

    public bool TriggerSignal(string signalName)
    {
        bool triggered = false;
        foreach (SignalEvent signalEvent in signalEvents)
        {
            if (signalName.Equals(signalEvent.signalName))
            {
                triggered = true;
                signalEvent.signalEvent?.Invoke(signalName);
            }
        }
        return triggered;
    }

    public static void ParseSignalData(string signalName, 
        out Dictionary<string, float> floatValues, 
        out Dictionary<string, int> intValues, 
        out Dictionary<string, string> stringValues,
        out Dictionary<string, bool> boolValues)
    {
        floatValues = new Dictionary<string, float>();
        intValues = new Dictionary<string, int>();
        stringValues = new Dictionary<string, string>();
        boolValues = new Dictionary<string, bool>();

        // TODO: read in values based on signal data delim and operators
        // the signals will carry variables similar to URL parameters in the format
        // signalName?f:floatVar=2.1&i:intVar=2&s:stringVar=geaig&b:trueBoolVar=true
    }
}

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