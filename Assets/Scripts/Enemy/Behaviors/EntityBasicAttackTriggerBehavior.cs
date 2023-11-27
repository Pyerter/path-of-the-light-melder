using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Entity Behavior/Enemy/Trigger Detection Attack")]
public class EntityBasicAttackTriggerBehavior : EntityTriggerDetectionBehavior
{
    [SerializeField] protected string animTrigger = "Attack";
    [SerializeField] protected string animState = "Attack";
    [SerializeField] protected int animStartWindow = 3;
    protected int startCounter = 0;
    protected bool receivedTrigger = false;

    public override bool BehaviorPredicate(EntityController controller)
    {
        if (receivedTrigger)
        {
            receivedTrigger = false;
            return true;
        }
        return false;
    }

    public override void InitiateBehavior(EntityController controller)
    {
        base.InitiateBehavior(controller);
        controller.Anim.SetTrigger("Attack");
        Vector2 velocity = controller.velocity;
        velocity.x = 0;
        controller.velocity = velocity;
        startCounter = 0;
    }

    public override bool BehaviorShouldEnd(EntityController controller)
    {
        if (startCounter >= animStartWindow)
        {
            return !controller.Anim.GetCurrentAnimatorStateInfo(0).IsName(animState);
        }
        startCounter++;
        return false;
    }

    public override void FinalizeBehavior(EntityController controller)
    {
        base.FinalizeBehavior(controller);
        receivedTrigger = false;
    }

    public override void UpdateBehavior(EntityController controller)
    {
        Vector2 velocity = controller.velocity;
        velocity.x = 0;
        controller.velocity = velocity;
    }

    public override void OnEntityTriggerDetection(EnemyPlayerDetection detector, Transform target, bool detected)
    {
        if (detected && !BehaviorActive)
            receivedTrigger = detected;
        else if (!detected && !BehaviorActive)
            receivedTrigger = detected;
    }
}
