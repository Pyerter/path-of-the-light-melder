using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerMotion : ScriptableObject
{
    public PlayerMotion GetInstance()
    {
        return Instantiate(this);
    }

    public abstract bool ActiveMotion { get; }
    public abstract List<MotionDataModifier> InputMotion(PlayerController controller, BufferedInput.InputData input, BufferedInput.StandardControlLocker locker = null, MotionData motionData = default);
    public abstract List<MotionDataModifier> UpdateMotion(PlayerController controller, MotionData motionData = default);
}
