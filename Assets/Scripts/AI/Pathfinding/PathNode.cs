using System.Collections;
using System.Collections.Generic;
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

    protected PathNode parentNode;
    public PathNode ParentNode { get { return parentNode; } }
    public bool HasParent { get { return parentNode != null; } }

    public PathNode(Vector2Int position)
    {
        this.position = position;
    }

    public PathNode Initialize(int gCost = int.MaxValue)
    {
        GCost = gCost;
        CalculateFCost();
        parentNode = null;
        return this;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public void UpdateParent(PathNode parent, int gCost)
    {
        this.parentNode = parent;
        GCost = gCost;
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

    public class PathNodeCostComparer : IComparer<PathNode>
    {
        int IComparer<PathNode>.Compare(PathNode a, PathNode b)
        {
            return a.FCost - b.FCost;
        }
    }
}
