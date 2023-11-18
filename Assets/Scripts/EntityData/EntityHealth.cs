using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EntityHealth
{
    [SerializeField] public int maxHealth;
    [SerializeField] public int health;
    public bool Dead { get { return health <= 0; } }
    public bool Alive { get { return health > 0; } }

    public EntityHealth(int health, int maxHealth)
    {
        this.health = health;
        this.maxHealth = maxHealth;
    }

    public int Damage(int amount)
    {
        health = Mathf.Clamp(health - amount, 0, maxHealth);
        return health;
    }

    public int Heal(int amount)
    {
        health = Mathf.Clamp(health + amount, 0, maxHealth);
        return health;
    }
}
