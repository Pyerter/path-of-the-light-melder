using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface MotionDataModifier
{

    public MotionDataModifierType ModifierType { get; }
    public MotionData ModifyData(MotionData data);
}

public enum MotionDataModifierType
{
    RUN,
    JUMP,
    GROUNDED,
    SETTING
}

public class MotionDataModifierFactory
{
    
    protected List<MotionDataModifier> modifiers;
    public bool ModifiesMotion { get { return modifiers.Count > 0; } }
    public MotionDataModifierFactory()
    {
        modifiers = new List<MotionDataModifier>();
    }
    public MotionDataModifierFactory(MotionDataModifierFactory modifierFactory)
    {
        modifiers = new List<MotionDataModifier>();
        modifiers.AddRange(modifierFactory.modifiers);
    }

    public MotionDataModifierFactory AddRunModifier(float runSpeed)
    {
        modifiers.Add(new MotionDataRunModifier(runSpeed));
        return this;
    }

    public MotionDataModifierFactory AddJumpModifier(float jumpSpeed)
    {
        modifiers.Add(new MotionDataJumpModifier(jumpSpeed));
        return this;
    }

    public MotionDataModifierFactory AddGroundedModifier(bool grounded)
    {
        modifiers.Add(new MotionDataGroundedModifier(grounded));
        return this;
    }

    public MotionDataModifierFactory AddSettingModifier(AnimatorSetting setting)
    {
        modifiers.Add(new MotionDataSettingsModifier(setting));
        return this;
    }

    public MotionDataModifierFactory MergeFactory(MotionDataModifierFactory target)
    {
        MotionDataModifierFactory mergedFactory = new MotionDataModifierFactory(target);
        mergedFactory.modifiers.AddRange(modifiers);
        return mergedFactory;
    }

    public MotionData ApplyModifiers(MotionData motionData)
    {
        foreach (MotionDataModifier modifier in modifiers)
        {
            motionData = modifier.ModifyData(motionData);
        }
        return motionData;
    }
}

public struct MotionDataRunModifier : MotionDataModifier
{
    public MotionDataModifierType ModifierType { get { return MotionDataModifierType.RUN; } }
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
    public MotionDataModifierType ModifierType { get { return MotionDataModifierType.JUMP; } }
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
    public MotionDataModifierType ModifierType { get { return MotionDataModifierType.GROUNDED; } }
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
    public MotionDataModifierType ModifierType { get { return MotionDataModifierType.SETTING; } }
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