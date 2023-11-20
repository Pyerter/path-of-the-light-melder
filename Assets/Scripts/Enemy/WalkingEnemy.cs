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

    public void ApplyVelocity()
    {
        Vector2 gravity = Vector2.up * GameManager.Instance.Gravity * gravityScale;
        RB.AddForce(gravity, ForceMode2D.Force);
    }

    public bool PointIsLevelAndFurther(Vector2 currentPosition, Vector2 currentTarget, Vector2 nextTarget)
    {
        return currentTarget.y >= nextTarget.y;
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
            RB.velocity = Vector2.zero;
            //Debug.Log("No path left.");
        }
    }

    public bool ShouldJump(Vector2 targetLocation)
    {
        bool targetAboveLevel = PathfinderManager.Instance.GetGridPosition(targetLocation).y > PathfinderManager.Instance.GetGridPosition(GroundLevelTransform.position).y;
        bool notJumping = RB.velocity.y <= 0.05f;
        return targetAboveLevel && notJumping;
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
        ApplyVelocity();
        RunToPosition();
        CheckFlip();
    }
}
