using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MaskedLocks;

public abstract class EntityBehavior : ScriptableObject, MaskedLockProvider
{
    public static EntityBehaviorPriorityComparer priorityComparer { get { return EntityBehaviorPriorityComparer.instance; } }

    [SerializeField] protected string behaviorName = "behavior";
    public string BehaviorName { get { return behaviorName; } }

    protected bool behaviorActive = false;
    public bool BehaviorActive { get { return behaviorActive; } }

    [SerializeField] protected MaskedLock maskedLock;
    public MaskedLock MaskedLock { get { return maskedLock; } }
    public bool UsesLock { get { return maskedLock != null; } }
    public int Priority { get { return maskedLock != null ? maskedLock.Priority : 0; } }

    public abstract bool BehaviorPredicate(EntityController controller);
    public virtual void InitiateBehavior(EntityController controller)
    {
        behaviorActive = true;
    }
    public abstract void UpdateBehavior(EntityController controller);
    public virtual void CancelBehavior(EntityController controller)
    {
        behaviorActive = false;
    }
    public abstract bool BehaviorShouldEnd(EntityController controller);
    public virtual void FinalizeBehavior(EntityController controller)
    {
        behaviorActive = false;
    }

    public class EntityBehaviorPriorityComparer : IComparer<EntityBehavior>
    {
        public static readonly EntityBehaviorPriorityComparer instance = new EntityBehaviorPriorityComparer();
        int IComparer<EntityBehavior>.Compare(EntityBehavior a, EntityBehavior b)
        {
            return a.Priority - b.Priority;
        }
    }
}
