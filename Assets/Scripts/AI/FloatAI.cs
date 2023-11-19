using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
public class FloatAI : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] Transform target;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float speed = 2f;
    AStarPathfinding pathfinder;
    List<Vector2> targetPositions;
    int currentTarget = 0;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        pathfinder = new AStarPathfinding();
        targetPositions = pathfinder.FindPositionPath(tilemap, transform.position, target.position);
        currentTarget = 0;
    }

    public void FloatToPosition()
    {
        if (currentTarget >= targetPositions.Count)
            return;

        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);
        if (MathUtility.Equivalent(Vector2.Distance(targetPositions[currentTarget], currentPosition), 0))
        {
            currentTarget++;
            if (currentTarget >= targetPositions.Count)
            {
                rb.velocity = Vector2.zero;
                return;
            }
        }


        Vector2 diff = targetPositions[currentTarget] - currentPosition;
        rb.velocity = diff.normalized * speed;
    }

    private void FixedUpdate()
    {
        FloatToPosition();
    }
}
