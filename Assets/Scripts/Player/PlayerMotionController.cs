using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BufferedInput;

public class PlayerMotionController : MonoBehaviour
{
    [SerializeField] protected Rigidbody2D rb;
    public Rigidbody2D RB { get { if (rb == null) rb = SearchForRB(); return rb; } }
    public bool Valid { get { return rb != null; } }

    [SerializeField] protected HotSwapMotionController hotSwapMotionController;
    public HotSwapMotionController HotSwapMotionController { get { return hotSwapMotionController; } }

    [Header("General Motions")]
    [SerializeField] protected PlayerMotionPair blockMotion;
    [SerializeField] protected PlayerMotionPair punchMotion;
    [SerializeField] protected PlayerMotionPair backstepMotion;
    [SerializeField] protected PlayerMotionPair aerialPunchMotion;
    public PlayerMotionPair BlockMotion { get { return blockMotion; } }
    public PlayerMotionPair PunchMotion { get { return punchMotion; } }
    public PlayerMotionPair BackstepMotion { get { return backstepMotion; } }
    public PlayerMotionPair AerialPunchMotion { get { return aerialPunchMotion; } }
    public PlayerMotionPair[] AllMotions { get { return new PlayerMotionPair[] { BlockMotion, PunchMotion, BackstepMotion, AerialPunchMotion }; } }

    [Header("Movement Variables")]
    [SerializeField] private Collider2D walkingCollider;
    [SerializeField] public PhysicsMaterial2D staticMaterial;
    [SerializeField] public PhysicsMaterial2D kineticMaterial;
    [SerializeField] [Range(0, 50)] private float moveSpeed = 10f;
    [SerializeField] [Range(0, 50)] private float jumpSpeed = 2f;
    [SerializeField] [Range(0, 3)] private float gravityScale = 3f;
    [SerializeField] [Range(10, 300)] private float terminalVelocity = 100;
    [SerializeField] protected bool gravityEnabled = true;
    protected float momentumDeccelMultiplier = 1f;
    [SerializeField] [Range(0, 1)] protected float momentumDecceleration = 0.05f;
    [SerializeField] [Range(0, 1)] protected float aerialMomentumDecceleration = 0.02f;
    [SerializeField] [Range(0, 1)] protected float aerialMovementControl = 0.05f;
    public float MoveSpeed { get { return moveSpeed; } }
    public float JumpSpeed { get { return jumpSpeed; } }
    public float GravityScale { get { return gravityScale; } }
    public float TerminalVelocity { get { return terminalVelocity; } }

