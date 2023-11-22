using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator), typeof(AnimatorOverrider))]
public class ComplexAnimator : MonoBehaviour
{
    [SerializeField] protected Animator anim;
    public Animator Anim { get { if (anim == null) anim = GetComponent<Animator>(); return anim; } }

    [SerializeField] protected AnimatorOverrider animOverrider;
    public AnimatorOverrider AnimOverrider { get { return animOverrider; } }

    [SerializeField] protected ComplexAnimatorHotSwapper hotSwapper;
    public ComplexAnimatorHotSwapper HotSwapper { get { if (hotSwapper == null) hotSwapper = GetComponent<ComplexAnimatorHotSwapper>(); return hotSwapper; } }

    [SerializeField] List<UnityEvent> animationEvents;

    private void Awake()
    {
        if (anim == null)
            anim = GetComponent<Animator>();
        if (animOverrider == null)
            animOverrider = GetComponent<AnimatorOverrider>();

        animOverrider.InitializeAnimator(anim);
        Anim.keepAnimatorStateOnDisable = true;
        Anim.writeDefaultValuesOnDisable = true;
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

    public bool ClipNameIs(string name, int layerIndex = 0)
    {
        foreach (AnimatorClipInfo clip in Anim.GetCurrentAnimatorClipInfo(layerIndex))
        {
            if (clip.clip.name.Equals(name))
                return true;
        }
        return false;
    }

    public void TriggerEvent(int index)
    {
        if (index >= animationEvents.Count)
            return;
        animationEvents[index].Invoke();
        Debug.Log("Triggered animation event: " + index);
    }

    public void ResetSprite(SpriteRenderer spriteRenderer)
    {
        spriteRenderer.sprite = null;
        Debug.Log("Set sprite of " + spriteRenderer.name + " to null.");
    }
}