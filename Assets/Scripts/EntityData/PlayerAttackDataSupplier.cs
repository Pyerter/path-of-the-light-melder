using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackDataSupplier : EntityAttackDataSupplier
{
    [SerializeField] protected PlayerController controller;

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<PlayerController>();
    }

    public override EntityAttackData GetAttackData()
    {
        if (controller != null)
        {
            return controller.GetAttackData();
        }
        return null;
    }
}
