using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Entity Behavior/Gravity")]
public class EntityGravityBehavior : EntityBehavior
{
    [SerializeField] protected float gravityScale = 3f;
    public float GravityScale { get { return gravityScale; } }

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
        Vector2 gravity = Vector2.up * GameManager.Instance.Gravity * gravityScale;
        controller.RB.AddForce(gravity, ForceMode2D.Force);
    }
}
