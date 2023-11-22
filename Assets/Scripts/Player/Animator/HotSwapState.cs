using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HotSwapState
{
    [SerializeField] protected string stateName;
    public string StateName { get { return stateName; } }

    [SerializeField] protected HotSwapAnimation currentAnimation;
    public HotSwapAnimation CurrentAnimation { get { return currentAnimation; } }
    public bool HasAnimation { get { return currentAnimation != null; } }
    public bool NoAnimation { get { return currentAnimation == null; } }

    public bool ClearState()
    {
        ReplaceAnimation(null);
        return true;
    }

    public bool TrySetCurrentAnimation(HotSwapAnimation animation, bool force = false)
    {
        if (!HasAnimation || force)
        {
            ReplaceAnimation(animation);
            return true;
        }
        return false;
    }

    public HotSwapAnimation ReplaceAnimation(HotSwapAnimation animation)
    {
        HotSwapAnimation old = currentAnimation;
        currentAnimation = animation;
        animation?.ResetBufferPlay();
        Debug.Log("Replaced hot swap animation: " + (animation != null ? animation.name : "null"));
        return old;
    }
}
