using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SignalMessaging
{
    public interface ISignalDataAcceptor
    {
        public void AcceptSignalData(SignalData signalData);
    }
}