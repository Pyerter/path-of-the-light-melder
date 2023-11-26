using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Entity Behavior/Enemy/Targeted Walk")]
public class EntityTargetedWalkBehavior : EntityBehavior
{
    [SerializeField][Range(0, 5f)] protected float speed = 2;
    public float Speed { get { return speed; } }

    [SerializeField][Range(0, 15f)] protected float jumpSpeed = 4f;
    public float JumpSpeed { get { return jumpSpeed; } }

    public override bool BehaviorPredicate(EntityController controller)
    {
        return true;
    }

    public override bool BehaviorShouldEnd(EntityController controller)
    {
        return false;
    }

    public override void UpdateBehavior(EntityController controller)
    {
        RunToPosition(controller);
        controller.CheckFlip();
    }

    protected void RunToPosition(EntityController controller)
    {
        Vector2 current = controller.GroundLevelPosition;
        if (controller.GetPathTarget(out Vector2 target, PointIsLevelAndFurther))
        {
            Vector2 velocity = target - current;
            if (ShouldJump(controller, target))
            {
                velocity.y = jumpSpeed;
            }
            else
            {
                velocity.y = controller.velocity.y;
            }
            velocity.x = Mathf.Sign(velocity.x) * speed;
            controller.velocity = velocity;
            // RB.velocity = (target - current).normalized * speed;
            //Debug.Log("Path remains: " + target);
        }
        else
        {
            Vector2 velocity = controller.velocity;
            velocity.x = 0;
            controller.velocity = velocity;
            //Debug.Log("No path left.");
        }
    }

    public bool ShouldJump(EntityController controller, Vector2 targetLocation)
    {
        bool targetAboveLevel = PathfinderManager.Instance.GetGridPosition(targetLocation).y > PathfinderManager.Instance.GetGridPosition(controller.GroundLevelPosition).y;
        bool notJumping = controller.velocity.y <= 0.05f;
        return targetAboveLevel && notJumping;
    }

    public static bool PointIsLevelAndFurther(Vector2 currentPosition, Vector2 currentTarget, Vector2 nextTarget)
    {
        return nextTarget.y <= currentTarget.y || TargetIsEquivalentLevel(nextTarget, currentPosition);
    }

    public static bool TargetIsEquivalentLevel(Vector2 target, Vector2 current)
    {
        Vector2Int targetPosition = PathfinderManager.Instance.GetCellPosition(target);
        Vector2Int currentPosition = PathfinderManager.Instance.GetCellPosition(current);
        return targetPosition.y == currentPosition.y;
    }
}
