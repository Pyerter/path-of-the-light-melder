using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthyEntitySupplier : MonoBehaviour
{
    [SerializeField] protected HealthyEntity healthyEntity;
    public HealthyEntity HealthyEntity { get { return healthyEntity; } }
}
