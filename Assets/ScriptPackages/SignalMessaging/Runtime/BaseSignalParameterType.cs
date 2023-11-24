using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SignalMessaging
{
    public abstract class BaseSignalParameterType : ScriptableObject
    {
        public static bool TryGetSignalParameterType<T>(BaseSignalParameterType baseSignalParameter, out SignalParameterType<T> signalParameterType)
        {
            signalParameterType = GetSignalParameterType<T>(baseSignalParameter);
            return signalParameterType != null;
        }

        public static SignalParameterType<T> GetSignalParameterType<T>(BaseSignalParameterType baseSignalParameter)
        {
            return baseSignalParameter as SignalParameterType<T>;
        }

        public abstract Type ParamType { get; }
        public abstract string SIG_TYPE_SYMBOL { get; }
        public abstract BaseSignalParameter ParseBaseParameter(string paramName, string paramValueKey);
    }
}