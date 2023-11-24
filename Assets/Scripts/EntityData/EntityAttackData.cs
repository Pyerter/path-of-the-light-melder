using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[System.Serializable]
public class EntityAttackData
{
    [SerializeField] public int damage;
    [SerializeField] public List<AttackRider> attackRiders;
    [SerializeField] public List<string> attackSignals; // TODO: create signal struct that has a list of parameters that can be assigned

    public virtual void Damage(HealthyEntity entity)
    {
        entity.Health.Damage(damage);
        foreach (AttackRider attackRider in attackRiders)
        {
            attackRider.OnAttack(entity, this);
        }
        entity.NotifyDamage(this);
        foreach (string signal in attackSignals)
        {
            entity.SignalAcceptor.TriggerSignal(signal);
        }
    }
}
