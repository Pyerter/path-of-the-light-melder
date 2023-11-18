using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MotionData
{
    public float runSpeed;
    public float jumpSpeed;
    public bool grounded;
    public List<AnimatorSetting> settings;
    public bool nonDefault;

    public MotionData(float runSpeed, float jumpSpeed, bool grounded, List<AnimatorSetting> settings = null)
    {
        this.runSpeed = runSpeed;
        this.jumpSpeed = jumpSpeed;
        this.grounded = grounded;
        if (settings == null)
            this.settings = new List<AnimatorSetting>();
        else
            this.settings = settings;
        nonDefault = true;
    }

    public MotionData(PlayerController controller, List<AnimatorSetting> settings = null) : this(controller, controller.MotionController.RB, settings)
    {
    }

    public MotionData(PlayerController controller, Rigidbody2D rb, List<AnimatorSetting> settings = null)
    {
        Vector3 velocity = rb.velocity;
        runSpeed = velocity.x;
        jumpSpeed = velocity.y;
        grounded = controller.Grounded;
        if (settings == null)
            this.settings = new List<AnimatorSetting>();
        else
            this.settings = settings;
        nonDefault = true;
    }

    public MotionData(ComplexAnimator animator, List<AnimatorSetting> settings = null)
    {
        runSpeed = animator.Anim.GetFloat("RunSpeed");
        jumpSpeed = animator.Anim.GetFloat("JumpSpeed");
        grounded = animator.Anim.GetBool("Grounded");
        if (settings == null)
            this.settings = new List<AnimatorSetting>();
        else
            this.settings = settings;
        nonDefault = true;
    }

    public List<AnimatorSetting> AsSettings()
    {
        List<AnimatorSetting> newSettings = new List<AnimatorSetting>();
        newSettings.Add(new AnimatorFloatSetting("RunSpeed", Mathf.Abs(runSpeed)));
        newSettings.Add(new AnimatorFloatSetting("JumpSpeed", jumpSpeed));
        newSettings.Add(new AnimatorBoolSetting("Grounded", grounded));
        newSettings.AddRange(settings);
        return newSettings;
    }

    public static MotionData ApplyModifiers(List<MotionDataModifier> modifiers, MotionData data)
    {
        foreach (MotionDataModifier mod in modifiers)
        {
            data = mod.ModifyData(data);
        }
        return data;
    }
}

public interface MotionDataModifier
{
    public MotionData ModifyData(MotionData data);
}

public struct MotionDataRunModifier : MotionDataModifier
{
    public float runSpeed;
    public MotionDataRunModifier(float runSpeed)
    {
        this.runSpeed = runSpeed;
    }

    public MotionData ModifyData(MotionData data)
    {
        data.runSpeed = runSpeed;
        return data;
    }
}

public struct MotionDataJumpModifier : MotionDataModifier
{
    public float jumpSpeed;
    public MotionDataJumpModifier(float jumpSpeed)
    {
        this.jumpSpeed = jumpSpeed;
    }

    public MotionData ModifyData(MotionData data)
    {
        data.jumpSpeed = jumpSpeed;
        return data;
    }
}

public struct MotionDataGroundedModifier : MotionDataModifier
{
    public bool grounded;
    public MotionDataGroundedModifier(bool grounded)
    {
        this.grounded = grounded;
    }

    public MotionData ModifyData(MotionData data)
    {
        data.grounded = grounded;
        return data;
    }
}

public struct MotionDataSettingsModifier : MotionDataModifier
{
    public AnimatorSetting setting;
    public MotionDataSettingsModifier(AnimatorSetting setting)
    {
        this.setting = setting;
    }

    public MotionData ModifyData(MotionData data)
    {
        data.settings.Add(setting);
        return data;
    }
}