using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SignalMessaging;

[CreateAssetMenu(menuName = "Entity Behavior/Knockback")]
public class EntityKnockbackBehavior : EntitySignalBehavior
{
    [SerializeField] protected string knockbackAnimTrigger = "Staggered";
    [SerializeField] protected string knockbackStateName = "Staggered";
    [SerializeField][Range(0, 10f)] protected float knockbackStrengthMultiplier = 1f;
    [SerializeField][Range(0, 5f)] protected float knockbackDuration = 1f;
    [SerializeField][Range(0, 1f)] protected float knockbackUpPercentage = 0.1f;
    [SerializeField][Range(0, 1f)] protected float knockbackDrag = 0.05f;
    protected bool knocked = false;
    protected float force = 0f;
    protected int dragIterations;
    protected int currentDragIteration;
    public override bool BehaviorPredicate(EntityController controller)
    {
        if (knocked)
        {
            knocked = false;
            return true;
        }
        return false;
    }

    public override void InitiateBehavior(EntityController controller)
    {
        base.InitiateBehavior(controller);
        float xDiff = controller.PathManager.Target.position.x - controller.transform.position.x;
        float xDirection = -Mathf.Sign(xDiff);
        float strength = force * knockbackStrengthMultiplier;
        Debug.Log("Knocked back with force " + force + " and strength " + strength);
        Vector2 knockbackVelocity = new Vector2(1 - knockbackUpPercentage, knockbackUpPercentage);
        knockbackVelocity.x *= xDirection;
        knockbackVelocity.Normalize();
        knockbackVelocity *= strength;
        controller.velocity = knockbackVelocity;
        dragIterations = Mathf.FloorToInt(knockbackDuration / knockbackDrag);
        currentDragIteration = 0;
        try
        {
            controller.Anim.SetTrigger(knockbackAnimTrigger);
        }
        catch
        {

        }
    }

    public override bool BehaviorShouldEnd(EntityController controller)
    {
        return !controller.Anim.GetCurrentAnimatorStateInfo(0).IsName(knockbackStateName);
    }

    public override void UpdateBehavior(EntityController controller)
    {
        if (currentDragIteration < dragIterations)
        {
            Vector2 velocity = controller.velocity;
            velocity.x *= (1 - knockbackDrag);
            controller.velocity = velocity;
            currentDragIteration++;
        }
    }

    public override void AcceptSignalData(SignalData signalData)
    {
        if (!BehaviorActive)
        {
            force = signalData.ReadValue<float>("force", 1f);
            knocked = true;
        }
    }
}
