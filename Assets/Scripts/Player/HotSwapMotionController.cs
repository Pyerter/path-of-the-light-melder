using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotSwapMotionController : MonoBehaviour
{
    [SerializeField] protected ComplexAnimatorHotSwapper hotSwapper;
    public ComplexAnimatorHotSwapper HotSwapper { get { return hotSwapper; } }

    protected PlayerMotion currentMotion;
    protected PlayerMotion queuedMotion;

    public bool HasCurrentMotion { get { return currentMotion != null; } }
    public bool HasQueuedMotion { get { return queuedMotion != null; } }

    public bool MotionOccupiesSlot(PlayerMotion motion)
    {
        bool nullMatch = (currentMotion == null) == (motion == null);
        if (!nullMatch)
            return false;
        return currentMotion.MotionName.Equals(motion.MotionName);
    }

    public bool CanSetCurrentMotion(PlayerMotion motion)
    {
        return currentMotion == null;
    }

    public bool CanSetQueuedMotion(PlayerMotion motion)
    {
        return queuedMotion == null;
    }

    public bool SetCurrentMotion(PlayerMotion motion)
    {
        if (CanSetCurrentMotion(motion) && HotSwapper.TryAddAnimation(motion.MotionAnimation))
        {
            currentMotion = motion;
            HotSwapper.PendingHotSwap = motion;
            return true;
        }
        return false;
    }

    public bool RemoveCurrentMotion(PlayerMotion motion)
    {
        if (MotionOccupiesSlot(motion))
        {
            HotSwapper.PendingHotSwap = null;
            currentMotion = null;
            MoveQueuedMotionToCurrent();
            return true;
        }
        return false;
    }

    public bool SetQueuedMotion(PlayerMotion motion)
    {
        if (CanSetQueuedMotion(motion))
        {
            queuedMotion = motion;
            return true;
        }
        return false;
    }

    public bool QueueMotion(PlayerMotion motion)
    {
        if (SetCurrentMotion(motion))
        {
            return true;
        }
        if (SetQueuedMotion(motion))
        {
            return true;
        }
        return false;
    }

    public bool MoveQueuedMotionToCurrent()
    {
        if (HasQueuedMotion && SetCurrentMotion(queuedMotion))
        {
            queuedMotion = null;
            return true;
        }
        return false;
    }

    // TODO: Make this class to control the one motion which can use the hot swapper
    // or use this to transition between motions in the hot swapper
}
