using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityVisionDetectionBehavior : EntityBehavior
{
    public abstract void OnEntityVisionDetection(EnemyPlayerDetection detector, Transform target, bool detected);
}
