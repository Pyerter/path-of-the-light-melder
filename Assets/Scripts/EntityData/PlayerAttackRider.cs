using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerAttackRider : AttackRider
{
    protected PlayerController controller;
    protected bool initialized = false;

    public virtual PlayerAttackRider Initialize(PlayerController controller)
    {
        PlayerAttackRider attackRider = Instantiate(this);
        attackRider.controller = controller;
        return attackRider;
    }
}
