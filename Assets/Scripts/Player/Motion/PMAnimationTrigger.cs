using BufferedInput;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Motion/Animation Trigger")]
public class PMAnimationTrigger : PlayerMotion
{
    [SerializeField] protected string animationName = "Punch";
    [SerializeField] protected List<string> alternateStateNames = new List<string>();
    [SerializeField] protected int animationLayerIndex = 1;
    [SerializeField] protected bool shortable = false;
    [SerializeField][Range(-2, 2)] protected float forwardMovement = 0f;
    protected StandardControlLocker cachedLocker;

    public virtual void OnInputMotion(PlayerController controller, BufferedInput.InputData input)
    {

    }

    public virtual void OnPreUpdateMotion(PlayerController controller, MotionData motionData)
    {

    }

    public virtual void OnPostUpdateMotion(PlayerController controller, MotionData motionData)
    {

    }

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
            controller.MotionController.LockFlip = true;
            OnInputMotion(controller, input);
            return modifiers;
        } else if (input.IsReleased() && shortable)
        {
            return CancelMotion(controller, motionData);
        }
        return new List<MotionDataModifier>();
    }

    public override List<MotionDataModifier> CancelMotion(PlayerController controller, MotionData motionData = default)
    {
        Debug.Log("Canceling motion.");
        if (cachedLocker != null)
        {
            controller.RemoveLocker(cachedLocker);
            cachedLocker = null;
        }
        controller.MotionController.LockFlip = false;
        List<MotionDataModifier> modifiers = new List<MotionDataModifier>();
        if (activeMotion)
            modifiers.Add(new MotionDataSettingsModifier(new AnimatorBoolSetting("CancelAbility", true, true)));
        activeMotion = false;
        return modifiers;
    }

    public override List<MotionDataModifier> UpdateMotion(PlayerController controller, MotionData motionData = default)
    {
        OnPreUpdateMotion(controller, motionData);
        if (!StateMatchesName(controller))
        {
            activeMotion = false;
            Debug.Log("Ending animation: " + animationName);
        }

        if (!activeMotion)
        {
            CancelMotion(controller, motionData);
        }
        else if (forwardMovement != 0)
        {
            controller.MotionController.Move(controller, forwardMovement * controller.MotionController.ForwardMult);
        }
        OnPostUpdateMotion(controller, motionData);
        return new List<MotionDataModifier>();
    }

    public bool StateMatchesName(PlayerController controller)
    {
        if (controller.ComplexAnimator.StateNameIs(animationName, animationLayerIndex))
            return true;
        foreach (string name in alternateStateNames)
        {
            if (controller.ComplexAnimator.StateNameIs(name, animationLayerIndex))
                return true;
        }
        return false;
    }
}
