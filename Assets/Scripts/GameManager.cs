using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimeSlowing;

public class GameManager : MonoBehaviour
{
    protected static GameManager instance;
    public static GameManager Instance { get { if (instance == null) instance = FindObjectOfType<GameManager>(); return instance; } }

    protected FloatingTimeController timeController;
    public FloatingTimeController TimeController { get { if (timeController == null) timeController = new FloatingTimeController(SlowUpdateType.FIXED); return timeController; } }
    public static float Time { get { return Instance.TimeController.ControlledTime; } }
    public bool Paused { get { return TimeController.Frozen; } set { TimeController.Frozen = value; } }

    [SerializeField] private float _gravity = -9.81f;
    public float Gravity { get { return _gravity; } private set { _gravity = value; } }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("GameManager instance already exists, deleting new GameManager...");
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        TimeController.Tick();
    }
}
