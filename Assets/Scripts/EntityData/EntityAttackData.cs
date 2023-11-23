using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityAttackData
{
    [SerializeField] public int damage;
    [SerializeField] public List<AttackRider> attackRiders;

    public EntityAttackData(int damage, List<AttackRider> attackRiders = null)
    {
        this.damage = damage;
        this.attackRiders = attackRiders != null ? attackRiders : new List<AttackRider>();
    }

    public virtual void Damage(HealthyEntity entity)
    {
        entity.Health.Damage(damage);
        foreach (AttackRider attackRider in attackRiders)
        {
            attackRider.OnAttack(entity, this);
        }
        entity.NotifyDamage(this);
    }
}
