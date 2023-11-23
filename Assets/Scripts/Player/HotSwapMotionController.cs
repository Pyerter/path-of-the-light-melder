using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotSwapMotionController : MonoBehaviour
{
    [SerializeField] protected ComplexAnimatorHotSwapper hotSwapper;
    public ComplexAnimatorHotSwapper HotSwapper { get { return hotSwapper; } }

    protected PlayerMotion currentMotion;
    protected PlayerMotion queuedMotion;

    public PlayerMotion CurrentMotion { get { return currentMotion; } }
    public PlayerMotion QueuedMotion { get { return queuedMotion; } }

    public bool HasCurrentMotion { get { return currentMotion != null; } }
    public bool HasQueuedMotion { get { return queuedMotion != null; } }

    private void FixedUpdate()
    {
        CheckToRemoveCurrentMotion();
    }

    public bool MotionOccupiesSlot(PlayerMotion motion)
    {
        bool nullCheck = (currentMotion == null) || (motion == null);
        if (nullCheck)
            return (currentMotion == null) && (motion == null);
        return currentMotion.MotionName.Equals(motion.MotionName);
    }

    public void CheckToRemoveCurrentMotion()
    {
        if (!VerifyCurrentMotionPlaying())
        {
            RemoveCurrentMotion(currentMotion);
            MoveQueuedMotionToCurrent();
        }
    }

    public bool VerifyCurrentMotionPlaying()
    {
        return hotSwapper.MatchesCurrentState(currentMotion);
    }

    public bool CanSetCurrentMotion(PlayerMotion motion)
    {
        if (currentMotion == null)
        {
            return true;
        }
        foreach (string motionName in currentMotion.InterruptibleMotions)
        {
            if (motion.MotionName.Equals(motionName))
                return true;
        }
        return false;
    }

    public bool CanSetQueuedMotion(PlayerMotion motion)
    {
        return queuedMotion == null;
    }

    public bool SetCurrentMotion(PlayerMotion motion, PlayerController controller = null)
    {
        if (CanSetCurrentMotion(motion)&& HotSwapper.TryAddAnimation(motion.MotionAnimation))
        {
            if (currentMotion != null && !MotionOccupiesSlot(motion))
            {
                currentMotion.TryCancelMotion(controller);
                HotSwapper.SkipAnimation();
                Debug.Log("Canceled current motion: " + currentMotion.MotionName);
            }
            currentMotion = motion;
            HotSwapper.PendingHotSwap = motion;
            //Debug.Log("Set current hot swap motion to " + motion.MotionName);
            return true;
        }
        return false;
    }

    public bool RemoveCurrentMotion(PlayerMotion motion, bool forceSkip = false)
    {
        if (MotionOccupiesSlot(motion))
        {
            HotSwapper.PendingHotSwap = null;
            if (forceSkip)
                HotSwapper.SkipAnimation();
            currentMotion = null;
            MoveQueuedMotionToCurrent();
            return true;
        }
        return false;
    }

    public bool TryCancelCurrentMotion(PlayerMotion motion)
    {
        if (HotSwapper.MatchesCurrentState(motion) && RemoveCurrentMotion(motion, true))
        {
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
        if (!HasCurrentMotion && SetCurrentMotion(motion))
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
        if (HasQueuedMotion && !HasCurrentMotion && SetCurrentMotion(queuedMotion))
        {
            queuedMotion = null;
            return true;
        }
        return false;
    }

    // TODO: Make this class to control the one motion which can use the hot swapper
    // or use this to transition between motions in the hot swapper
}
