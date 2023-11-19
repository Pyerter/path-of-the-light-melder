using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AStarPathManager))]
public class FloatingEnemy : MonoBehaviour
{
    [SerializeField] protected AStarPathManager pathManager;
    public AStarPathManager PathManager { get { if (pathManager == null) pathManager = GetComponent<AStarPathManager>(); return pathManager; } }

    [SerializeField] protected Rigidbody2D rb;
    public Rigidbody2D RB { get { if (rb == null) rb = GetComponent<Rigidbody2D>(); return rb; } }

    [SerializeField] protected float speed = 2;
    public float Speed { get { return speed; } }

    [SerializeField][Range(0, 1f)] protected float distanceThresholdToPoint = 0.05f;

    public void FloatToPosition()
    {
        Vector2 current = new Vector2(transform.position.x, transform.position.y);
        if (PathManager.GetPathUpdate(out Vector2 target, distanceThresholdToPoint))
        {
            RB.velocity = (target - current).normalized * speed;
            //Debug.Log("Path remains: " + target);
        }
        else
        {
            rb.velocity = Vector2.zero;
            //Debug.Log("No path left.");
        }
    }

    private void FixedUpdate()
    {
        FloatToPosition();
    }
}
