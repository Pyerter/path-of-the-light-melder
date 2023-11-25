using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityBehaviorManager : MonoBehaviour
{
    [SerializeField] protected List<EntityBehavior> behaviors;
    public IReadOnlyList<EntityBehavior> Behaviors { get { return behaviors.AsReadOnly(); } }

    protected StringClaimManager behaviorClaimManager = new StringClaimManager();
    public StringClaimManager BehaviorClaimManager { get { return behaviorClaimManager; } }

    // TODO: Create a method (callable in fixedupdate loop from EntityController) that check for behavior predicates and
    // activates the behaviors as necessary based on those predicates. It becomes a pseudo
    // behavior tree that controls the actions of the entity
}
