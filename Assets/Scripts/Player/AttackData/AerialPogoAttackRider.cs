using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Attack Data/Player/Aerial Pogo")]
public class AerialPogoAttackRider : PlayerAttackRider
{
    [SerializeField][Range(0, 5f)] protected float jumpMultiplier = 1f;
    [SerializeField][Range(0, 20f)] protected float jumpSpeed = 2f;
    [SerializeField] protected bool usesMultiplier = true;

    public float JumpInput { get { return usesMultiplier ? jumpMultiplier : jumpSpeed; } }

    public override void OnAttack(HealthyEntity entity, EntityAttackData attackData)
    {
        Debug.Log("Activating pogo attack rider.");
        controller?.MotionController.ForceJump(controller, JumpInput, usesMultiplier);
    }
}
