using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BufferedInput;

[RequireComponent(typeof(BufferedInputManager))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] protected BufferedInputManager inputManager;
    public BufferedInputManager InputManager { get { return inputManager; } }

    [SerializeField] protected PlayerMotionController motionController;
    public PlayerMotionController MotionController { get { return motionController; } }

    [Header("Control Lockers")]
    [SerializeField] protected StandardControlLocker runLocker;
    public StandardControlLocker RunLocker { get { return runLocker; } }
    [SerializeField] protected StandardControlLocker jumpLocker;
    public StandardControlLocker JumpLocker { get { return jumpLocker; } }
    [SerializeField] protected StandardControlLocker fullLocker;
    public StandardControlLocker FullLocker { get { return fullLocker; } }

    public bool Grounded { get { return true; } }

    private void Awake()
    {
        if (motionController == null)
            motionController = GetComponentInChildren<PlayerMotionController>();
        if (inputManager == null)
            inputManager = GetComponent<BufferedInputManager>();
    }

    private void FixedUpdate()
    {
        MotionController.UpdateMotion(this);
    }

    public void AddLocker(StandardControlLocker locker)
    {
        InputManager.LockManager.AddLocker(locker);
    }

    public void RemoveLocker(StandardControlLocker locker)
    {
        InputManager.LockManager.RemoveLocker(locker);
    }

    public void Move(InputData input)
    {
        Vector2 movement = input.Direction.current;
        MotionController.Move(movement.x);
    }

    public void Jump(InputData input)
    {
        if (input.IsPending())
        {
            MotionController.Jump();
        }
    }

    public void Punch(InputData input)
    {
        MotionController.PunchMotion.motion.InputMotion(this, input, MotionController.PunchMotion.control);
    }
}
