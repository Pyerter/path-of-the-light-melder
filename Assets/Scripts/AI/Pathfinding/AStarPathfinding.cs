/* Implementation adapted from Code Monkey's A* tutorial: https://youtu.be/alU04hvz6L4?si=pmIRwJenaMXlIdc9 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.RuleTile.TilingRuleOutput;

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
    protected PathNode cachedStartNode = null;
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

    public bool NodeValidUnsearched(Tilemap tilemap, PathNode node, PathNode previous, Dictionary<Vector2Int, PathNode> closedList)
    {
        return NodeValid(tilemap, node, previous) && !closedList.ContainsKey(node.Position);
    }

    public List<PathNode> FindInformedPath(Tilemap tilemap, Vector2 startF, Vector2 endF)
    {
        // turn starting vector into grid space
        Vector3Int start3 = tilemap.WorldToCell(startF);
        Vector3Int end3 = tilemap.WorldToCell(endF);
        Vector2Int start = new Vector2Int(start3.x, start3.y);
        Vector2Int end = new Vector2Int(end3.x, end3.y);

        // Verify that the openList and closedList variables are set
        if (openList == null)
            openList = new List<PathNode>();
        if (closedList == null)
            closedList = new Dictionary<Vector2Int, PathNode>();

        // Initialize starting nodes
        // start node MUST be reinitialized so that we can validate path from previous calculations
        PathNode startNode = this[start].Reinitialize(0); 
        PathNode endNode = this[end].Initialize();

        // Make calculations for the first node
        startNode.HCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();
        GridUtility.InsertIntoSortedList(openList, startNode, nodeCostComparer);

        List<PathNode> validationInputList = new List<PathNode> { startNode };

        if (!usePreviousPathAsPriority)
        {
            ClearCachedNodes();
            openList = validationInputList;
            return FindPath(tilemap, startNode, endNode, openList, closedList, false);
        }

        // Mark nodes as stale or uninitialized when necessary
        PrepareForNewTarget(startNode, endNode);

        // cache the previously calculated open list
        List<PathNode> cachedOpenList = openList;

        // validation iteration of find path: open list only contains start node, it doesn't updated nodes which already have it as a parent
        List<PathNode> validatedPath = FindPath(tilemap, startNode, endNode, validationInputList, new Dictionary<Vector2Int, PathNode>(), 
            out List<PathNode> validatedOpenList, out Dictionary<Vector2Int, PathNode> validatedClosedList, true);

        // Create a new open list excluding nodes we've already searched:
        // No nodes that are in the vaildatedClosedList
        // No nodes that are in the closedList and not in the validatedOpenList
        // Yes nodes that are in the validatedOpenList
        // Yes nodes that are in the openList and not in the validatedClosedList
        // Also, a new closed list that contains:
        // Yes nodes that are in the closedList and not in the validatedOpenList
        // Yes nodes that are in the validatedClosedList
        Dictionary<Vector2Int, PathNode> freshOpenList = new Dictionary<Vector2Int, PathNode>();
        foreach (PathNode pathNode in validatedOpenList)
        {
            freshOpenList.TryAdd(pathNode.Position, pathNode);
            closedList.Remove(pathNode.Position);
        }
        foreach (PathNode node in cachedOpenList)
        {
            if (!validatedClosedList.ContainsKey(node.Position))
                freshOpenList.TryAdd(node.Position, node);
        }

        openList = freshOpenList.Values.ToList();
        foreach (KeyValuePair<Vector2Int, PathNode> keyValue in validatedClosedList)
        {
            closedList.TryAdd(keyValue.Key, keyValue.Value);
        }

        // Check if the closed list already contains the end node
        if (closedList.ContainsKey(endNode.Position))
        {
            return CalculatePath(startNode, endNode);
        }

        // Check if the validation failed to find a path
        if (validatedPath == null)
        {
            // if so, recalculate FCosts and find a new path with the new open list
            RecalculateStaleFCosts(openList, endNode);
            validatedPath = FindPath(tilemap, startNode, endNode, openList, closedList, false);
        }


        return validatedPath;
    }

    public List<PathNode> FindPath(Tilemap tilemap, PathNode startNode, PathNode endNode,
        List<PathNode> openList, Dictionary<Vector2Int, PathNode> closedList, bool avoidPrecalculatedNodes)
    {
        return FindPath(tilemap, startNode, endNode, openList, closedList, out List<PathNode> outOpenList, out Dictionary<Vector2Int, PathNode> outClosedList, avoidPrecalculatedNodes);
    }

    public List<PathNode> FindPath(Tilemap tilemap, PathNode startNode, PathNode endNode, 
        List<PathNode> openList, Dictionary<Vector2Int, PathNode> closedList, 
        out List<PathNode> returnedOpenList, out Dictionary<Vector2Int, PathNode> returnedClosedList,
        bool avoidPrecalculatedNodes)
    {
        returnedOpenList = openList;
        returnedClosedList = closedList;

        System.Func<Tilemap, PathNode, PathNode, bool> nodeValidationPredicate = (t, p1, p2) => NodeValidUnsearched(t, p1, p2, closedList);

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
                return CalculatePath(startNode, currentNode);
            }

            // remove the grabbed node and add it the closed list (we know it's not the target and it has the best FCost it can now)
            openList.RemoveAt(0);
            closedList.TryAdd(currentNode.Position, currentNode);

            // Get the neighboring nodes (as long as they're not already searched in the closed list)
            List<PathNode> neighbors = GridUtility.GetNearValidNode(tilemap, currentNode, GetNode, nodeValidationPredicate, 1);
            foreach (PathNode neighbor in neighbors)
            {
                // Check if current node is already a neighbor of target node
                if (avoidPrecalculatedNodes && neighbor.ParentNode == currentNode)
                {
                    // (try to) remove from the list because we're going to update the value it's sorted by
                    GridUtility.TryPopFromSortedList(openList, neighbor, nodeCostComparer, out PathNode poppedNeighbor);
                    // If so, update the FCost so that other nodes don't claim it
                    int targetGCost = currentNode.GCost + CalculateDistanceCost(currentNode, neighbor);
                    // update the parent, HCost, and FCost
                    neighbor.UpdateParent(currentNode, targetGCost);
                    neighbor.HCost = CalculateDistanceCost(neighbor, endNode);
                    neighbor.CalculateFCost();
                    // Note: we're not adding it to the open list because it must have a previously calculated
                    // shortest path from a previous iteration of calculations
                    continue;
                }

                // Reinitialize node so that we don't have any skewed data
                // If current node is stale, then it may have an improper FCost because it was
                // calculated with respect to the wrong starting location
                if (neighbor.StaleFlag)
                    neighbor.Reinitialize();

                // calculate the pending path cost
                int tentativeCost = currentNode.GCost + CalculateDistanceCost(currentNode, neighbor);
                // if it's better, update that node
                // if a node is unsearched, it will always be better because they're initialized to infinite GCost (path cost)
                if (tentativeCost < neighbor.GCost)
                {
                    UpdateAndAddNode(currentNode, neighbor, endNode, openList, tentativeCost);
                }
            }
        }

        return null;
    }

    protected void UpdateAndAddNode(PathNode currentNode, PathNode neighborNode, PathNode endNode, List<PathNode> openList, int newGCost)
    {
        // (try to) remove from the list because we're going to update the value it's sorted by
        GridUtility.TryPopFromSortedList(openList, neighborNode, nodeCostComparer, out PathNode poppedNeighbor);
        // update the parent, HCost, and FCost
        neighborNode.UpdateParent(currentNode, newGCost);
        neighborNode.HCost = CalculateDistanceCost(neighborNode, endNode);
        neighborNode.CalculateFCost();
        // Put it back into the list after updating FCost
        GridUtility.InsertIntoSortedList(openList, neighborNode, nodeCostComparer);
    }

    public List<PathNode> CalculatePath(PathNode start, PathNode end)
    {
        cachedStartNode = start;
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
        List<PathNode> path = FindInformedPath(tilemap, startF, endF);
        if (path == null)
            return new List<Vector2>();
        return PathToPositions(tilemap, path);
    }

    public List<Vector2> FindPositionPath(Vector2 startF, Vector2 endF)
    {
        return FindPositionPath(tilemap, startF, endF);
    }

    // Returns true if the current start is on the priority path
    public bool PrepareForNewTarget(PathNode startNode, PathNode endNode)
    {
        // if no preexisting calculations exist, return false
        if (validNodes.Count == 0 || cachedEndNode == null || cachedStartNode == null)
        {
            return false;
        }

        // if the start and end nodes are the same, no calculations are stale
        if (cachedStartNode == startNode && endNode == cachedEndNode)
            return false;

        // TODO: add parent trace of cached end node to see if the target is
        // on a previously calculated path and if the agent is currently
        // following that same path... if so, just return the old path with
        // the extraneous ends cut off

        foreach (KeyValuePair<Vector2Int, PathNode> keyValue in validNodes)
        {
            keyValue.Value.StaleFlag = true;
        }

        return true;
    }

    public void RecalculateStaleFCosts(List<PathNode> openList, PathNode endNode)
    {
        foreach (PathNode staleOpenNode in openList)
        {
            if (!staleOpenNode.StaleFlag)
                continue;
            PathNode staleNode = staleOpenNode;
            foreach (PathNode parentNode in staleOpenNode.TraceParent())
            {
                // if a node is found that is not stale, update the GCost
                if (!parentNode.StaleFlag)
                {
                    // calculate disparity in GCosts along this path
                    int updatedGCost = parentNode.GCost + CalculateDistanceCost(staleNode, parentNode);
                    int gCostDiff = updatedGCost - staleNode.GCost;
                    // update the calculation that's already been made
                    staleNode.GCost = updatedGCost;
                    // fix disparity on the current open node
                    staleOpenNode.GCost += gCostDiff;
                    // recalculate FCost
                    staleOpenNode.HCost = CalculateDistanceCost(staleOpenNode, endNode);
                    staleOpenNode.CalculateFCost();
                    break;
                }
                staleNode = parentNode;
            }
        }
    }

    public void ClearCachedNodes()
    {
        validNodes.Clear();
        cachedStartNode = null;
        cachedEndNode = null;
        if (openList.Count > 0)
            openList = new List<PathNode>();
        if (closedList.Count > 0)
            closedList = new Dictionary<Vector2Int, PathNode>();
    }
}
