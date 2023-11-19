/* Implementation adapted from Code Monkey's A* tutorial: https://youtu.be/alU04hvz6L4?si=pmIRwJenaMXlIdc9 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStarPathfinding
{
    public const int MOVE_STRAIGHT_COST = 10;
    public const int MOVE_DIAGONAL_COST = 14;

    private IComparer<PathNode> nodeCostComparer = new PathNode.PathNodeCostComparer();
    protected System.Func<Tilemap, PathNode, PathNode, bool> nodeValidator;
    public System.Func<Tilemap, PathNode, PathNode, bool> NodeValidator {  get { if (nodeValidator == null) nodeValidator = NodeValidStandable; return nodeValidator; } }

    protected Tilemap tilemap;
    public Tilemap Tilemap { get { return tilemap; } }

    protected int entityHeight;
    public int EntityHeight { get { return entityHeight; } }

    protected Dictionary<Vector2Int, PathNode> validNodes = new Dictionary<Vector2Int, PathNode>();
    protected List<PathNode> openList;
    protected Dictionary<Vector2Int, PathNode> closedList;
    protected PathNode cachedEndNode = null;

    protected bool usePreviousPathAsPriority = false; // TODO: Fix the encapsulated code to work properly, it currently breaks if this is true
    public bool UsePreviousPathAsPriority { get { return usePreviousPathAsPriority; } set { usePreviousPathAsPriority = value; } }

    public AStarPathfinding(Tilemap tilemap, int entityHeight = 1, System.Func<Tilemap, PathNode, PathNode, bool> nodeValidator = null)
    {
        this.tilemap = tilemap;
        this.entityHeight = entityHeight;
        if (nodeValidator == null)
            this.nodeValidator = NodeValidStandable;
        else
            this.nodeValidator = nodeValidator;
    }

    public PathNode this[Vector2Int pos]
    {
        get
        {
            if (validNodes.TryGetValue(pos, out PathNode node))
                return node.Initialize();
            PathNode newNode = new PathNode(pos).Initialize();
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

    public bool NodeValid(Tilemap tilemap, PathNode node, PathNode previous)
    {
        return NodeValidator.Invoke(tilemap, node, previous);
    }

    public bool NodeValidStandable(Tilemap tilemap, PathNode node, PathNode previous)
    {
        return GridUtility.NodeIsStandable(tilemap, node, EntityHeight, 0);
    }

    public bool NodeValidUnsearched(Tilemap tilemap, PathNode node, PathNode previous)
    {
        return NodeValid(tilemap, node, previous) && !closedList.ContainsKey(node.Position);
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

        if (!PrepareForNewTarget(start, end, out openList, out closedList))
        {
            startNode.HCost = CalculateDistanceCost(startNode, endNode);
            startNode.CalculateFCost();
            GridUtility.InsertIntoSortedList(openList, startNode, nodeCostComparer);
        }

        // iterate while we still have nodes in the list
        while (openList.Count > 0)
        {
            // get the node at the beginning (we keep the list sorted by FCost, so it has the lowest FCost)
            PathNode currentNode = openList[0];
            //Debug.Log("Iterating in open list.");

            // Break early if the node is our target (we overload == in PathNode)
            if (currentNode == endNode)
            {
                // return the calculated path
                return CalculatePath(currentNode);
            }

            // remove the grabbed node and add it the closed list (we know it's not the target and it has the best FCost it can now)
            openList.RemoveAt(0);
            closedList.TryAdd(currentNode.Position, currentNode);

            if (currentNode.PriorityFlag)
            {

            }

            // Get the neighboring nodes (as long as they're not already searched in the closed list)
            List<PathNode> neighbors = GridUtility.GetNearValidNode(tilemap, currentNode, GetNode, NodeValidUnsearched, 1);
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
        cachedEndNode = end;
        List<PathNode> nodes = new List<PathNode>();
        PathNode currentNode = end;
        //Debug.Log("Building node path list");
        while (currentNode != null)
        {
            //Debug.Log("Current node: " + currentNode.Position + " and parent is " + (currentNode.HasParent ? currentNode.ParentNode.Position : "none."));
            if (!nodes.Contains(currentNode)) // for some reason a loop can be caused, although I'm not exactly sure how
            {
                nodes.Add(currentNode);
                currentNode = currentNode.ParentNode;
            } else
            {
                currentNode = null;
            }
        }
        nodes.Reverse();
        //Debug.Log("List contains " + nodes.Count + " nodes.");
        return nodes;
    }

    public int CalculateDistanceCost(PathNode a, PathNode b)
    {
        Vector2Int distances = a - b;
        int straights = Mathf.Abs(distances.x - distances.y);
        int diagonals = Mathf.Min(Mathf.Abs(distances.x), Mathf.Abs(distances.y));
        return MOVE_DIAGONAL_COST * diagonals + MOVE_STRAIGHT_COST * straights;
    }

    public List<Vector2> PathToPositions(Tilemap tilemap, List<PathNode> nodes)
    {
        List<Vector2> positions = new List<Vector2>();
        foreach (PathNode node in nodes)
        {
            Vector2 current = tilemap.GetCellCenterWorld(new Vector3Int(node.Position.x, node.Position.y, 0));
            positions.Add(current);
            //Debug.Log("Added position to path: " + current);
        }
        return positions;
    }

    public List<Vector2> FindPositionPath(Tilemap tilemap, Vector2 startF, Vector2 endF)
    {
        List<PathNode> path = FindPath(tilemap, startF, endF);
        if (path == null)
            return new List<Vector2>();
        return PathToPositions(tilemap, path);
    }

    public List<Vector2> FindPositionPath(Vector2 startF, Vector2 endF)
    {
        return FindPositionPath(tilemap, startF, endF);
    }

    // Returns true if the current start is on the priority path
    public bool PrepareForNewTarget(Vector2Int start, Vector2Int newTarget, out List<PathNode> openList, out Dictionary<Vector2Int, PathNode> closedList)
    {
        openList = new List<PathNode>();
        closedList = new Dictionary<Vector2Int, PathNode>();
        if (validNodes.Count == 0 || cachedEndNode == null)
        {
            return false;
        }

        PathNode startNode = new PathNode(start);
        PathNode endNode = new PathNode(newTarget);
        bool startOnPriorityPath = false;
        if (usePreviousPathAsPriority)
        {
            List<PathNode> priorityPath = new List<PathNode>();
            foreach (PathNode optPathNode in cachedEndNode.TraceParent())
            {
                //Debug.Log("Tracing parent :)");
                priorityPath.Add(optPathNode);
                optPathNode.PriorityFlag = true;
                if (optPathNode == startNode)
                {
                    optPathNode.Reinitialize(0);
                    optPathNode.HCost = CalculateDistanceCost(optPathNode, endNode);
                    optPathNode.CalculateFCost();
                    //Debug.Log("Set start node " + optPathNode.Position + " to new FCost: " + optPathNode.FCost);
                    startOnPriorityPath = true;
                    break;
                }
            }

            if (startOnPriorityPath)
            {
                priorityPath.Reverse();
                for (int i = 0; i < priorityPath.Count - 1; i++)
                {
                    //Debug.Log("Set node " + priorityPath[i + 1].Position + " parent to " + priorityPath[i].Position);
                    priorityPath[i + 1].Reinitialize();
                    int gCost = priorityPath[i].GCost + CalculateDistanceCost(priorityPath[i], priorityPath[i + 1]);
                    priorityPath[i + 1].UpdateParent(priorityPath[i], gCost);
                    priorityPath[i + 1].HCost = CalculateDistanceCost(priorityPath[i + 1], endNode);
                    priorityPath[i + 1].CalculateFCost();
                    //Debug.Log("Set priority path node " + priorityPath[i + 1].Position + " to new FCost: " + priorityPath[i + 1].FCost + " from gCost " + gCost);
                }
            }
            else
            {
                foreach (PathNode node in priorityPath)
                {
                    node.PriorityFlag = false;
                }
                priorityPath.Clear();
            }
        }

        List<Vector2Int> pendingRemoval = new List<Vector2Int>();
        foreach (KeyValuePair<Vector2Int, PathNode> keyValue in validNodes)
        {
            // Debug.Log("Iterating on node " + keyValue.Key);
            if (keyValue.Value.PriorityFlag)
            {
                keyValue.Value.PriorityInitialize(CalculateDistanceCost(keyValue.Value, endNode));
                GridUtility.InsertIntoSortedList(openList, keyValue.Value, nodeCostComparer);
                // ? closedList.Add(keyValue.Key, keyValue.Value);
                continue;
            }

            if (keyValue.Value.Initialized)
            {
                keyValue.Value.Uninitialize();
                continue;
            }

            pendingRemoval.Add(keyValue.Key);
        }
        foreach (Vector2Int vec in pendingRemoval)
        {
            validNodes.Remove(vec);
        }

        return startOnPriorityPath;
    }

    public void ClearCachedNodes()
    {
        validNodes.Clear();
    }
}
