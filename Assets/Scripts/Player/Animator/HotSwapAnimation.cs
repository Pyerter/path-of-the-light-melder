using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Motion/Hot Swap Animation")]
public class HotSwapAnimation : ScriptableObject
{
    [SerializeField] protected AnimationClip clip;
    public AnimationClip Clip { get { return clip; } }
}
