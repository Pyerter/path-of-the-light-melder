using BufferedInput;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Motion/Motions/Animation Trigger")]
public class PMAnimationTrigger : PlayerMotion
{
    [SerializeField][Range(-2, 2)] protected float forwardMovement = 0f;

    public override MotionDataModifierFactory ActivateMotion(PlayerController controller, InputData input, StandardControlLocker locker = null)
    {
        //MotionDataModifierFactory modifierFactory = new MotionDataModifierFactory();
        //modifierFactory.AddSettingModifier(new AnimatorBoolSetting())
        if (locker != null)
        {
            controller.AddLocker(locker);
        }
        controller.MotionController.LockFlip = true;
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

        return default;
    }

    public override MotionDataModifierFactory UpdateMotion(PlayerController controller, InputData input = null)
    {
        controller.MotionController.Move(controller, forwardMovement * controller.MotionController.ForwardMult);
        return default;
    }
}
