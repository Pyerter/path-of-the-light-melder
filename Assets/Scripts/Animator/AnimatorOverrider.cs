using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorOverrider : MonoBehaviour
{
    protected Animator initialAnim;
    [SerializeField] protected AnimatorOverrideController currentAnim;
    protected RuntimeAnimatorController currentRuntimeController;
    public RuntimeAnimatorController CurrentRuntimeController { get { return currentAnim != null ? currentAnim : currentRuntimeController; } }
    public bool UsingOverrides { get { return currentAnim != null; } }

    protected AnimationClipOverrides overrides;

    public void InitializeAnimator(Animator anim)
    {
        initialAnim = anim;
        if (currentAnim == null)
        {
            currentRuntimeController = initialAnim.runtimeAnimatorController;
            Debug.LogWarning("No AnimatorOverrideController set for this AnimatorOverrider: " + gameObject.name + ", not using overrides.");
        }
        else
        {
            initialAnim.runtimeAnimatorController = currentAnim;
            overrides = new AnimationClipOverrides(currentAnim.overridesCount);
            currentAnim.GetOverrides(overrides);
        }
    }

    public void ConfigureAnimations(string[] clipNames, AnimationClip[] clips)
    {
        if (!UsingOverrides)
        {
            Debug.Log("Not using overrides because no AnimatorOverrideController is set. Cannot configure animations.");
            return;
        }

        string clipsDebugMessage = "Player AnimationClips Debug:\n\n";
        string tab1 = "  ";
        string tab2 = "    ";
        clipsDebugMessage += tab1 + "All key value pairs:\n";
        foreach (KeyValuePair<AnimationClip, AnimationClip> pair in overrides)
        {
            string output = tab2 + "Key: ";
            if (pair.Key != null)
            {
                output += pair.Key.name;
            }
            else
            {
                output += "null";
            }

            output += ", Value: ";
            if (pair.Value != null)
            {
                output += pair.Value.name;
            }
            else
            {
                output += "null";
            }
            output += "\n";
            clipsDebugMessage += output;
        }
        clipsDebugMessage += "\n" + tab1 + "Clip replacement info:\n";
        clipsDebugMessage += tab1 + "Clip Names Length: " + clipNames.Length + ", Clips Length: " + clips.Length + "\n";
        for (int i = 0; i < clipNames.Length && i < clips.Length; i++)
        {
            if (clips[i] != null)
            {
                overrides[clipNames[i]] = clips[i];
                clipsDebugMessage += tab2 + overrides.recentDebugMessage;
            } else
            {
                clipsDebugMessage += tab2 + "Missing Clip: " + clipNames[i];
            }
            clipsDebugMessage += "\n";
        }
        Debug.Log(clipsDebugMessage);
        currentAnim.ApplyOverrides(overrides);
    }
}
