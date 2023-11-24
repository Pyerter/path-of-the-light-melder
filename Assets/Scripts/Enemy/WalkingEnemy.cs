using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AStarPathManager))]
public class WalkingEnemy : MonoBehaviour
{
    [SerializeField] protected AStarPathManager pathManager;
    public AStarPathManager PathManager { get { if (pathManager == null) pathManager = GetComponent<AStarPathManager>(); return pathManager; } }

    [SerializeField] protected Rigidbody2D rb;
    public Rigidbody2D RB { get { if (rb == null) rb = GetComponent<Rigidbody2D>(); return rb; } }

    [SerializeField] protected Animator anim;
    public Animator Anim { get { return anim; } }

    [SerializeField] protected Transform groundLevelTransform;
    public Transform GroundLevelTransform { get { return groundLevelTransform; } }
    public Vector2 GroundLevelPosition { get { return new Vector2(groundLevelTransform.position.x, groundLevelTransform.position.y); } }

    [SerializeField] protected float speed = 2;
    public float Speed { get { return speed; } }

    [SerializeField] protected float jumpSpeed = 4f;
    public float JumpSpeed { get { return jumpSpeed; } }

    [SerializeField] protected float gravityScale = 3f;

    protected bool flipped = false;

    [SerializeField][Range(0, 1f)] protected float distanceThresholdToPoint = 0.05f;

    [SerializeField] [Range(0, 100f)] protected float socialAggroRange = 20f;
    [SerializeField] [Range(0, 1f)] protected float aggroDelayMin = 0.1f;
    [SerializeField] [Range(0, 1f)] protected float aggroDelayMax = 0.2f;
    [SerializeField] LayerMask socialAggroLayerMask;

    protected bool attacking = false;

    public void TriggerFollow(EnemyPlayerDetection detector, PlayerController controller)
    {
        bool pathfinding = PathManager.RunPathfinding;
        PathManager.RunPathfinding = true;
        if (!pathfinding)
        {
            Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position, socialAggroRange, socialAggroLayerMask);
            foreach (Collider2D collision in collisions)
            {
                if (collision.TryGetComponent<WalkingEnemy>(out WalkingEnemy enemy) && !enemy.PathManager.RunPathfinding)
                {
                    Debug.Log("Triggered delayed social aggro.");
                    float delayMax = Mathf.Clamp(aggroDelayMax, aggroDelayMin, 1);
                    StartCoroutine(QueueSocialAggro(controller, enemy, GameManager.Time + Random.Range(aggroDelayMin, delayMax)));
                    break;
                }
            }
        }
    }

    public void TriggerAttack(EnemyPlayerDetection detector, PlayerController controller, bool onEnter)
    {
        if (!attacking)
        {
            attacking = true;
            anim.SetTrigger("Attack");
            RB.velocity = Vector2.zero;
        }
    }

    public void TriggerAttackStop()
    {
        attacking = false;
    }

    public IEnumerator QueueSocialAggro(PlayerController controller, WalkingEnemy enemy, float threshold)
    {
        yield return new WaitUntil(() => GameManager.Time > threshold);
        enemy.TriggerFollow(null, controller);
    }

    public void ApplyGravity()
    {
        Vector2 gravity = Vector2.up * GameManager.Instance.Gravity * gravityScale;
        RB.AddForce(gravity, ForceMode2D.Force);
    }

    public bool PointIsLevelAndFurther(Vector2 currentPosition, Vector2 currentTarget, Vector2 nextTarget)
    {
        return nextTarget.y <= currentTarget.y || TargetIsEquivalentLevel(nextTarget, currentPosition);
    }

    public void RunToPosition()
    {
        Vector2 current = GroundLevelPosition;
        if (PathManager.GetPathUpdate(out Vector2 target, distanceThresholdToPoint, PointIsLevelAndFurther))
        {
            Vector2 velocity = target - current;
            if (ShouldJump(target))
            {
                velocity.y = jumpSpeed;
            } else
            {
                velocity.y = RB.velocity.y;
            }
            velocity.x = Mathf.Sign(velocity.x) * speed;
            RB.velocity = velocity;
            // RB.velocity = (target - current).normalized * speed;
            //Debug.Log("Path remains: " + target);
        }
        else
        {
            Vector2 velocity = RB.velocity;
            velocity.x = 0;
            RB.velocity = velocity;
            //Debug.Log("No path left.");
        }
    }

    public bool ShouldJump(Vector2 targetLocation)
    {
        bool targetAboveLevel = PathfinderManager.Instance.GetGridPosition(targetLocation).y > PathfinderManager.Instance.GetGridPosition(GroundLevelTransform.position).y;
        bool notJumping = RB.velocity.y <= 0.05f;
        return targetAboveLevel && notJumping;
    }

    public bool TargetIsEquivalentLevel(Vector2 target, Vector2 current)
    {
        Vector2Int targetPosition = PathfinderManager.Instance.GetCellPosition(target);
        Vector2Int currentPosition = PathfinderManager.Instance.GetCellPosition(current);
        return targetPosition.y == currentPosition.y;
    }

    public bool TargetIsAboveLevel(Vector2 target)
    {
        return PathfinderManager.Instance.GetGridPosition(target).y > PathfinderManager.Instance.GetGridPosition(GroundLevelTransform.position).y;
    }

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

    private void FixedUpdate()
    {
        ApplyGravity();
        if (!attacking)
        {
            RunToPosition();
            CheckFlip();
        }
    }
}
