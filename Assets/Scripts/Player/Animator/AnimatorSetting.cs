using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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