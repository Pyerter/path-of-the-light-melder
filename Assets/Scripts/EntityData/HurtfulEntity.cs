using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtfulEntity : MonoBehaviour
{
    [SerializeField] protected EntityAttackData attackData;
    public EntityAttackData AttackData { get { return attackData; } }

    [SerializeField] protected LayerMask attackLayers;
    public LayerMask AttackLayers { get { return attackLayers; } }

    public bool ObjectInAttackLayers(GameObject obj)
    {
        //return (obj.layer & AttackLayers) > 0;
        return true;
    }
}
