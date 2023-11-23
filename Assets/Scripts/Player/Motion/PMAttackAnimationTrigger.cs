using BufferedInput;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Motion/Motions/Attack Animation Trigger")]
public class PMAttackAnimationTrigger : PMAnimationTrigger
{
    [SerializeField] List<PlayerAttackRider> playerAttackRiders = new List<PlayerAttackRider>();
    protected List<PlayerAttackRider> initAttackRiders = new List<PlayerAttackRider>();
    protected bool attackRiderInitialized = false;

    protected override PlayerMotion Init()
    {
        PMAttackAnimationTrigger initializedMotion = (PMAttackAnimationTrigger)base.Init();
        initializedMotion.attackRiderInitialized = false;
        return initializedMotion;
    }

    public override MotionDataModifierFactory ActivateMotion(PlayerController controller, InputData input, StandardControlLocker locker = null)
    {
        // Debug.Log("Attack animation initialized: " + attackRiderInitialized);
        MotionDataModifierFactory modifierFactory = base.ActivateMotion(controller, input, locker);
        if (!attackRiderInitialized)
        {
            foreach (PlayerAttackRider attackRider in playerAttackRiders)
            {
                attackData.attackRiders.Add(attackRider.Initialize(controller));
            }
            Debug.Log("Initialized attack riders for animation: " + motionName);
            attackRiderInitialized = true;
        }
        return modifierFactory;
    }
}
