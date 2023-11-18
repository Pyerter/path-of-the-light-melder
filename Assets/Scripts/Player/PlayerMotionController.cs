using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BufferedInput;

public class PlayerMotionController : MonoBehaviour
{
    [SerializeField] protected Rigidbody2D rb;
    public Rigidbody2D RB { get { if (rb == null) rb = SearchForRB(); return rb; } }
    public bool Valid { get { return rb != null; } }

    [Header("General Motions")]
    [SerializeField] protected PlayerMotionPair blockMotion;
    [SerializeField] protected PlayerMotionPair punchMotion;
    public PlayerMotionPair BlockMotion { get { return blockMotion; } }
    public PlayerMotionPair PunchMotion { get { return punchMotion; } }

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
    [SerializeField] private FloatState jumping = new FloatState(0); // threshold is fall speed threshold
    public float GetFallSpeed() { return rb.velocity.y; }
    public bool removeJumpLockOnGrounded = false;

    [Header("2D Facing")]
    [SerializeField] private bool flipped = false; // false is right, true is left
    public int ForwardMult { get { return flipped ? -1 : 1; } }

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

        blockMotion.Initialize();
        punchMotion.Initialize();
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

    public bool IsJumping()
    {
        if (jumping.BelowThreshold() && grounded)
            jumping.Update(false);
        return jumping;
    }

    public bool Jump(PlayerController controller)
    {
        if (grounded && !IsJumping())
        {
            Vector2 velocity = rb.velocity;
            velocity.y = jumpSpeed;
            rb.velocity = velocity;
            removeJumpLockOnGrounded = true;
            controller.AddLocker(controller.JumpLocker);
            Debug.Log("Added jump locker");
            return true;
        }
        return false;
    }

    public void Move(PlayerController controller, float movement)
    {
        CheckFlip(movement);
        MoveForward(movement * MoveSpeed);
        inputMoving = true;
    }

    public void CheckFlip(float forward)
    {
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
        speed = GetAdditiveSpeed(speed, ForwardMult);
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

    public float GetAdditiveSpeed(float speed, float moveDirection = 1)
    {
        speed = Mathf.Abs(speed);
        float currentSpeed = Mathf.Max(Mathf.Abs(rb.velocity.x), speed);
        if (currentSpeed > speed)
        {
            float momentDeccel = (grounded ? momentumDecceleration : aerialMomentumDecceleration) * momentumDeccelMultiplier;
            currentSpeed = Mathf.Lerp(currentSpeed, speed, momentDeccel);
        } 
        else if (currentSpeed < speed)
        {
            currentSpeed = speed;
        }
        else if (MathUtility.Equivalent(currentSpeed, 0))
        {
            currentSpeed = 0;
        }
        return currentSpeed * moveDirection;
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
        IsJumping();
        if (grounded && !jumping && removeJumpLockOnGrounded)
        {
            controller.RemoveLocker(controller.JumpLocker);
            removeJumpLockOnGrounded = false;
            Debug.Log("Removed jump locker");
        }

        walkingCollider.sharedMaterial = inputMoving ? kineticMaterial : staticMaterial;
        inputMoving = false;

        MotionData motionData = new MotionData(controller, rb);

        if (punchMotion.motion.ActiveMotion)
        {
            punchMotion.motion.UpdateMotion(controller, motionData);
        }

        return motionData;
    }
}

[System.Serializable]
public struct PlayerMotionPair
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