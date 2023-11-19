using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStarPathfinding
{
    public const int MOVE_STRAIGHT_COST = 10;
    public const int MOVE_DIAGONAL_COST = 14;

    private IComparer<PathNode> nodeCostComparer = new PathNode.PathNodeCostComparer();

    protected int entityHeight;
    public int EntityHeight { get { return entityHeight; } }

    protected Dictionary<Vector2Int, PathNode> validNodes = new Dictionary<Vector2Int, PathNode>();
    protected List<PathNode> openList;
    protected Dictionary<Vector2Int, PathNode> closedList;

    public AStarPathfinding(int entityHeight)
    {
        this.entityHeight = entityHeight;
    }

    public PathNode this[Vector2Int pos]
    {
        get
        {
            if (validNodes.TryGetValue(pos, out PathNode node))
                return node;
            PathNode newNode = new PathNode(pos);
            validNodes.Add(pos, newNode);
            return newNode;
        }
        set
        {
            validNodes[pos] = value;
        }
    }

    public PathNode GetNode(Vector2Int vec)
    {
        return this[vec];
    }

    public PathNode GetNodeInitialized(Vector2Int vec)
    {
        return this[vec].Initialize();
    }

    public bool NodeValid(Tilemap tilemap, PathNode node)
    {
        return GridUtility.NodeIsStandable(tilemap, node, EntityHeight, 0);
    }

    public bool NodeValidUnsearched(Tilemap tilemap, PathNode node)
    {
        return NodeValid(tilemap, node) && !closedList.ContainsKey(node.Position);
    }

    public List<PathNode> FindPath(Tilemap tilemap, Vector2 startF, Vector2 endF)
    {
        // turn starting vector into grid space
        Vector3Int start3 = tilemap.WorldToCell(startF);
        Vector3Int end3 = tilemap.WorldToCell(endF);
        Vector2Int start = new Vector2Int(start3.x, start3.y);
        Vector2Int end = new Vector2Int(end3.x, end3.y);

        // Initialize starting nodes
        PathNode startNode = this[start].Initialize(0);
        PathNode endNode = this[end].Initialize();

        // Initialize lists
        openList = new List<PathNode> { startNode };
        closedList = new Dictionary<Vector2Int, PathNode>();

        // Calculate start node values
        startNode.HCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        // iterate while we still have nodes in the list
        while (openList.Count > 0)
        {
            // get the node at the beginning (we keep the list sorted by FCost, so it has the lowest FCost)
            PathNode currentNode = openList[0];

            // Break early if the node is our target (we overload == in PathNode)
            if (currentNode == endNode)
            {
                // return the calculated path
                return CalculatePath(currentNode);
            }

            // remove the grabbed node and add it the closed list (we know it's not the target and it has the best FCost it can now)
            openList.RemoveAt(0);
            closedList.Add(currentNode.Position, currentNode);

            // Get the neighboring nodes (as long as they're not already searched in the closed list)
            List<PathNode> neighbors = GridUtility.GetNearValidNode(tilemap, currentNode, GetNodeInitialized, NodeValidUnsearched, 1);
            foreach (PathNode neighbor in neighbors)
            {
                // calculate the pending path cost
                int tentativeCost = currentNode.GCost + CalculateDistanceCost(currentNode, neighbor);
                // if it's better, update that node
                // if a node is unsearched, it will always be better because they're initialized to infinite GCost (path cost)
                if (tentativeCost < neighbor.GCost)
                {
                    // (try to) remove from the list because we're going to update the value it's sorted by
                    GridUtility.TryPopFromSortedList(openList, neighbor, nodeCostComparer, out PathNode poppedNeighbor); 
                    // update the parent, HCost, and FCost
                    neighbor.UpdateParent(currentNode, tentativeCost);
                    neighbor.HCost = CalculateDistanceCost(neighbor, endNode);
                    neighbor.CalculateFCost(); 
                    // Put it back into the list after updating FCost
                    GridUtility.InsertIntoSortedList(openList, neighbor, nodeCostComparer);
                }
            }
        }

        return null;
    }

    public List<PathNode> CalculatePath(PathNode end)
    {
        List<PathNode> nodes = new List<PathNode>();
        PathNode currentNode = end;
        while (currentNode != null)
        {
            nodes[0] = currentNode;
            currentNode = nodes[0].ParentNode;
        }
        return nodes;
    }

    public int CalculateDistanceCost(PathNode a, PathNode b)
    {
        Vector2Int distances = a - b;
        int straights = Mathf.Abs(distances.x - distances.y);
        int diagonals = Mathf.Min(Mathf.Abs(distances.x), Mathf.Abs(distances.y));
        return MOVE_DIAGONAL_COST * diagonals + MOVE_STRAIGHT_COST * straights;
    }
}
