using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AStarPathManager), typeof(Rigidbody2D), typeof(EntityBehaviorManager))]
public class EntityController : MonoBehaviour
{
    [SerializeField] protected AStarPathManager pathManager;
    public AStarPathManager PathManager { get { if (pathManager == null) pathManager = GetComponent<AStarPathManager>(); return pathManager; } }
    public bool RunPathfinding { get { return PathManager.RunPathfinding; } set { PathManager.RunPathfinding = value; } }

    [SerializeField][Range(0, 2f)] protected float proximityThreshold = 0.05f;
    public float ProximityThreshold { get { return proximityThreshold; } }

    [SerializeField] protected Rigidbody2D rb;
    public Rigidbody2D RB { get { if (rb == null) rb = GetComponent<Rigidbody2D>(); return rb; } }
    public Vector2 velocity { get { return RB.velocity; } set { RB.velocity = value; } }

    [SerializeField] protected Transform groundLevelTransform;
    public Transform GroundLevelTransform { get { return groundLevelTransform; } }
    public Vector2 GroundLevelPosition { get { return new Vector2(groundLevelTransform.position.x, groundLevelTransform.position.y); } }

    [SerializeField] protected Animator anim;
    public Animator Anim { get { if (anim == null) anim = GetComponentInChildren<Animator>(); return anim; } }

    protected EntityBehaviorManager behaviorManager;
    public EntityBehaviorManager BehaviorManager { get { if (behaviorManager == null) behaviorManager = GetComponent<EntityBehaviorManager>(); return behaviorManager; } }

    protected bool flipped = false;
    public bool Flipped { get { return flipped; } }

    public void CheckFlip()
    {
        bool shouldFlip = false;
        if (rb.velocity.x > 0 && flipped)
            shouldFlip = true;
        if (rb.velocity.x < 0 && !flipped)
            shouldFlip = true;

        if (shouldFlip)
        {
            flipped = !flipped;
            Vector3 scale = RB.transform.localScale;
            scale.x *= -1;
            RB.transform.localScale = scale;
        }
    }

    public bool GetPathTarget(out Vector2 target, System.Func<Vector2, Vector2, Vector2, bool> skipNextPositionPredicate = null)
    {
        return PathManager.GetPathUpdate(out target, ProximityThreshold, skipNextPositionPredicate);
    }

    public bool GetPathTarget(out Vector2 target, float threshold, System.Func<Vector2, Vector2, Vector2, bool> skipNextPositionPredicate = null)
    {
        return PathManager.GetPathUpdate(out target, threshold, skipNextPositionPredicate);
    }

    public virtual void UpdateEntity()
    {
        BehaviorManager.LoopBehaviorsUpdate(this);
    }

    private void FixedUpdate()
    {
        UpdateEntity();
    }

    // TODO: Add fixedupdate calling behavior manager methods and create hooks for triggering
    // the pathfinding from running and not
}
