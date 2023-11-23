using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimatorEventDirector : MonoBehaviour
{

    [SerializeField] List<UnityEvent> animationEvents;
    public void TriggerEvent(int index)
    {
        if (index >= animationEvents.Count)
            return;
        animationEvents[index]?.Invoke();
        // Debug.Log("Triggered animation event: " + index);
    }
}
