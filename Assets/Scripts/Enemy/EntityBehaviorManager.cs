using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MaskedLocks;

[RequireComponent(typeof(MaskedLockManager))]
public class EntityBehaviorManager : MonoBehaviour
{
    [SerializeField] protected List<EntityBehavior> behaviors = new List<EntityBehavior>();
    public IReadOnlyList<EntityBehavior> Behaviors { get { return behaviors.AsReadOnly(); } }
    [SerializeField] protected List<EntityBehavior> activeBehaviors = new List<EntityBehavior>();

    [SerializeField] protected MaskedLockManager lockManager;
    public MaskedLockManager LockManager { get { if (lockManager == null) lockManager = GetComponent<MaskedLockManager>(); return lockManager; } }

    protected EntityVisionDetectionBehavior visionDetectionBehavior;
    protected EntityTriggerDetectionBehavior triggerDetectionBehavior;

    private void Awake()
    {
        List<EntityBehavior> instantiatedBehaviors = new List<EntityBehavior>();
        foreach (EntityBehavior behavior in behaviors)
        {
            if (behavior != null)
                instantiatedBehaviors.Add(Instantiate(behavior));
        }
        behaviors = instantiatedBehaviors;

        TryGetBehavior(out visionDetectionBehavior);
        TryGetBehavior(out triggerDetectionBehavior);
    }

    public void OnVisionDetection(EnemyPlayerDetection detector, Transform target, bool detected)
    {
        if (visionDetectionBehavior != null)
            visionDetectionBehavior.OnEntityVisionDetection(detector, target, detected);
    }

    public void OnTriggerDetection(EnemyPlayerDetection detector, Transform target, bool detected)
    {
        if (triggerDetectionBehavior != null)
            triggerDetectionBehavior.OnEntityTriggerDetection(detector, target, detected);
    }

    public bool TryGetBehavior(string behaviorName, out EntityBehavior targetBehavior)
    {
        foreach (EntityBehavior behavior in behaviors)
        {
            if (behavior.BehaviorName.ToLower().Equals(behaviorName.ToLower()))
            {
                targetBehavior = behavior;
                return true;
            }    
        }
        targetBehavior = null;
        return false;
    }

    public bool TryGetBehavior<T>(out T targetBehavior) where T : EntityBehavior
    {
        foreach (EntityBehavior behavior in behaviors)
        {
            T typedBehavior = behavior as T;
            if (typedBehavior != null)
            {
                targetBehavior = typedBehavior;
                return true;
            }
        }
        targetBehavior = null;
        return false;
    }

    public bool TryGetBehaviorAs<T>(string behaviorName, out T targetBehavior) where T : EntityBehavior
    {
        if (TryGetBehavior(behaviorName, out EntityBehavior untypedBehavior))
        {
            targetBehavior = untypedBehavior as T;
            return targetBehavior != null;
        }
        targetBehavior = null;
        return false;
    }

    private void OnValidate()
    {
        AssertBehaviorsSorted();
    }

    public void AssertBehaviorsSorted()
    {
        bool sorted = true;
        for (int i = 0; i < behaviors.Count - 1; i++)
        {
            if (behaviors[i] == null || behaviors[i + 1] == null || behaviors[i].Priority > behaviors[i + 1].Priority)
            {
                sorted = false;
                break;
            }
        }
        if (!sorted)
        {
            List<EntityBehavior> sortedBehaviors = new List<EntityBehavior>();
            List<EntityBehavior> nullBehaviors = new List<EntityBehavior>();
            foreach (EntityBehavior behavior in behaviors)
            {
                if (behavior == null)
                    nullBehaviors.Add(behavior);
                else
                    ListUtility.InsertIntoSortedList(sortedBehaviors, behavior, EntityBehavior.priorityComparer);
            }
            foreach (EntityBehavior behavior in nullBehaviors)
            {
                sortedBehaviors.Add(behavior);
            }
            behaviors = sortedBehaviors;
        }
    }

    public void AddActiveBehavior(EntityController controller, EntityBehavior behavior)
    {
        if (!behavior.BehaviorActive)
        {
            behavior.InitiateBehavior(controller);
            activeBehaviors.Add(behavior);
        }
    }

    public void CheckBehaviorsInitiate(EntityController controller)
    {
        foreach (EntityBehavior behavior in behaviors)
        {
            if (behavior == null)
            {
                Debug.Log("Null behavior!");
                continue;
            }
            if (!behavior.BehaviorActive
                && (!behavior.UsesLock || LockManager.CanAddLock(behavior.MaskedLock.Key))
                && behavior.BehaviorPredicate(controller)
                && (!behavior.UsesLock || LockManager.TryAddLock(behavior.MaskedLock.Key)))
            {
                AddActiveBehavior(controller, behavior);
            }
        }
    }

    public void UpdateBehaviors(EntityController controller)
    {
        foreach (EntityBehavior behavior in activeBehaviors)
        {
            behavior.UpdateBehavior(controller);
        }
    }

    public void CheckBehaviorsEnded(EntityController controller)
    {
        for (int i = 0; i < activeBehaviors.Count; i++)
        {
            EntityBehavior behavior = activeBehaviors[i];
            bool removed = false;
            if (behavior.BehaviorShouldEnd(controller))
            {
                behavior.FinalizeBehavior(controller);
                removed = true;
            } else if (behavior.UsesLock && !LockManager.ContainsLock(behavior.MaskedLock.Key))
            {
                behavior.CancelBehavior(controller);
                removed = true;
            }
            // else check if behavior lock is removed
            if (removed)
            {
                activeBehaviors.RemoveAt(i);
                i--;
                if (behavior.UsesLock)
                    LockManager.TryRemoveLock(behavior.MaskedLock.Key);
            }
        }
    }

    public void LoopBehaviorsUpdate(EntityController controller)
    {
        CheckBehaviorsInitiate(controller);
        UpdateBehaviors(controller);
        CheckBehaviorsEnded(controller);
    }
}
