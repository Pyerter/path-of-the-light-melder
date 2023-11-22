using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BufferedInput;

public abstract class PlayerMotion : ScriptableObject, HotSwapSupplier
{
    #region Variables
    [SerializeField] protected string motionName = "PlayerMotion";
    [SerializeField] protected List<string> alternateNames = new List<string>(); // used for clip and state names
    [SerializeField] protected bool shortable = false; // true if this motion can be stopped short
    [SerializeField] protected bool usesMotionSlot = true; // true if this motion uses the hot swap motion slot
    [SerializeField] protected HotSwapAnimation motionAnimation;
    [SerializeField] protected List<string> interruptibleMotions = new List<string>();
    protected StandardControlLocker cachedLocker;
    protected InputData cachedInput;
    protected bool inMotion = false;
    protected bool shouldCancel = false;
    #endregion

    #region Properties
    public string MotionName { get { return motionName; } }
    public IReadOnlyList<string> AlternateNames { get { return alternateNames.AsReadOnly(); } }
    public bool Shortable { get { return shortable; } }
    public bool UsesMotionSlot { get { return usesMotionSlot; } }
    public HotSwapAnimation MotionAnimation { get { return motionAnimation; } }
    public IReadOnlyList<string> InterruptibleMotions { get { return interruptibleMotions.AsReadOnly(); } }
    public StandardControlLocker CachedLocker { get { return cachedLocker; } protected set { cachedLocker = value; } }
    public InputData CachedInput { get { return cachedInput; } protected set { cachedInput = value; } }
    public bool InMotion { get { return inMotion; } protected set { inMotion = value; } }
    public bool ShouldCancel { get { return shouldCancel; } set { shouldCancel = value; } }
    #endregion

    public PlayerMotion GetInstance()
    {
        return Instantiate(this);
    }

    public abstract MotionDataModifierFactory ActivateMotion(PlayerController controller, InputData input, StandardControlLocker locker = null);
    public abstract MotionDataModifierFactory CancelMotion(PlayerController controller, InputData input = null);
    public abstract MotionDataModifierFactory UpdateMotion(PlayerController controller, InputData input = null);
    public virtual bool CanShort(PlayerController controller, InputData input)
    {
        return input.IsReleased() && shortable;
    }

    public virtual MotionData InputMotion(PlayerController controller, InputData input, MotionData motionData, StandardControlLocker locker = null)
    {
        return InputMotion(controller, input, locker).ApplyModifiers(motionData);
    }

    public virtual MotionDataModifierFactory InputMotion(PlayerController controller, InputData input, StandardControlLocker locker = null)
    {
        if (CanActivateMotion(controller, input))
        {
            CachedInput = input;
            CachedLocker = locker;
            ClaimMotionSlot(controller);
            InMotion = true;
            return ActivateMotion(controller, input, locker);
        }
        if (ShouldAcceptInputUpdate(controller, input))
        {
            return UpdateMotion(controller, input);
        }
        if (ShouldCancelMotion(controller, input))
        {
            return CancelMotion(controller, input);
        }
        return default;
    }

    public virtual bool CanActivateMotion(PlayerController controller, InputData input)
    {
        return input.IsPending() && !InMotion && (!UsesMotionSlot || controller.HotSwapMotionController.CanSetCurrentMotion(this));
    }

    public virtual bool ClaimMotionSlot(PlayerController controller)
    {
        if (UsesMotionSlot && controller.HotSwapMotionController.SetCurrentMotion(this, controller))
            return true;
        return false;
    }

    public virtual bool MotionOccupiesSlot(PlayerController controller)
    {
        return controller.HotSwapMotionController.MotionOccupiesSlot(this);
    }

    public virtual bool ShouldCancelMotion(PlayerController controller, InputData input)
    {
        return Shortable && input.IsReleased();
    }

    public virtual bool ShouldCancelMotion(PlayerController controller)
    {
        // Debug.Log("Motion " + MotionName + " occupies slot: " + MotionOccupiesSlot(controller));
        return ShouldCancel || (UsesMotionSlot && !MotionOccupiesSlot(controller));
    }

    public virtual bool TryCancelMotion(PlayerController controller)
    {
        if (InMotion)
        {
            InMotion = false;
            CancelMotion(controller);
            return true;
        }
        return false;
    }

    public virtual bool ShouldAcceptInputUpdate(PlayerController controller, InputData input)
    {
        return false;
    }

    public virtual bool TryTickMotion(PlayerController controller, out MotionDataModifierFactory modifierFactory)
    {
        if (InMotion)
        {
            Debug.Log("Ticking Motion: " + MotionName);
            modifierFactory = TickMotion(controller);
            return true;
        }
        modifierFactory = default;
        return false;
    }

    public virtual MotionDataModifierFactory TickMotion(PlayerController controller)
    {
        if (ShouldCancelMotion(controller))
        {
            Debug.Log("Cancelling motion " + MotionName);
            InMotion = false;
            return CancelMotion(controller);
        }
        return UpdateMotion(controller);
    }

    public virtual bool QueryHotSwap(ComplexAnimatorHotSwapper hotSwapper, out HotSwapAnimation animation)
    {
        animation = null;
        return false;
    }
}
