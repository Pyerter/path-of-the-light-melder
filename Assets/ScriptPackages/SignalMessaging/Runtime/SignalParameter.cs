using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SignalMessaging
{
    [System.Serializable]
    public class SignalParameter <T> : BaseSignalParameter
    {
        [SerializeField] protected SignalParameterType<T> paramType;
        public SignalParameterType<T> ParamType { get { return paramType; } }
        public override BaseSignalParameterType BaseParamType { get { return paramType; } }

        [SerializeField] protected T value;
        public T Value { get { return value; } }

        public SignalParameter(string paramName, T value, SignalParameterType<T> paramType)
        {
            this.paramName = paramName;
            this.value = value;
            this.paramType = paramType;
        }

        public override string WriteParam()
        {
            return paramType.WriteParameter(this);
        }
    }
}