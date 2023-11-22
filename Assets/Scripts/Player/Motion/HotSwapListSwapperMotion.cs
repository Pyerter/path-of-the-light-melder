using BufferedInput;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Motion/Motions/Hot Swap Animations List")]
public class HotSwapListSwapperMotion : PMAnimationTrigger, HotSwapSupplier
{
    [SerializeField] List<HotSwapAnimation> animations;
    [SerializeField] protected int currentAnimation = 0;

    public override bool QueryHotSwap(ComplexAnimatorHotSwapper hotSwapper, out HotSwapAnimation animation)
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
