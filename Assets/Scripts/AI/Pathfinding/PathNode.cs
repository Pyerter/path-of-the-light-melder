using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathNode
{
    protected readonly Vector2Int position;
    public Vector2Int Position { get { return position; } }

    protected int gCost; // grid cost
    protected int hCost; // heuristic cost
    protected int fCost; // combined
    public int GCost { get { return gCost; } set { gCost = value; } }
    public int HCost { get { return hCost; } set { hCost = value; } }
    public int FCost { get { return fCost; } set { fCost = value; } }

    protected bool priorityFlag = false;
    public bool PriorityFlag { get { return priorityFlag; } set { priorityFlag = value; } }

    protected bool staleFlag = false;
    public bool StaleFlag { get { return staleFlag; } set { staleFlag = value; } }

    protected PathNode parentNode;
    public PathNode ParentNode { get { return parentNode; } }
    public bool HasParent { get { return parentNode != null; } }

    protected bool initialized = false;
    public bool Initialized { get { return initialized; } }

    public PathNode(Vector2Int position)
    {
        this.position = position;
    }

    public PathNode(PathNode target)
    {
        this.position = target.position;
        this.gCost = target.gCost;
        this.hCost = target.hCost;
        this.fCost = target.fCost;
        this.parentNode = target.parentNode;
    }

    public PathNode Reinitialize(int gCost = int.MaxValue)
    {
        initialized = false;
        return Initialize(gCost);
    }

    public PathNode Initialize(int gCost = int.MaxValue)
    {
        if (initialized)
            return this;

        GCost = gCost;
        CalculateFCost();
        parentNode = null;
        initialized = true;
        StaleFlag = false;
        return this;
    }

    public void PriorityInitialize(int hCost)
    {
        PriorityInitialize(GCost, hCost);
    }

    public void PriorityInitialize(int gCost, int hCost)
    {
        GCost = gCost;
        HCost = hCost;
        CalculateFCost();
    }

    public void Uninitialize()
    {
        initialized = false;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public void UpdateParent(PathNode parent, int gCost)
    {
        this.parentNode = parent;
        GCost = gCost;
        staleFlag = false;
    }

    public static Vector2Int operator -(PathNode a, PathNode b)
    {
        return a.position - b.position;
    }

    public static bool operator ==(PathNode a, PathNode b)
    {
        if ((a is null) || (b is null))
            return (a is null) == (b is null);
        return a.position.Equals(b.position);
    }

    public static bool operator !=(PathNode a, PathNode b)
    {
        if ((a is null) || (b is null))
            return (a is null) != (b is null);
        return !a.position.Equals(b.position);
    }

    public bool StandablePosition(Tilemap tilemap, int height = 1, int heightOffset = 0)
    {
        if (height < 1)
            return true;

        if (!tilemap.HasTile(new Vector3Int(position.x, position.y + heightOffset - 1, 0)))
            return false;

        for (int i = 0; i < height; i++)
        {
            if (tilemap.HasTile(new Vector3Int(position.x, position.y + i + heightOffset, 0)))
                return false;
        }
        return true;
    }

    public IEnumerable<PathNode> TraceParent()
    {
        PathNode currentNode = this;
        while (currentNode != null)
        {
            yield return currentNode;
            currentNode = currentNode.ParentNode;
        }
    }

    public class PathNodeCostComparer : IComparer<PathNode>
    {
        int IComparer<PathNode>.Compare(PathNode a, PathNode b)
        {
            int costDiff = a.FCost - b.FCost;
            if (costDiff != 0)
                return costDiff;

            int xDiff = a.position.x - b.position.x;
            if (xDiff != 0)
                return xDiff;

            int yDiff = a.position.y - b.position.y;
            return yDiff;
        }
    }
}
