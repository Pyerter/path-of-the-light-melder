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
    public void HotTrigger(bool trig) { if (trig) ComplexAnim.Anim.SetTrigger(hotTrigger); else ComplexAnim.Anim.ResetTrigger(hotTrigger); }
    public void HotSkipTrigger(bool trig) { if (trig) { ComplexAnim.Anim.SetTrigger(hotSkipTrigger); CurrentState?.ClearState(); } else ComplexAnim.Anim.ResetTrigger(hotSkipTrigger); }

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
        bool nullCheck = (motion == null) || (CurrentState == null);
        if (nullCheck)
            return (motion == null) && (CurrentState == null);

        if (CurrentState.NoAnimation)
            return false;

        return CurrentState.CurrentAnimation.ClipName.Equals(motion.MotionAnimation.ClipName);
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
        CurrentState.ClearState();
        CurrentIndex = NextIndex;
        changedOverrides = true;
        HotSkipTrigger(true);
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
        int targetIndex = CurrentState.NoAnimation ? CurrentIndex : NextIndex;
        this[targetIndex].TrySetCurrentAnimation(animation);
        HotBool = true;
        changedOverrides = true;
        //Debug.Log("Added animation");
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
        CurrentState.ClearState();
        HotSkipTrigger(true);
        NextState.TrySetCurrentAnimation(animation, true);
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
        bool currentStateIsPlaying = CurrentState.CurrentAnimation.ClipIsPlaying(this) || HotSwapStateIsName(CurrentState.StateName);
        if (CurrentState.HasAnimation && !currentStateIsPlaying)
        {
            CurrentState.ClearState();
            changedOverrides = true;
            CurrentIndex = NextIndex;
        }

        if (QueueReady && QueryPendingHotSwap(out HotSwapAnimation animation))
        {
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
            CurrentIndex = NextIndex;
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
                return false;
            }
        }

        CurrentIndex = 0;
        if (QueryPendingHotSwap(out HotSwapAnimation animation))
        {
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
