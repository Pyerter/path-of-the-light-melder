using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthyEntity : MonoBehaviour
{
    [SerializeField] protected EntityHealthData health;
    public EntityHealthData Health { get { return health; } }

    [SerializeField] protected bool destroyOnDeath = true;

    private void Awake()
    {
        Health.RestoreToFull();
        if (destroyOnDeath)
            Health.onDeath += () =>
            {
                Destroy(gameObject);
            };
    }

    public int Damage(int amount)
    {
        return health.Damage(amount);
    }
}
