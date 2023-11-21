using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotSwapMotionController : MonoBehaviour
{
    [SerializeField] protected ComplexAnimatorHotSwapper hotSwapper;
    public ComplexAnimatorHotSwapper HotSwapper { get { return hotSwapper; } }

    // TODO: Make this class to control the one motion which can use the hot swapper
    // or use this to transition between motions in the hot swapper
}
