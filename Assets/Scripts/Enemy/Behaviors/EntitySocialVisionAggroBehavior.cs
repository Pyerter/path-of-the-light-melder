using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Entity Behavior/Enemy/Social Vision Aggro")]
public class EntitySocialVisionAggroBehavior : EntityVisionDetectionBehavior
{
    [SerializeField] [Range(0, 100f)] protected float socialAggroRange = 20f;
    [SerializeField] [Range(0, 1f)] protected float aggroDelayMin = 0.1f;
    [SerializeField] [Range(0, 1f)] protected float aggroDelayMax = 0.2f;
    [SerializeField] LayerMask socialAggroLayerMask;

    protected bool receivedAggro = false;
    public bool Aggro { get { return BehaviorActive; } }

    public override bool BehaviorPredicate(EntityController controller)
    {
        return receivedAggro;
    }

    public override void InitiateBehavior(EntityController controller)
    {
        base.InitiateBehavior(controller);
        bool pathfinding = controller.RunPathfinding;
        controller.RunPathfinding = true;
        if (!pathfinding)
        {
            Collider2D[] collisions = Physics2D.OverlapCircleAll(controller.GroundLevelPosition, socialAggroRange, socialAggroLayerMask);
            foreach (Collider2D collision in collisions)
            {
                if (collision.TryGetComponent<EntityController>(out EntityController entity) && !entity.RunPathfinding
                    && entity.BehaviorManager.TryGetBehavior<EntitySocialVisionAggroBehavior>(out EntitySocialVisionAggroBehavior aggroBehavior))
                {
                    Debug.Log("Triggered delayed social aggro.");
                    float delayMax = Mathf.Clamp(aggroDelayMax, aggroDelayMin, 1);
                    controller.StartCoroutine(QueueSocialAggro(null, aggroBehavior, GameManager.Time + Random.Range(aggroDelayMin, delayMax)));
                    break;
                }
            }
        }
    }

    public override bool BehaviorShouldEnd(EntityController controller)
    {
        return !receivedAggro;
    }

    public override void UpdateBehavior(EntityController controller)
    {
        // :)
    }

    public IEnumerator QueueSocialAggro(PlayerController controller, EntitySocialVisionAggroBehavior visionAggro, float threshold)
    {
        yield return new WaitUntil(() => GameManager.Time > threshold);
        visionAggro.receivedAggro = true;
    }

    public override void OnEntityVisionDetection(EnemyPlayerDetection detector, Transform target, bool detected)
    {
        if (detected)
            receivedAggro = detected;
    }
}
