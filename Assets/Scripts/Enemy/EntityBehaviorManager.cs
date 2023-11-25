using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityBehaviorManager : MonoBehaviour
{
    [SerializeField] protected List<EntityBehavior> behaviors;
    public IReadOnlyList<EntityBehavior> Behaviors { get { return behaviors.AsReadOnly(); } }

    protected StringClaimManager behaviorClaimManager = new StringClaimManager();
    public StringClaimManager BehaviorClaimManager { get { return behaviorClaimManager; } }
}
