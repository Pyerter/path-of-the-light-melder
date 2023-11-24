using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SignalMessaging {
    [CreateAssetMenu(menuName = "Signal Messaging/Parameter Types/Integer")]
    public class IntSignalParameterType : SignalParameterType<int>
    {
        public override string SIG_TYPE_SYMBOL => "i";

        public override SignalParameter<int> ParseParameter(string paramName, string paramValueKey)
        {
            try
            {
                int value = int.Parse(paramValueKey);
                return new SignalParameter<int>(paramName, value, this);
            } catch
            {
                Debug.LogError("Error while parsing int signal parameter from: " + paramValueKey);
                return null;
            }
        }

        public override string WriteParameter(SignalParameter<int> param)
        {
            return SignalScribe.JoinParameterTypeNameValue(SIG_TYPE_SYMBOL, param.ParamName, param.Value.ToString());
        }
    }
}
