using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ComplexAnimator))]
public class ComplexAnimatorHotSwapper : MonoBehaviour
{
    [SerializeField] protected ComplexAnimator complexAnim;
    public ComplexAnimator ComplexAnim { get { return complexAnim; } }

    [SerializeField] protected string hotBool = "HotBool";
    [SerializeField] protected string hotTrigger = "HotTrigger";
    [SerializeField] protected string hotSkipTrigger = "HotSkipTrigger";

    public bool HotBool { get { return ComplexAnim.Anim.GetBool(hotBool); } set { ComplexAnim.Anim.SetBool(hotBool, value); } }
    public void HotTrigger(bool trig)
    {
        if (trig)
        {
            //ComplexAnim.Anim.SetTrigger(hotTrigger);
            if (ValidateEmptyQueue())
                complexAnim.Anim.Play(hotSwapIdle, hotSwapLayer);
            //Debug.Log("Hot Trigger: Reset Hot Swapper animation index to 0.");
        }
        else
            ComplexAnim.Anim.ResetTrigger(hotTrigger);
    }
    public void HotSkipTrigger(bool trig)
    {
        if (trig)
        {
            //Debug.Log("Skip trigger on animation.");
            CurrentState?.ClearState();
            ComplexAnim.Anim.SetTrigger(hotSkipTrigger);
        }
        else 
            ComplexAnim.Anim.ResetTrigger(hotSkipTrigger);
    }

    [SerializeField] protected string hotSwapIdle = "HotSwapIdle";
    [SerializeField] protected HotSwapState hotSwapState1;
    [SerializeField] protected HotSwapState hotSwapState2;
    [SerializeField] protected HotSwapState hotSwapState3;
    public int Length { get { return 3; } }
    [SerializeField] protected int hotSwapLayer = 2;
    protected int currentIndex;
    public int CurrentIndex { get { return currentIndex; } protected set { currentIndex = value; } }
    public int NextIndex { get { int next = currentIndex + 1; return next >= Length ? 0 : next; } }

    public bool QueueWorking { get { return CurrentState.HasAnimation || NextState.HasAnimation; } }
    public bool QueueEmpty { get { return CurrentState.NoAnimation && NextState.NoAnimation; } }
    public bool QueueLagging { get { return CurrentState.NoAnimation && NextState.HasAnimation; } }
    public bool QueueReady { get { return CurrentState.NoAnimation || NextState.NoAnimation; } }
    public bool QueueFull { get { return CurrentState.HasAnimation && NextState.HasAnimation; } }
    public HotSwapState CurrentState { get { return this[CurrentIndex]; } }
    public HotSwapState NextState { get { return this[NextIndex]; } }

    protected HotSwapSupplier pendingHotSwap = null;
    public HotSwapSupplier PendingHotSwap { get { return pendingHotSwap; } set { pendingHotSwap = value; } }
    public bool HotSwapStateIsName(string name) { return ComplexAnim.Anim.GetCurrentAnimatorStateInfo(hotSwapLayer).IsName(name); }
    public bool HotSwapClipIsPlaying(string name)
    {
        AnimatorClipInfo[] infos = ComplexAnim.Anim.GetCurrentAnimatorClipInfo(hotSwapLayer);
        foreach (AnimatorClipInfo info in infos)
        {
            if (info.clip.name.Equals(name))
                return true;
        }
        //Debug.Log("Clip is not playing: " + name);
        return false;
    }
    public bool MatchesCurrentState(PlayerMotion motion)
    {
        if (motion == null)
            return CurrentState.CurrentAnimation == null;

        bool pendingHotSwapMatches = pendingHotSwap != null && pendingHotSwap.SupplierName.Equals(motion.SupplierName);
        if (CurrentState.NoAnimation)
            return pendingHotSwapMatches;

        return pendingHotSwapMatches || CurrentClipMatchesClipFrom(motion);
    }

    public bool CurrentClipMatchesClipFrom(PlayerMotion motion)
    {
        bool currentMotionEquals = CurrentState.CurrentAnimation.ClipName.Equals(motion.MotionAnimation.ClipName);
        if (!currentMotionEquals)
        {
            foreach (HotSwapAnimation hotSwapAnim in motion.FollowupAnimations)
            {
                if (hotSwapAnim.ClipName.Equals(CurrentState.CurrentAnimation.ClipName))
                {
                    currentMotionEquals = true;
                    break;
                }
            }
        }
        return currentMotionEquals;
    }

    public bool CurrentStateEquals(PlayerMotion motion)
    {
        if (motion == null || CurrentState.NoAnimation)
            return motion == null && CurrentState.NoAnimation;
        //Debug.Log("Current state animation: " + CurrentState.CurrentAnimation.ClipName);
        return CurrentClipMatchesClipFrom(motion);
    }

    protected bool changedOverrides = false;

    private void Awake()
    {
        if (complexAnim == null)
            complexAnim = GetComponent<ComplexAnimator>();

        currentIndex = 0;
    }

    public HotSwapState this[int index]
    {
        get
        {
            switch (index)
            {
                case 0: return hotSwapState1;
                case 1: return hotSwapState2;
                case 2: return hotSwapState3;
            }
            return null;
        }
    }

