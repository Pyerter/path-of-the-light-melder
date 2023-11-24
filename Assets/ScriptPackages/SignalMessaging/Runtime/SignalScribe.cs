using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SignalMessaging
{
    [CreateAssetMenu(menuName = "Signal Messaging/Signal Scribe")]
    public class SignalScribe : ScriptableObject
    {
        public const string SIG_DATA_PARAM_DECLARE_DELIM = "?";
        public const string SIG_DATA_PARAM_DELIM = "&";
        public const string SIG_DATA_PARAM_KEY_DELIM = ":";
        public const string SIG_DATA_PARAM_ASSIGN_DELIM = "=";
        public const string SIG_DATA_PARAM_EXPECTED_FORMAT = "signalName?paramTypeSymbol:paramName=paramValue&param2TypeSymbol:param2Name=param2Value";

        #region Instance
        protected static SignalScribe instance;
        public static SignalScribe Instance { get { return instance; } }

        public void ApplySettingsToInstance()
        {
            instance = this;
        }
        #endregion

        [SerializeField] protected List<BaseSignalParameterType> parameterTypes = new List<BaseSignalParameterType>();
        [SerializeField] protected Dictionary<string, BaseSignalParameterType> typeDict = new Dictionary<string, BaseSignalParameterType>();
        protected bool initialized = false;

        public static string JoinParameterTypeNameValue(string paramType, string paramName, string paramValue)
        {
            return paramType + SIG_DATA_PARAM_KEY_DELIM + paramName + SIG_DATA_PARAM_ASSIGN_DELIM + paramValue;
        }

        public void CheckInitialize()
        {
            bool countMismatch = parameterTypes.Count != typeDict.Count;
            if (countMismatch)
            {
                Initialize();
                return;
            }

            bool typeMismatch = false;
            foreach (BaseSignalParameterType paramType in parameterTypes)
            {
                if (!typeDict.ContainsKey(paramType.SIG_TYPE_SYMBOL))
                {
                    typeMismatch = true;
                    break;
                }
            }
            if (typeMismatch)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            typeDict = new Dictionary<string, BaseSignalParameterType>();
            foreach (BaseSignalParameterType paramType in parameterTypes)
            {
                typeDict.Add(paramType.SIG_TYPE_SYMBOL, paramType);
            }
            initialized = true;
            Debug.Log("Initialized SignalScribe. Current list of types contains " + parameterTypes.Count + " types.");
        }

        public SignalData ParseSignal(string signalKey)
        {
            signalKey = signalKey.ToLower();
            string[] keyParamsSplit = signalKey.Split(SIG_DATA_PARAM_DECLARE_DELIM);
            if (keyParamsSplit.Length != 2) return null;

            string signalName = keyParamsSplit[0];
            string[] paramsList = keyParamsSplit[1].Split(SIG_DATA_PARAM_DELIM);
            List<BaseSignalParameter> parameters = new List<BaseSignalParameter>();
            //Debug.Log("Parsing " + paramsList.Length + " parameters from " + signalName);
            foreach (string paramKey in paramsList)
            {
                if (TryParseParameter(paramKey, out BaseSignalParameter param))
                    parameters.Add(param);
                else
                    Debug.LogWarning("Failed to parse parameter from signal " + signalName + ": " + paramKey + "\nExpected signal format is: " + SIG_DATA_PARAM_EXPECTED_FORMAT);
            }

            return new SignalData(signalName, parameters);
        }

        public bool TryParseParameter(string paramKey, out BaseSignalParameter param)
        {
            param = ParseParameter(paramKey);
            return param != null;
        }

        public BaseSignalParameter ParseParameter(string paramKey)
        {
            string[] nameValueSplits = paramKey.Split(SIG_DATA_PARAM_ASSIGN_DELIM);
            if (nameValueSplits.Length != 2) return null;

            string[] nameTypeSplits = nameValueSplits[0].Split(SIG_DATA_PARAM_KEY_DELIM);
            if (nameTypeSplits.Length != 2)  return null;

            string paramTypeKey = nameTypeSplits[0];
            string paramNameKey = nameTypeSplits[1];
            string paramValueKey = nameValueSplits[1];
            if (paramTypeKey.Length == 0 || paramNameKey.Length == 0 || paramValueKey.Length == 0)
            {
                Debug.LogError("When parsing parameter the type, name, or value keys did not contain any characters: " +  paramKey);
                return null;
            }

            bool typeStored = typeDict.TryGetValue(paramTypeKey, out BaseSignalParameterType paramType);
            if (!typeStored)
            {
                Debug.LogWarning("While parsing parameter, failed to find type in typeDict: " + paramKey);
                return null;
            }

            BaseSignalParameter param = paramType.ParseBaseParameter(paramNameKey, paramValueKey);
            return param;
        }

        public string WriteParameter(BaseSignalParameter param)
        {
            return param.WriteParam();
        }
    }
}
