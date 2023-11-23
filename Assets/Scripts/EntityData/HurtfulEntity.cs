using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtfulEntity : MonoBehaviour
{
    [SerializeField] protected EntityAttackData attackData;
    public virtual EntityAttackData AttackData
    {
        get
        {
            EntityAttackData data = supplier != null ? supplier.GetAttackData() : null;
            return data != null ? data : attackData;
        }
    }

    [SerializeField] protected LayerMask attackLayers;
    public LayerMask AttackLayers { get { return attackLayers; } }

    [SerializeField] protected EntityAttackDataSupplier supplier;

    public bool ObjectInAttackLayers(GameObject obj)
    {
        return ((1 << obj.layer) & AttackLayers.value) != 0;
        //return true;
    }
}
