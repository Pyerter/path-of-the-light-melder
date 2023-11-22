using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Motion/Hot Swap Animation")]
public class HotSwapAnimation : ScriptableObject
{
    [SerializeField] protected AnimationClip clip;
    public AnimationClip Clip { get { return clip; } }

    [SerializeField] protected string clipName;
    public string ClipName { get { return clip.name; } }

    [SerializeField] protected List<string> alternateNames;
    public IReadOnlyList<string> AlternateNames { get { return alternateNames.AsReadOnly(); } }

    protected int bufferedPlayingDelay = 0;

    public void ResetBufferPlay()
    {
        bufferedPlayingDelay = 0;
    }

    public bool ClipIsPlaying(ComplexAnimatorHotSwapper hotSwapper)
    {
        if (bufferedPlayingDelay < 3)
        {
            bufferedPlayingDelay++;
            return true;
        }

        if (hotSwapper.HotSwapClipIsPlaying(ClipName))
            return true;
        return false;
    }
}
