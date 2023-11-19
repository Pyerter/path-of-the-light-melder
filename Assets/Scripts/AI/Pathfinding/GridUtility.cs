using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridUtility
{
    public static List<PathNode> GetNearValidNode(Tilemap tilemap, PathNode node, System.Func<Vector2Int, PathNode> nodeGenerator, System.Func<Tilemap, PathNode, PathNode, bool> nodePredicate = null, int distance = 1)
    {
        if (distance < 1)
            return new List<PathNode>();
        if (nodeGenerator == null)
            nodeGenerator = (vec) => new PathNode(vec);
        if (nodePredicate == null)
            nodePredicate = (tilemap, node, previous) => NodeIsStandable(tilemap, node, 1, 0);

        List<PathNode> nodes = new List<PathNode>();
        for (int i = -distance; i <= distance; i++)
        {
            for (int j = -distance; j <= distance; j++)
            {
                if (i == 0 && j == 0)
                    continue;
                Vector2Int currentPosition = new Vector2Int(node.Position.x + i, node.Position.y + j);
                PathNode currentNode = nodeGenerator.Invoke(currentPosition);
                if (nodePredicate.Invoke(tilemap, currentNode, node))
                {
                    nodes.Add(currentNode);
                    //Debug.Log("Added node " + currentPosition + " to the pathfinding.");
                }
            }
        }

        return nodes;
    }

    public static bool NodeIsStandable(Tilemap tilemap, PathNode node, int height, int offset)
    {
        return node.StandablePosition(tilemap, height, offset);
    }

    public static void InsertIntoSortedList<T>(List<T> list, T element, IComparer<T> comparer)
    {
        int index = list.BinarySearch(element, comparer);
        if (index < 0) index = ~index;
        list.Insert(index, element);
    }

    public static bool TryPopFromSortedList<T>(List<T> list, T target, IComparer<T> comparer, out T popped)
    {
        int index = list.BinarySearch(target, comparer);
        if (index < 0) {
            popped = default;
            return false;
        }
        popped = list[index]; 
        list.RemoveAt(index);
        return true;
    }

    public static bool TryGetIndexInSortedList<T>(List<T> list, T element, IComparer<T> comparer, out int index)
    {
        index = list.BinarySearch(element, comparer);
        return index >= 0;
    }

    public static bool SortedListContains<T>(List<T> list, T element, IComparer<T> comparer)
    {
        int index = list.BinarySearch(element, comparer);
        return index >= 0;
    }
}
