using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthyEntity : MonoBehaviour
{
    [SerializeField] protected EntityHealthData health;
    public EntityHealthData Health { get { return health; } }

    [SerializeField] protected bool destroyOnDeath = true;

    [SerializeField] protected UnityEvent<int> onDamaged;

    private void Awake()
    {
        Health.RestoreToFull();
        if (destroyOnDeath)
            Health.onDeath += () =>
            {
                Destroy(gameObject);
            };
    }
    
    public void NotifyDamage(EntityAttackData data)
    {
        onDamaged?.Invoke(data.damage);
    }
}
