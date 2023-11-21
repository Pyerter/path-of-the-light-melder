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

    [SerializeField] protected ComplexAnimator complexAnimator;
    public ComplexAnimator ComplexAnimator { get { return complexAnimator; } }

    [SerializeField] protected Transform pathfinderTarget;
    public Transform PathfinderTarget { get { return pathfinderTarget; } }

    [Header("Control Lockers")]
    [SerializeField] protected StandardControlLocker runLocker;
    public StandardControlLocker RunLocker { get { return runLocker; } }
    [SerializeField] protected StandardControlLocker jumpLocker;
    public StandardControlLocker JumpLocker { get { return jumpLocker; } }
    [SerializeField] protected StandardControlLocker fullLocker;
    public StandardControlLocker FullLocker { get { return fullLocker; } }

    public bool Grounded { get { return MotionController.Grounded; } }

    private void Awake()
    {
        if (motionController == null)
            motionController = GetComponentInChildren<PlayerMotionController>();
        if (inputManager == null)
            inputManager = GetComponent<BufferedInputManager>();
        if (complexAnimator == null)
            complexAnimator = GetComponentInChildren<ComplexAnimator>();
    }

    private void FixedUpdate()
    {
        MotionData data = MotionController.UpdateMotion(this);
        complexAnimator.AcceptSettings(data.AsSettings());
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
        MotionController.Move(this, movement.x);
    }

    public void Jump(InputData input)
    {
        if (input.IsPending())
        {
            MotionController.Jump(this);
        } else if (input.IsReleased())
        {
            MotionController.EndJump(this);
        }
    }

    public void Punch(InputData input)
    {
        InputMotion(MotionController.PunchMotion, input);
    }

    public void Block(InputData input)
    {
        InputMotion(MotionController.BlockMotion, input);
    }

    public void Backstep(InputData input)
    {
        InputMotion(MotionController.BackstepMotion, input);
    }

    public void InputMotion(PlayerMotionPair motionPair, InputData input)
    {
        List<AnimatorSetting> settings = MotionData.ApplyModifiers(motionPair.motion.InputMotion(this, input, motionPair.control), new MotionData(ComplexAnimator)).AsSettings();
        ComplexAnimator.AcceptSettings(settings);
    }
}
