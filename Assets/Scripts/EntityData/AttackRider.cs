using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackRider : ScriptableObject
{
    public abstract void OnAttack(HealthyEntity entity, EntityAttackData attackData);
}
