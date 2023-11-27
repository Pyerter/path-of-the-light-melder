using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SignalMessaging;

public class EntityBehaviorSignalAccepto : MonoBehaviour
{
    [SerializeField] EntityController controller;
    [SerializeField] EntitySignalBehavior behaviorSignal;

    private void Awake()
    {
        if (behaviorSignal != null)
        {
            behaviorSignal = Instantiate(behaviorSignal);
        }
    }

    public void AcceptSignal(SignalData signalData)
    {
        if (behaviorSignal != null && controller != null)
        {
            behaviorSignal.AcceptSignalData(signalData);
            controller.BehaviorManager.TryAddActiveBehavior(controller, behaviorSignal);
        } else
        {
            Debug.LogError("BehaviorSignal or EntityController is not set.");
        }
    }
}
