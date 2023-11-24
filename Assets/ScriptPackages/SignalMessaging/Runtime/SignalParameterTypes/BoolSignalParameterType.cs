using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SignalMessaging {
    [CreateAssetMenu(menuName = "Signal Messaging/Parameter Types/Bool")]
    public class BoolSignalParameterType : SignalParameterType<bool>
    {
        public override string SIG_TYPE_SYMBOL => "b";
        public static readonly string[] TRUE_STRINGS = { "true", "t" };
        public static readonly string[] FALSE_STRINGS = { "false", "f" };

        public static bool MatchesTrueStrings(string paramValueKey)
        {
            foreach (string trueString in TRUE_STRINGS)
            {
                if (trueString.Equals(paramValueKey))
                    return true;
            }
            return false;
        }

        public static bool MatchesFalseStrings(string paramValueKey)
        {
            foreach (string falseString in FALSE_STRINGS)
            {
                if (falseString.Equals(paramValueKey))
                    return true;
            }
            return false;
        }

        public override SignalParameter<bool> ParseParameter(string paramName, string paramValueKey)
        {
            try
            {
                paramValueKey = paramValueKey.ToLower();
                bool value = MatchesTrueStrings(paramValueKey);
                return new SignalParameter<bool>(paramName, value, this);
            } catch
            {
                Debug.LogError("Error while parsing bool signal parameter from: " + paramValueKey);
                return null;
            }
        }

        public override string WriteParameter(SignalParameter<bool> param)
        {
            return SignalScribe.JoinParameterTypeNameValue(SIG_TYPE_SYMBOL, param.ParamName, param.Value ? TRUE_STRINGS[0] : FALSE_STRINGS[0]);
        }
    }
}
