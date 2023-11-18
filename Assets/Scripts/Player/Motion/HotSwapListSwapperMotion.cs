using BufferedInput;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Motion/Hot Swap Animations List")]
public class HotSwapListSwapperMotion : PMAnimationTrigger, HotSwapSupplier
{
    [SerializeField] List<HotSwapAnimation> animations;
    [SerializeField] protected int currentAnimation = 0;

    public override List<MotionDataModifier> InputMotion(PlayerController controller, InputData input, StandardControlLocker locker = null, MotionData motionData = default)
    {
        List<MotionDataModifier> modifiers = base.InputMotion(controller, input, locker, motionData);
        if (input.IsPending())
        {
            controller.ComplexAnimator.HotSwapper.PendingHotSwap = this;
            Debug.Log("Assigning PendingHotSwap to this.");
        }
        return modifiers;
    }

    public bool QueryHotSwap(ComplexAnimatorHotSwapper hotSwapper, out HotSwapAnimation animation)
    {
        if (currentAnimation >= animations.Count)
            currentAnimation = 0;
        if (currentAnimation < animations.Count)
        {
            animation = animations[currentAnimation];
            currentAnimation++;
        }
        else
        {
            animation = null;
        }
        return true;
    }
}
