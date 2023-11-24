using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SignalMessaging
{
    [CreateAssetMenu(menuName = "Signal Messaging/Parameter Types/String")]
    public class StringSignalParameterType : SignalParameterType<string>
    {
        public override string SIG_TYPE_SYMBOL => "s";

        public override SignalParameter<string> ParseParameter(string paramName, string paramValueKey)
        {
            try
            {
                return new SignalParameter<string>(paramName, paramValueKey, this);
            }
            catch
            {
                Debug.LogError("Error while parsing string signal parameter from: " + paramValueKey);
                return null;
            }
        }

        public override string WriteParameter(SignalParameter<string> param)
        {
            return SignalScribe.JoinParameterTypeNameValue(SIG_TYPE_SYMBOL, param.ParamName, param.Value);
        }
    }
}
