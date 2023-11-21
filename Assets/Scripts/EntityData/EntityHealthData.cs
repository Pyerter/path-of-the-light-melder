using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityHealthData
{
    [SerializeField] protected int maxHealth;
    [SerializeField] protected int health;
    public int Health { 
        get { return health; }
        set
        {
            health = Mathf.Clamp(value, 0, maxHealth);
            if (Dead && !triggeredDeath)
            {
                triggeredDeath = true;
                onDeath?.Invoke();
            }
        }
    }
    public bool Dead { get { return health <= 0; } }
    public bool Alive { get { return health > 0; } }

    protected bool triggeredDeath = false;
    public event System.Action onDeath;

    public EntityHealthData(int health, int maxHealth)
    {
        this.health = health;
        this.maxHealth = maxHealth;
    }

    public void RestoreToFull()
    {
        health = maxHealth;
    }

    public int Damage(int amount)
    {
        Health -= amount;
        Debug.Log("Entity damage! Now at " + Health + " health.");
        return Health;
    }

    public int Heal(int amount)
    {
        Health += amount;
        return Health;
    }
}
