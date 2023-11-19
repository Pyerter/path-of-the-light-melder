using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathManager : MonoBehaviour
{
    [SerializeField] protected Transform target;
    protected AStarPathfinding pathfinder;
    protected Vector2Int previousTargetCell;
    protected Vector2Int updatedTargetCell;
    protected Vector2 previousPathUpdate;
    protected bool queueTargetRecalculation = true;

    protected PathInstance currentPath;
    public PathInstance CurrentPath { get { return currentPath; } }

    private void Awake()
    {
        if (target == null)
            target = PathfinderManager.Instance.PlayerTarget;
        pathfinder = new AStarPathfinding(PathfinderManager.Instance.GroundTiles);
    }

    private void Start()
    {
        // Do an initial path calculation
        queueTargetRecalculation = false;
        updatedTargetCell = GetCurrentTargetCell();
        previousTargetCell = updatedTargetCell;
        CalculatePathInstance();

        // Start the coroutine to calculate it intermittently
        StartCoroutine(CalculatePath());
    }

    public bool ShouldRecalculateTarget()
    {
        return queueTargetRecalculation;
    }

    public IEnumerator WaitForCalculationQueue()
    {
        yield return new WaitUntil(ShouldRecalculateTarget);
        previousTargetCell = updatedTargetCell;
        queueTargetRecalculation = false;
    }

    public IEnumerator CalculatePath()
    {
        while (true)
        {
            yield return StartCoroutine(WaitForCalculationQueue());
            CalculatePathInstance();
        }
    }

    public void CalculatePathInstance()
    {
        Debug.Log("Calculating new path instance.");
        pathfinder.ClearCachedNodes();
        Vector2 start = new Vector2(transform.position.x, transform.position.y);
        Vector2 end = new Vector2(target.position.x, target.position.y);
        Debug.Log("Start is " + start + " and end is " + end);
        PathInstance inst = new PathInstance(pathfinder.FindPositionPath(start, end));
        Debug.Log("New calculation contains " + inst.positions.Count + " positions.");
        if (inst.positions.Count > 0)
        {
            currentPath = inst;
        } else
        {
            Debug.Log("Not setting to new path because new path is empty.");
        }
    }

    public void CheckShouldRecalculateTarget()
    {
        updatedTargetCell = GetCurrentTargetCell();
        if (!previousTargetCell.Equals(updatedTargetCell))
        {
            queueTargetRecalculation = true;
        }
    }

    public Vector2Int GetCurrentTargetCell()
    {
        Vector3Int targetCell3 = PathfinderManager.Instance.GroundTiles.WorldToCell(target.position);
        return new Vector2Int(targetCell3.x, targetCell3.y);
    }

    // returns true if there is a remaining target location in the path
    public bool GetPathUpdate(out Vector2 target, float threshold = 0.01f)
    {
        Vector2 current = new Vector2(transform.position.x, transform.position.y);
        bool hasPath = currentPath.TryTargetAndPosition(current, out target, threshold);
        if (!hasPath)
            target = current;
        CheckShouldRecalculateTarget();
        previousPathUpdate = target;
        return hasPath;
    }

    public struct PathInstance
    {
        public List<Vector2> positions;
        public int currentIndex;

        public PathInstance(List<Vector2> positions, int currentIndex = 0)
        {
            this.positions = positions;
            this.currentIndex = currentIndex;
        }

        public bool TryGetCurrentTarget(out Vector2 target)
        {
            if (positions == null || currentIndex >= positions.Count)
            {
                target = Vector2.zero;
                return false;
            }
            target = positions[currentIndex];
            return true;
        }

        public bool TryGetNextTarget(out Vector2 target)
        {
            if (positions == null || currentIndex + 1 >= positions.Count)
            {
                target = Vector2.zero;
                return false;
            }
            target = positions[currentIndex + 1];
            return true;
        }

        public bool CheckShouldSkipCurrentTarget(Vector2 current)
        {
            if (TryGetCurrentTarget(out Vector2 target) && TryGetNextTarget(out Vector2 nextTarget))
            {
                if (Vector2.Distance(target, nextTarget) > Vector2.Distance(nextTarget, current))
                {
                    currentIndex++;
                    return true;
                }
            }
            return false;
        }

        public bool CheckPositionEquivalence(Vector2 current, float threshold = 0.01f)
        {
            if (TryGetCurrentTarget(out Vector2 target))
            {
                bool equivalent = MathUtility.Equivalent(Vector2.Distance(current, target), 0, threshold);
                if (equivalent)
                    currentIndex++;
                return equivalent;
            }
            return false;
        }

        public bool TryTargetAndPosition(Vector2 current, out Vector2 target, float threshold = 0.01f)
        {
            CheckPositionEquivalence(current, threshold);
            CheckShouldSkipCurrentTarget(current);
            bool hasTarget = TryGetCurrentTarget(out target);
            return hasTarget;
        }
    }
}
