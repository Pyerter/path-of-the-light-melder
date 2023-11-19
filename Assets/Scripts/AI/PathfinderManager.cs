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
}
