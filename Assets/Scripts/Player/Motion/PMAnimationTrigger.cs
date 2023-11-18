using BufferedInput;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Motion/Animation Trigger")]
public class PMAnimationTrigger : PlayerMotion
{
    [SerializeField] protected string animationName = "Punch";
    [SerializeField] protected int animationLayerIndex = 1;
    protected bool activeMotion = false;
    public override bool ActiveMotion { get { return activeMotion; } }
    protected StandardControlLocker cachedLocker;

    public override List<MotionDataModifier> InputMotion(PlayerController controller, InputData input, BufferedInput.StandardControlLocker locker = null, MotionData motionData = default)
    {
        if (input.IsPending())
        {
            Debug.Log("Triggered animation: " + animationName);
            activeMotion = true;
            if (locker != null)
            {
                cachedLocker = locker;
                controller.AddLocker(cachedLocker);
            }
            List<MotionDataModifier> modifiers = new List<MotionDataModifier>();
            modifiers.Add(new MotionDataSettingsModifier(new AnimatorBoolSetting(animationName, true, true)));
            return modifiers;
        }
        return new List<MotionDataModifier>();
    }

    public override List<MotionDataModifier> UpdateMotion(PlayerController controller, MotionData motionData = default)
    {
        if (!controller.ComplexAnimator.StateNameIs(animationName, animationLayerIndex))
        {
            activeMotion = false;
            Debug.Log("Ending animation: " + animationName);
        }
        //controller.MotionController.Move(controller, 0);
        if (!activeMotion && cachedLocker != null)
        {
            controller.RemoveLocker(cachedLocker);
            cachedLocker = null;
        }
        return new List<MotionDataModifier>();
    }
}
