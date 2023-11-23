using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyPlayerDetection : MonoBehaviour
{
    [Header("Trigger Detection")]
    [SerializeField] protected bool useTriggerDetection = true;
    [SerializeField] protected UnityEvent<EnemyPlayerDetection, PlayerController, bool> onTriggerDetection;

    [Header("Sight Detection")]
    [SerializeField] protected bool useVisionDetection = true;
    [SerializeField] protected LayerMask visionColliders;
    [SerializeField][Range(0, 100f)] protected float visionRange = 20f;
    [SerializeField] protected UnityEvent<EnemyPlayerDetection, PlayerController> onVisionDetection;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (useTriggerDetection)
            AttemptTriggerDetection(collision, true);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (useTriggerDetection)
            AttemptTriggerDetection(collision, false);
    }

    public void AttemptTriggerDetection(Collider2D collision, bool onEnter)
    {
        if (collision.TryGetComponent<PlayerReferenceSupplier>(out PlayerReferenceSupplier controller))
        {
            onTriggerDetection?.Invoke(this, controller.Controller, onEnter);
        }
    }

    public void AttemptVisionDetection()
    {
        if (!useVisionDetection)
            return;
        Transform target = PathfinderManager.Instance.PlayerVisionTarget;
        Vector3 diff = target.position - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, diff, visionRange, visionColliders);
        if (hit.collider != null && hit.collider.TryGetComponent<PlayerReferenceSupplier>(out PlayerReferenceSupplier controller))
        {
            // Debug.Log("Vision detected player, running events!");
            onVisionDetection?.Invoke(this, controller.Controller);
        }
    }

    private void Update()
    {
        if (useVisionDetection)
            AttemptVisionDetection();
    }
}