    public void SkipAnimation()
    {
        HotSkipTrigger(true);
        CurrentIndex = NextIndex;
        changedOverrides = true;
    }

    public void UpdateOverrides()
    {
        GetClipNamePairs(out string[] clipNames, out AnimationClip[] clips);
        ComplexAnim.AnimOverrider.ConfigureAnimations(clipNames, clips);
        changedOverrides = false;
    }

    public void GetClipNamePairs(out string[] clipNames, out AnimationClip[] clips)
    {
        clipNames = new string[this.Length];
        clips = new AnimationClip[this.Length];
        for (int i = 0; i < this.Length; i++)
        {
            clipNames[i] = this[i].StateName;
            clips[i] = this[i].CurrentAnimation != null ? this[i].CurrentAnimation.Clip : null;
        }
    }

    protected void AddAnimation(HotSwapAnimation animation)
    {
        //bool hasCurrent = CurrentState.NoAnimation;
        int targetIndex = CurrentState.NoAnimation ? CurrentIndex : NextIndex;
        this[targetIndex].TrySetCurrentAnimation(animation);
        HotBool = true;
        changedOverrides = true;
        //Debug.Log("Added animation: " + animation.ClipName + " to " + (hasCurrent ? "current state " : "next state ") + targetIndex);
    }

    public bool TryAddAnimation(HotSwapAnimation animation)
    {
        if (QueueFull)
            return false;
        AddAnimation(animation);
        return true;
    }

    public void ForceNextAnimation(HotSwapAnimation animation)
    {
        SkipAnimation();
        CurrentState.TrySetCurrentAnimation(animation, true);
    }

    public void UpdateStates()
    {
        // If not running, validate that animations are indeed empty
        if (!HotBool && ValidateEmptyQueue())
            return;

        // If running, also validate that animations are indeed non-null
        if (!ValidateCurrentAnimations())
            return;

        HotBool = true; // if validations have succeeded, ensure HotBool is true
        // Update the current state to move along if it's done
        bool clipPlaying = CurrentState.CurrentAnimation.ClipIsPlaying(this);
        bool stateIsPlaying = HotSwapStateIsName(CurrentState.StateName);
        bool currentStateIsPlaying = clipPlaying || stateIsPlaying;
        /*for (int i = 0; i < Length; i++)
            if (HotSwapStateIsName(this[i].StateName))
                Debug.Log("Animator current state: " + this[i].StateName);
        Debug.Log("Current animation clip (state " + (CurrentIndex + 1) + ") is: " + CurrentState.CurrentAnimation.ClipName + ".\n Clip is " + 
            (clipPlaying ? "" : "not ") + "playing.\n State is " + (stateIsPlaying ? "" : "not ") + "active.");*/
        if (CurrentState.HasAnimation && !currentStateIsPlaying)
        {
            CurrentState.ClearState();
            changedOverrides = true;
            CurrentIndex = NextIndex;
            //Debug.Log("Removing current animation clip from hot swapper.");
        }

        if (QueueReady && QueryPendingHotSwap(out HotSwapAnimation animation))
        {
            //Debug.Log("Adding animation from query: " + animation.ClipName);
            AddAnimation(animation);
        }

        if (CurrentState.NoAnimation)
        {
            HotBool = false;
            HotTrigger(true);
        }
    }

    public bool ValidateCurrentAnimations()
    {
        if (CurrentState.NoAnimation)
        {
            CurrentIndex = NextIndex;
            //Debug.Log("Skipped current state while validating current animations.");
        }
        bool hasAnimation = CurrentState.HasAnimation;
        if (!hasAnimation)
        {
            if (ValidateEmptyQueue())
            {
                HotBool = false;
                HotTrigger(true);
            } else
            {
                hasAnimation = true;
            }
        }

        return hasAnimation;
    }

    public bool ValidateEmptyQueue()
    {
        for (int i = CurrentIndex; i < CurrentIndex + this.Length; i++)
        {
            int current = i % this.Length;
            if (this[current].HasAnimation)
            {
                CurrentIndex = current;
                changedOverrides = true;
                //Debug.Log("In empty queue validation found animation at index " + current);
                return false;
            }
        }

        CurrentIndex = 0;
        if (QueryPendingHotSwap(out HotSwapAnimation animation))
        {
            //Debug.Log("Added animation from query during empty queue validation: " + animation.ClipName);
            AddAnimation(animation);
            return false;
        }

        // Debug.Log("Empty queue validated - no animations.");
        return true;
    }

    public bool QueryPendingHotSwap(out HotSwapAnimation animation)
    {
        if (PendingHotSwap == null)
        {
            animation = null;
            return false;
        }
        bool hotSwapQuery = PendingHotSwap.QueryHotSwap(this, out animation);
        if (!hotSwapQuery)
            PendingHotSwap = null;
        if (animation != null)
            Debug.Log("Grabbed animation from pending hot swap");
        return animation != null;
    }

    private void FixedUpdate()
    {
        UpdateStates();
        if (changedOverrides)
            UpdateOverrides();
    }

}
