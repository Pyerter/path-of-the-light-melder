using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathfinderManager : MonoBehaviour
{
    protected static PathfinderManager instance;
    public static PathfinderManager Instance { get { if (instance == null) instance = FindObjectOfType<PathfinderManager>(); return instance; } }

    [SerializeField] Tilemap groundTilemap;
    public Tilemap GroundTiles { get { return groundTilemap; } }

    protected Transform playerTarget;
    public Transform PlayerTarget
    {
        get
        {
            if (playerTarget == null)
            {
                PlayerController controller = FindObjectOfType<PlayerController>();
                playerTarget = controller.PathfinderTarget;
            }
            return playerTarget;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else if (instance != this)
        {
            Debug.LogWarning("Instance is already set, deleting new pathfinder manager...");
        }
    }

    public Vector2 GetCellSize()
    {
        Vector2 cellSize = GroundTiles.cellSize;
        return new Vector2(cellSize.x, cellSize.y);
    }

    public Vector2 GetGridPosition(Vector3 pos)
    {
        Vector3 cell = GroundTiles.WorldToCell(pos);
        cell.x += 0.5f;
        cell.y += 0.5f;
        return cell;
    }

    public Vector2Int GetCellPosition(Vector3 pos)
    {
        Vector3Int cell = GroundTiles.WorldToCell(pos);
        return new Vector2Int(cell.x, cell.y);
    }

    public static Vector2 GetPlayerTargetCellPosition(Tilemap tilemap, Vector2 pos)
    {
        Vector3Int end3 = tilemap.WorldToCell(pos);
        Vector2Int end = new Vector2Int(end3.x, end3.y);
        end = GetNodeBelow(tilemap, end);
        end3 = new Vector3Int(end.x, end.y, 0);
        return tilemap.GetCellCenterWorld(end3);
    }

    public static Vector2Int GetNodeBelow(Tilemap tilemap, Vector2Int pos)
    {
        Vector3Int previousSafe = new Vector3Int(pos.x, pos.y, 0);
        int maxSearchBelow = 10;
        if (tilemap.HasTile(previousSafe))
            return pos;
        for (int i = 0; i >= -maxSearchBelow; i--)
        {
            Vector3Int current = previousSafe + Vector3Int.down;
            if (!tilemap.HasTile(previousSafe) && tilemap.HasTile(current))
            {
                return new Vector2Int(previousSafe.x, previousSafe.y);
            }
            previousSafe = current;
        }
        return pos;
    }
}
