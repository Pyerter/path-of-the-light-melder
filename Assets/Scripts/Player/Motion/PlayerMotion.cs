using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BufferedInput;

public abstract class PlayerMotion : ScriptableObject, HotSwapSupplier
{
    #region Variables
    [SerializeField] protected string motionName = "PlayerMotion";
    [SerializeField] protected List<string> alternateNames = new List<string>(); // used for clip and state names
    [SerializeField] protected HotSwapAnimation motionAnimation;
    [SerializeField] protected List<HotSwapAnimation> followupAnimations = new List<HotSwapAnimation>();
    [SerializeField] protected List<string> interruptibleMotions = new List<string>();
    [SerializeField] protected bool shortable = false; // true if this motion can be stopped short
    [SerializeField] protected bool usesMotionSlot = true; // true if this motion uses the hot swap motion slot
    [SerializeField] protected bool providesAttackData = false;
    [SerializeField] protected EntityAttackData attackData = null;
    protected StandardControlLocker cachedLocker;
    protected InputData cachedInput;
    protected bool inMotion = false;
    protected bool shouldCancel = false;
    protected int motionIndex = 0;
    protected bool yieldNextMotion = false;
    #endregion

    #region Properties
    public string MotionName { get { return motionName; } }
    public IReadOnlyList<string> AlternateNames { get { return alternateNames.AsReadOnly(); } }
    public HotSwapAnimation MotionAnimation { get { return motionAnimation; } }
    public IReadOnlyList<HotSwapAnimation> FollowupAnimations { get { return followupAnimations.AsReadOnly(); } }
    public IReadOnlyList<string> InterruptibleMotions { get { return interruptibleMotions.AsReadOnly(); } }
    public bool Shortable { get { return shortable; } }
    public bool UsesMotionSlot { get { return usesMotionSlot; } }
    public bool ProvidesAttackData { get { return providesAttackData; } }
    public StandardControlLocker CachedLocker { get { return cachedLocker; } protected set { cachedLocker = value; } }
    public InputData CachedInput { get { return cachedInput; } protected set { cachedInput = value; } }
    public bool InMotion { get { return inMotion; } protected set { inMotion = value; } }
    public bool ShouldCancel { get { return shouldCancel; } set { shouldCancel = value; } }
    #endregion

    public PlayerMotion GetInstance()
    {
        PlayerMotion initializedMotion = Instantiate(this);
        return initializedMotion.Init();
    }

    protected virtual PlayerMotion Init()
    {
        return this;
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
            motionIndex = -1;
            yieldNextMotion = false;
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

    public virtual bool MotionActivelyOccupiesSlot(PlayerController controller)
    {
        return controller.HotSwapMotionController.MotionActivelyOccupiesSlot(this);
    }

    public virtual bool ShouldCancelMotion(PlayerController controller, InputData input)
    {
        return Shortable && input.IsReleased();
    }

    public virtual bool ShouldCancelMotion(PlayerController controller)
    {
        // Debug.Log("Motion " + MotionName + " occupies slot: " + MotionOccupiesSlot(controller));
        bool cancels = ShouldCancel;
        bool notPlaying = (UsesMotionSlot && !MotionActivelyOccupiesSlot(controller));
        bool cancelOrNotPlaying = cancels || notPlaying;
        // Debug.Log("Not playing " + MotionName + ": " + notPlaying);
        if (cancelOrNotPlaying && (followupAnimations.Count - 1 > motionIndex))
        {
            //Debug.Log("Checking for buffered input on mask: " + cachedInput.Mask.ToStringFormatted(12));
            if (controller.InputManager.BufferedInputExists(cachedInput.Mask, out InputData input) && input.IsPending() && input != cachedInput)
            {
                //Debug.Log("Found input for buffered mask: " + cachedInput.Mask.ToStringFormatted(12) + " to continue followup!");
                input.ProcessStage = InputProcessStage.PROCESSING;
                cachedInput = input;
                motionIndex++;
                yieldNextMotion = true;
                cancelOrNotPlaying = false;
            }
        }
        if (cancelOrNotPlaying)
            cancelOrNotPlaying = MotionOccupiesSlot(controller);
        return cancelOrNotPlaying;
    }

    public virtual bool TryCancelMotion(PlayerController controller)
    {
        if (InMotion)
        {
            InMotion = false;
            ShouldCancel = false;
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
            //Debug.Log("Ticking Motion: " + MotionName);
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
            //Debug.Log("Cancelling motion " + MotionName + " from the motion.");
            InMotion = false;
            ShouldCancel = false;
            controller.HotSwapMotionController.TryCancelCurrentMotion(this);
            return CancelMotion(controller);
        }
        return UpdateMotion(controller);
    }

    // Hot Swap Supplier interface
    public virtual string SupplierName { get { return MotionName; } }

    public virtual bool QueryHotSwap(ComplexAnimatorHotSwapper hotSwapper, out HotSwapAnimation animation)
    {
        //Debug.Log("Querying hot swap.");
        if (motionIndex <= followupAnimations.Count - 1)
        {
            if (yieldNextMotion)
            {
                animation = followupAnimations[motionIndex];
                yieldNextMotion = false;
            }
            else
                animation = null;
            return true;
        }
        animation = null;
        //Debug.Log("Hot swap query being removed.");
        return false;
    }

    // Attack data supplier

    public virtual EntityAttackData GetAttackData()
    {
        return providesAttackData ? attackData : null;
    }
}