    [Header("Grounding")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private TimeState grounded = new TimeState(0, 0); // threshold is ground ghosting time: grounded when off platform
    [SerializeField] private Transform groundCheck;
    [SerializeField] [Range(0, 2)] private float checkToGroundDistance = 0.2f;
    public bool Grounded { get { return grounded; } }

    [Header("Jumping")]
    [SerializeField] private FloatState jumping = new FloatState(0.05f); // threshold is fall speed threshold
    public float GetFallSpeed() { return rb.velocity.y; }
    public bool removeJumpLockOnGrounded = false;
    protected bool aerial = false;
    public bool Jumping { get { return jumping; } }
    public bool Aerial { get { return aerial; } }

    [Header("2D Facing")]
    [SerializeField] private bool flipped = false; // false is right, true is left
    [SerializeField] private bool lockFlip = false;
    public bool LockFlip { get { return lockFlip; } set { lockFlip = value; } }
    public int ForwardMult { get { return flipped ? -1 : 1; } }
    public int VelocityMult { get { return (int)Mathf.Sign(RB.velocity.x); } }

    [Header("Moving")]
    protected bool inputMoving;
    public bool InputMoving { get { return inputMoving; } }

    public Rigidbody2D SearchForRB()
    {
        Rigidbody2D searched = GetComponent<Rigidbody2D>();
        if (!searched)
            searched = GetComponentInParent<Rigidbody2D>();
        if (!searched)
            searched = GetComponentInChildren<Rigidbody2D>();
        return searched;
    }

    private void Awake()
    {
        if (rb == null) {
            rb = SearchForRB();
            if (rb == null)
                Debug.LogError("Using a PlayerMotionController and there is no Rigidbody2D in parents or children! What do I latch onto?!?!?!");
        }

        jumping.valueProvider = GetFallSpeed;

        foreach (PlayerMotionPair motionPair in AllMotions)
        {
            motionPair.Initialize();
        }
    }

    public bool IsGrounded()
    {
        bool hittingGround = Physics2D.Raycast(groundCheck.position, Vector2.down, checkToGroundDistance, groundMask);
        if (hittingGround)
        {
            grounded.Update(true);
            // Debug.Log("Raycast hit ground, current grounded value: " + grounded.previous + " at " + GameManager.Time);
        }
        return grounded.TryUpdateAbove(false);
    }

    public bool IsJumping(PlayerController controller = null)
    {
        bool wasJumping = jumping;

        if (jumping.BelowThreshold())
            jumping.Update(false);

        bool nowJumping = jumping;
        if (!nowJumping && ((wasJumping && controller != null) || (!grounded)))
        {
            removeJumpLockOnGrounded = true;
            aerial = true;
            controller.AddLocker(controller.JumpLocker);
            //Debug.Log("Added jump locker");
        }
        return nowJumping;
    }

    public bool EndJump(PlayerController controller)
    {
        //Debug.Log("Trying to cancel jump");
        if (jumping)
        {
            //Debug.Log("Doing cancel!");
            Vector2 velocity = rb.velocity;
            velocity.y = 0;
            rb.velocity = velocity;
            return true;
        }
        return false;
    }

    public bool Jump(PlayerController controller)
    {
        if (grounded && !jumping)
        {
            ForceJump(controller);
            return true;
        }
        return false;
    }

    public void ForceJump(PlayerController controller, float value = 1f, bool multiplier = true)
    {
        Vector2 velocity = rb.velocity;
        if (multiplier)
            velocity.y = jumpSpeed * value;
        else
            velocity.y = value;
        rb.velocity = velocity;
        jumping.Update(true);
        aerial = true;
    }

    public void Move(PlayerController controller, float movement)
    {
        CheckFlip(movement);
        MoveForward(movement * MoveSpeed);
        inputMoving = true;
    }

    public void StopMoveInput()
    {
        inputMoving = false;
    }

    public void CheckFlip(float forward)
    {
        if (lockFlip)
            return;

        bool shouldFlip = false;
        if (flipped && forward > 0)
            shouldFlip = true;
        if (!flipped && forward < 0)
            shouldFlip = true;
        if (shouldFlip)
        {
            flipped = !flipped;
            Vector3 scale = rb.transform.localScale;
            scale.x *= -1;
            rb.transform.localScale = scale;
        }
    }

    public void MoveForward(float speed, bool aerialControl = false)
    {
        speed = GetAdditiveSpeed(speed);
        Vector2 movement = rb.velocity;
        movement.x = speed;
        if (aerialControl)
            ApplyAerialMovement(movement);
        else
            rb.velocity = movement;
    }

    public void ApplyAerialMovement(Vector2 movement)
    {
        rb.velocity = Vector2.Lerp(rb.velocity, movement, aerialMovementControl);
    }

    public float GetAdditiveSpeed(float speed)
    {
        float moveDirection = speed != 0 ? Mathf.Sign(speed) : ForwardMult;
        float abSpeed = Mathf.Abs(speed);
        bool shareDirection = Mathf.Sign(rb.velocity.x) == Mathf.Sign(speed);

        float currentSpeed = Mathf.Max(Mathf.Abs(rb.velocity.x), abSpeed);

        if (!shareDirection || currentSpeed > abSpeed)
        {
            currentSpeed = abSpeed;
        }
        else if (currentSpeed > abSpeed)
        {
            float momentDeccel = (grounded ? momentumDecceleration : aerialMomentumDecceleration) * momentumDeccelMultiplier;
            currentSpeed = Mathf.Lerp(currentSpeed, abSpeed, momentDeccel);
        }

        if (MathUtility.Equivalent(currentSpeed, 0))
        {
            currentSpeed = 0;
        }

        return currentSpeed * moveDirection;
    }

    public void FlipSpeed()
    {
        Vector2 movement = rb.velocity;
        movement.x = -movement.x;
        rb.velocity = movement;
    }

    public void DragMomentum()
    {
        if (!inputMoving) // Check && inputManager.LockManager.ControlsAllowed(abilityManager.moveLocker.Lock.Locks)
        {
            float currentVelocity = rb.velocity.x;
            if (Mathf.Abs(currentVelocity) > 0)
            {
                momentumDeccelMultiplier = 2f;
                MoveForward(0);
                momentumDeccelMultiplier = 1f;
            }
        }
    }

    public MotionData UpdateMotion(PlayerController controller)
    {
        if (gravityEnabled && rb.velocity.y > -terminalVelocity)
        {
            Vector2 gravity = Vector2.up * GameManager.Instance.Gravity * gravityScale;
            rb.AddForce(gravity, ForceMode2D.Force);
        }

        DragMomentum();
        IsGrounded();
        IsJumping(controller);
        if (grounded && !jumping && removeJumpLockOnGrounded)
        {
            controller.RemoveLocker(controller.JumpLocker);
            removeJumpLockOnGrounded = false;
            aerial = false;
            //Debug.Log("Removed jump locker");
        }

        walkingCollider.sharedMaterial = inputMoving ? kineticMaterial : staticMaterial;
        inputMoving = false;

        MotionData motionData = new MotionData(controller, rb);

        foreach (PlayerMotionPair motionPair in AllMotions)
        {
            motionPair.motion.TryTickMotion(controller, out MotionDataModifierFactory modifierFactory);
        }

        return motionData;
    }
}

[System.Serializable]
public class PlayerMotionPair
{
    [SerializeField] public PlayerMotion motion;
    [SerializeField] public BufferedInput.StandardControlLocker control;
    public PlayerMotionPair(PlayerMotion motion, BufferedInput.StandardControlLocker control)
    {
        this.motion = motion;
        this.control = control;
    }

    public void Initialize()
    {
        if (motion != null)
            motion = motion.GetInstance();
    }
}