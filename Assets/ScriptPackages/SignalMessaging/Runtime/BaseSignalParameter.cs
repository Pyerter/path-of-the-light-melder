using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SignalMessaging
{
    public abstract class BaseSignalParameter
    {
        public static bool TryGetSignalParameter<T>(BaseSignalParameter baseSignalParameter, out SignalParameter<T> signalParameter)
        {
            signalParameter = GetSignalParameter<T>(baseSignalParameter);
            return signalParameter != null;
        }

        public static SignalParameter<T> GetSignalParameter<T>(BaseSignalParameter baseSignalParameter)
        {
            return baseSignalParameter as SignalParameter<T>;
        }

        public abstract BaseSignalParameterType BaseParamType { get; }

        [SerializeField] protected string paramName;
        public string ParamName { get { return paramName; } }

        public abstract string WriteParam();
    }
}