using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(AnimatorOverrider))]
public class ComplexAnimator : MonoBehaviour
{
    [SerializeField] protected Animator anim;
    public Animator Anim { get { if (anim == null) anim = GetComponent<Animator>(); return anim; } }

    [SerializeField] protected AnimatorOverrider animOverrider;
    public AnimatorOverrider AnimOverrider { get { return animOverrider; } }

    private void Awake()
    {
        if (anim == null)
            anim = GetComponent<Animator>();
        if (animOverrider == null)
            animOverrider = GetComponent<AnimatorOverrider>();

        animOverrider.InitializeAnimator(anim);
    }

    public void AcceptSettings(List<AnimatorSetting> settings)
    {
        foreach (AnimatorSetting setting in settings)
        {
            setting.ApplySetting(this);
        }
    }

    public bool StateNameIs(string name, int layerIndex = 0)
    {
        return Anim.GetCurrentAnimatorStateInfo(layerIndex).IsName(name);
    }
}

public interface AnimatorSetting
{
    public abstract void ApplySetting(ComplexAnimator animator);
}

public struct AnimatorBoolSetting : AnimatorSetting
{
    public string name;
    public bool setting;
    public bool trigger;
    public AnimatorBoolSetting(string name, bool setting, bool trigger = false)
    {
        this.name = name;
        this.setting = setting;
        this.trigger = trigger;
    }

    public void ApplySetting(ComplexAnimator animator)
    {
        if (trigger)
        {
            if (setting)
                animator.Anim.SetTrigger(name);
            return;
        }

        animator.Anim.SetBool(name, setting);
    }
}

public struct AnimatorIntSetting : AnimatorSetting
{
    public string name;
    public int setting;
    public AnimatorIntSetting(string name, int setting)
    {
        this.name = name;
        this.setting = setting;
    }

    public void ApplySetting(ComplexAnimator animator)
    {
        animator.Anim.SetInteger(name, setting);
    }
}

public struct AnimatorFloatSetting : AnimatorSetting
{
    public string name;
    public float setting;
    public AnimatorFloatSetting(string name, float setting)
    {
        this.name = name;
        this.setting = setting;
    }

    public void ApplySetting(ComplexAnimator animator)
    {
        animator.Anim.SetFloat(name, setting);
    }
}