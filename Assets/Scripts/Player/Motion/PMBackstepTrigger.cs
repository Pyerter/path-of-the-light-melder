using BufferedInput;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Motion/Motions/Backstep")]
public class PMBackstepTrigger : PMAnimationTrigger
{
    protected float cachedSpeed = 0;

    public override MotionDataModifierFactory ActivateMotion(PlayerController controller, InputData input, StandardControlLocker locker = null)
    {
        MotionDataModifierFactory modifierFactory = base.ActivateMotion(controller, input, locker);
        if (controller.MotionController.InputMoving)
        {
            cachedSpeed = forwardMovement;
            forwardMovement = -forwardMovement;
        }
        controller.MotionController.Move(controller, forwardMovement * controller.MotionController.ForwardMult);
        return modifierFactory;
    }

    public override MotionDataModifierFactory CancelMotion(PlayerController controller, InputData input = null)
    {
        MotionDataModifierFactory modifierFactory = base.CancelMotion(controller, input);
        if (cachedSpeed != 0)
        {
            forwardMovement = cachedSpeed;
            cachedSpeed = 0;
        }
        return modifierFactory;
    }
}
