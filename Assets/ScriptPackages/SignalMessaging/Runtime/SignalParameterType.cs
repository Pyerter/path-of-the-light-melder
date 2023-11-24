using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SignalMessaging
{
    public abstract class SignalParameterType<T> : BaseSignalParameterType
    {
        public override Type ParamType { get { return typeof(T); } }

        public abstract SignalParameter<T> ParseParameter(string paramName, string paramValueKey);
        public abstract string WriteParameter(SignalParameter<T> param);

        public override BaseSignalParameter ParseBaseParameter(string paramName, string paramValueKey)
        {
            /*SignalParameter<T> param = ParseParameter(paramName, paramValueKey);
            BaseSignalParameter baseParam = param;
            SignalParameter<T> twistedParam = baseParam as SignalParameter<T>;
            Debug.Log("Parameter casted to " + typeof(T) + ": " + (twistedParam.BaseParamType.ParamType).ToString());*/
            return ParseParameter(paramName, paramValueKey);
        }
    }
}