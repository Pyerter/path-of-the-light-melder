using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityTriggerDetectionBehavior : EntityBehavior
{
    public abstract void OnEntityTriggerDetection(EnemyPlayerDetection detector, Transform target, bool detected);
}
