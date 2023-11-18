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
    public void HotSkipTrigger(bool trig) { if (trig) ComplexAnim.Anim.SetTrigger(hotSkipTrigger); else ComplexAnim.Anim.ResetTrigger(hotSkipTrigger); }

    [SerializeField] protected HotSwapState hotSwapState1;
    [SerializeField] protected HotSwapState hotSwapState2;
    [SerializeField] protected HotSwapState hotSwapState3;
    [SerializeField] protected int hotSwapLayer = 2;
    protected HotSwapState[] hotSwaps;
    protected int currentIndex;
    public int CurrentIndex { get { return currentIndex; } protected set { currentIndex = value; } }
    public int NextIndex { get { int next = currentIndex + 1; return next >= hotSwaps.Length ? 0 : next; } }

    public bool QueueWorking { get { return CurrentState.HasAnimation || NextState.HasAnimation; } }
    public bool QueueEmpty { get { return CurrentState.NoAnimation && NextState.NoAnimation; } }
    public bool QueueLagging { get { return CurrentState.NoAnimation && NextState.HasAnimation; } }
    public bool QueueReady { get { return CurrentState.NoAnimation || NextState.NoAnimation; } }
    public bool QueueFull { get { return CurrentState.HasAnimation && NextState.HasAnimation; } }
    public HotSwapState CurrentState { get { return hotSwaps[CurrentIndex]; } }
    public HotSwapState NextState { get { return hotSwaps[NextIndex]; } }

    protected HotSwapSupplier pendingHotSwap = null;
    public HotSwapSupplier PendingHotSwap { get { return pendingHotSwap; } set { pendingHotSwap = value; } }
    public bool HotSwapStateIsName(string name) { return ComplexAnim.Anim.GetCurrentAnimatorStateInfo(hotSwapLayer).IsName(name); }

    protected bool changedOverrides = false;

    private void Awake()
    {
        if (complexAnim == null)
            complexAnim = GetComponent<ComplexAnimator>();

        hotSwaps = new HotSwapState[3] { hotSwapState1, hotSwapState2, hotSwapState3 };
        currentIndex = 0;
    }

    public void UpdateOverrides()
    {
        GetClipNamePairs(out string[] clipNames, out AnimationClip[] clips);
        ComplexAnim.AnimOverrider.ConfigureAnimations(clipNames, clips);
    }

    public void GetClipNamePairs(out string[] clipNames, out AnimationClip[] clips)
    {
        clipNames = new string[hotSwaps.Length];
        clips = new AnimationClip[hotSwaps.Length];
        for (int i = 0; i < hotSwaps.Length; i++)
        {
            clipNames[i] = hotSwaps[i].StateName;
            clips[i] = hotSwaps[i].CurrentAnimation != null ? hotSwaps[i].CurrentAnimation.Clip : null;
        }
    }

    protected void AddAnimation(HotSwapAnimation animation)
    {
        int targetIndex = CurrentState.NoAnimation ? CurrentIndex : NextIndex;
        hotSwaps[targetIndex].TrySetCurrentAnimation(animation);
        HotBool = true;
        changedOverrides = true;
        Debug.Log("Added animation");
    }

    public bool TryAddAnimation(HotSwapAnimation animation)
    {
        if (QueueFull)
            return false;
        AddAnimation(animation);
        return true;
    }

    public void UpdateStates()
    {
        if (!HotBool && ValidateEmptyQueue())
            return;

        HotBool = true; // if validation failed, ensure HotBool is true

        // Update the current state to move along if it's done
        bool currentStateIsPlaying = HotSwapStateIsName(CurrentState.StateName);
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

    public bool ValidateEmptyQueue()
    {
        for (int i = CurrentIndex; i < CurrentIndex + hotSwaps.Length; i++)
        {
            int current = i % hotSwaps.Length;
            if (hotSwaps[current].HasAnimation)
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
