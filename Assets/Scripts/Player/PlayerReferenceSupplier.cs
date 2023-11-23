using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReferenceSupplier : MonoBehaviour
{
    [SerializeField] protected PlayerController controller;
    public PlayerController Controller
    {
        get
        {
            if (controller == null)
            {
                controller = GetComponent<PlayerController>();
                if (controller == null)
                {
                    controller = GetComponentInParent<PlayerController>();
                }
                if (controller == null)
                {
                    controller = GetComponentInChildren<PlayerController>();
                }
            }
            return controller;
        }
    }
}
