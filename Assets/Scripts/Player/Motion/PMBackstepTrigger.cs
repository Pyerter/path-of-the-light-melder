using BufferedInput;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Motion/Backstep")]
public class PMBackstepTrigger : PMAnimationTrigger
{
    protected float cachedSpeed = 0;

    public override void OnInputMotion(PlayerController controller, InputData input)
    {
        base.OnInputMotion(controller, input);
        if (controller.MotionController.RB.velocity.x != 0 && controller.MotionController.InputMoving)
        {
            cachedSpeed = forwardMovement;
            forwardMovement = -forwardMovement;
        }
    }

    public override List<MotionDataModifier> CancelMotion(PlayerController controller, MotionData motionData = default)
    {
        List<MotionDataModifier> modifiers = base.CancelMotion(controller, motionData);
        if (cachedSpeed != 0)
        {
            forwardMovement = cachedSpeed;
            cachedSpeed = 0;
        }
        return modifiers;
    }
}
