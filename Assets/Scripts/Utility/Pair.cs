using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Pair <T, R>
{
    public T left;
    public R right;

    public Pair(T left, R right)
    {
        this.left = left;
        this.right = right;
    }
}
