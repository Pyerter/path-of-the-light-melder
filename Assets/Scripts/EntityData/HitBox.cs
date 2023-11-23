using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    [SerializeField] protected HurtfulEntity hurtfulEntity;
    [SerializeField] protected float hitCooldown = 0.5f;

    protected Dictionary<Collider2D, float> hitTimes = new Dictionary<Collider2D, float>();

    public void ClearHitCooldowns()
    {
        hitTimes.Clear();
    }

    public bool TryHitTime(Collider2D collider)
    {
        if (!hitTimes.ContainsKey(collider))
        {
            hitTimes.Add(collider, GameManager.Time);
            return true;
        }

        if (GameManager.Time > hitTimes[collider] + hitCooldown)
        {
            hitTimes[collider] = GameManager.Time;
            return true;
        }

        return false;
    }

    public bool TryHit(Collider2D collider)
    {
        if (!hurtfulEntity.ObjectInAttackLayers(collider.gameObject))
            return false;

        bool hits = collider.TryGetComponent<HealthyEntity>(out HealthyEntity entity);
        if (!hits && collider.TryGetComponent<HealthyEntitySupplier>(out HealthyEntitySupplier supplier))
        {
            hits = true;
            entity = supplier.HealthyEntity;
        }
        if (hits && TryHitTime(collider))
        {
            hurtfulEntity?.AttackData.Damage(entity);
            return true;
        }
        return false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryHit(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryHit(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        hitTimes.Remove(collision);
    }
}
