using BufferedInput;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Motion/Motions/Animation Trigger")]
public class PMAnimationTrigger : PlayerMotion
{
    [SerializeField] protected bool aerialEffect = false;
    [SerializeField] protected bool groundedEffect = false;
    [SerializeField][Range(-2, 2)] protected float forwardMovement = 0f;
    protected bool cancellingEarly = false;

    public bool MeetsGroundedConditions(PlayerController controller)
    {
        if (!aerialEffect && !groundedEffect)
            return true;
        bool meetsGrounded = !groundedEffect || !controller.MotionController.Aerial;
        bool meetsAerial = !aerialEffect || controller.MotionController.Aerial;
        return meetsGrounded && meetsAerial;
    }

    public override MotionDataModifierFactory ActivateMotion(PlayerController controller, InputData input, StandardControlLocker locker = null)
    {
        //MotionDataModifierFactory modifierFactory = new MotionDataModifierFactory();
        //modifierFactory.AddSettingModifier(new AnimatorBoolSetting())
        if (locker != null)
        {
            controller.AddLocker(locker);
        }
        controller.MotionController.LockFlip = true;
        //Debug.Log("Activated motion: " + MotionName);
        return default;
    }

    public override MotionDataModifierFactory CancelMotion(PlayerController controller, InputData input = null)
    {
        if (CachedLocker != null)
        {
            controller.RemoveLocker(CachedLocker);
        }
        controller.MotionController.LockFlip = false;
        controller.MotionController.StopMoveInput();

        if (cancellingEarly && controller.HotSwapMotionController.MotionOccupiesSlot(this))
        {
            controller.HotSwapMotionController.RemoveCurrentMotion(this, true);
        }
        cancellingEarly = false;

        //Debug.Log("Cancelled motion: " + MotionName);

        return default;
    }

    public override bool CanActivateMotion(PlayerController controller, InputData input)
    {
        return base.CanActivateMotion(controller, input) && MeetsGroundedConditions(controller);
    }

    public override bool ShouldCancelMotion(PlayerController controller)
    {
        bool baseCancel = base.ShouldCancelMotion(controller);
        bool aerialCancel = !MeetsGroundedConditions(controller);
        return baseCancel | aerialCancel;
    }

    public override MotionDataModifierFactory UpdateMotion(PlayerController controller, InputData input = null)
    {
        controller.MotionController.Move(controller, forwardMovement * controller.MotionController.ForwardMult);
        return default;
    }
}
