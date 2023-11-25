using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AStarPathManager), typeof(Rigidbody2D), typeof(EntityBehaviorManager))]
public class EntityController : MonoBehaviour
{
    [SerializeField] protected AStarPathManager pathManager;
    public AStarPathManager PathManager { get { if (pathManager == null) pathManager = GetComponent<AStarPathManager>(); return pathManager; } }

    [SerializeField] protected Rigidbody2D rb;
    public Rigidbody2D RB { get { if (rb == null) rb = GetComponent<Rigidbody2D>(); return rb; } }

    [SerializeField] protected Animator anim;
    public Animator Anim { get { return anim; } }

    protected EntityBehaviorManager behaviorManager;
    public EntityBehaviorManager BehaviorManager { get { if (behaviorManager == null) behaviorManager = GetComponent<EntityBehaviorManager>(); return behaviorManager; } }

    private void Awake()
    {
        if (anim == null)
            anim = GetComponentInChildren<Animator>();
    }
}
