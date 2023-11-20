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
}
