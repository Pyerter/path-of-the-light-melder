using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityBehavior : ScriptableObject, StringClaim
{
    [SerializeField] protected string behaviorName = "behavior";
    public string BehaviorName { get { return behaviorName; } }

    [SerializeField] protected int priority = 0;
    public int Priority { get { return priority; } }

    [SerializeField] protected string claimName = "movement";
    public string ClaimName { get { return claimName; } }

    [SerializeField] protected bool usesClaim = true;
    public bool UsesClaim {  get { return usesClaim; } }

    public abstract void InitiateBehavior(EntityController controller);
    public abstract void UpdateBehavior(EntityController controller);
    public abstract void CancelBehavior(EntityController controller);
    public abstract void FinalizeBehavior(EntityController controller);
    public abstract bool BehaviorPredicate(EntityController controller);

    public virtual bool CanInitiate(EntityController controller)
    {
        return (!UsesClaim || controller.BehaviorManager.BehaviorClaimManager.HasClaim(ClaimName)) && BehaviorPredicate(controller);
    }

    public virtual bool TryInitiateBehavior(EntityController controller)
    {
        if (CanInitiate(controller) && (!UsesClaim || controller.BehaviorManager.BehaviorClaimManager.TryClaim(ClaimName, this)))
        {
            InitiateBehavior(controller);
            return true;
        }
        return false;
    }

    public class EntityBehaviorPriorityComparer : IComparer<EntityBehavior>
    {
        int IComparer<EntityBehavior>.Compare(EntityBehavior a, EntityBehavior b)
        {
            return a.Priority - b.Priority;
        }
    }
}
