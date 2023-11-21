using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Pair <T, R>
{
    [SerializeField] public T left;
    [SerializeField] public R right;

    public Pair(T left, R right)
    {
        this.left = left;
        this.right = right;
    }
}
