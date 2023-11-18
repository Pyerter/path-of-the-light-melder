using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface HealthyEntity
{
    public EntityHealth Health { get; }
    public int Damage(int amount);
}
