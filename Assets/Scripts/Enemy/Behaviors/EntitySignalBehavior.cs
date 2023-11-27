using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SignalMessaging;

public abstract class EntitySignalBehavior : EntityBehavior, ISignalDataAcceptor
{
    public abstract void AcceptSignalData(SignalData signalData);
}
