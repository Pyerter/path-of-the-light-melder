using BufferedInput;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Motion/Punch")]
public class PMPunch : PlayerMotion
{
    protected bool activeMotion = false;
    public override bool ActiveMotion { get { return activeMotion; } }
    protected BufferedInput.StandardControlLocker cachedLocker;

    public override List<MotionDataModifier> InputMotion(PlayerController controller, InputData input, BufferedInput.StandardControlLocker locker = null, MotionData motionData = default)
    {
        if (input.IsPending())
        {
            activeMotion = true;
            if (locker != null)
            {
                cachedLocker = locker;
                controller.AddLocker(cachedLocker);
            }
        }
        return new List<MotionDataModifier>();
    }

    public override List<MotionDataModifier> UpdateMotion(PlayerController controller, MotionData motionData = default)
    {
        activeMotion = false;
        if (cachedLocker != null)
        {
            controller.RemoveLocker(cachedLocker);
            cachedLocker = null;
        }
        return new List<MotionDataModifier>();
    }
}
