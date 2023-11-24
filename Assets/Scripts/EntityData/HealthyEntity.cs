using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SignalMessaging;

[RequireComponent(typeof(SignalAcceptor))]
public class HealthyEntity : MonoBehaviour
{
    [SerializeField] protected EntityHealthData health;
    public EntityHealthData Health { get { return health; } }

    [SerializeField] protected bool destroyOnDeath = true;

    [SerializeField] protected UnityEvent<int> onDamaged;

    [SerializeField] protected SignalAcceptor signalAcceptor;
    public SignalAcceptor SignalAcceptor { get { return signalAcceptor; } }

    private void Awake()
    {
        if (signalAcceptor == null)
            signalAcceptor = GetComponent<SignalAcceptor>();

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
