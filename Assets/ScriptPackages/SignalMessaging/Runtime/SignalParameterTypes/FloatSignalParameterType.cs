using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SignalMessaging {
    [CreateAssetMenu(menuName = "Signal Messaging/Parameter Types/Float")]
    public class FloatSignalParameterType : SignalParameterType<float>
    {
        public override string SIG_TYPE_SYMBOL => "f";

        public override SignalParameter<float> ParseParameter(string paramName, string paramValueKey)
        {
            try
            {
                float value = float.Parse(paramValueKey);
                return new SignalParameter<float>(paramName, value, this);
            } catch
            {
                Debug.LogError("Error while parsing float signal parameter from: " + paramValueKey);
                return null;
            }
        }

        public override string WriteParameter(SignalParameter<float> param)
        {
            return SignalScribe.JoinParameterTypeNameValue(SIG_TYPE_SYMBOL, param.ParamName, param.Value.ToString());
        }
    }
}
