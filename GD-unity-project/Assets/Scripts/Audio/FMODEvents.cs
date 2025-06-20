using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    [field: Header("Player SFX")]
    [field: SerializeField] public EventReference playerCloseAttackLoad { get; private set; }
    [field: SerializeField] public EventReference playerCloseAttackShoot { get; private set; }
    [field: SerializeField] public EventReference playerDistanceAttackLoad { get; private set; }
    [field: SerializeField] public EventReference playerDistanceAttackShoot { get; private set; }
    [field: SerializeField] public EventReference shieldActivation { get; private set; }
    [field: SerializeField] public EventReference shieldDeactivation { get; private set; }
    [field: SerializeField] public EventReference shieldHit { get; private set; }
    [field: SerializeField] public EventReference playerDie { get; private set; }
    [field: SerializeField] public EventReference doorOpen { get; private set; }
    [field: SerializeField] public EventReference doorClose { get; private set; }
    [field: SerializeField] public EventReference playerDrink { get; private set; }
    [field: SerializeField] public EventReference playerEatChips { get; private set; }
    [field: SerializeField] public EventReference playerEatChocolate { get; private set; }
    [field: SerializeField] public EventReference playerHit { get; private set; }
    [field: SerializeField] public EventReference paperInteraction { get; private set; }
    [field: SerializeField] public EventReference playerFootsteps { get; private set; }
    [field: SerializeField] public EventReference sphereDischarge { get; private set; }
    [field: SerializeField] public EventReference sphereFullRecharge { get; private set; }
    [field: SerializeField] public EventReference sphereRotation { get; private set; }
    [field: SerializeField] public EventReference terminalInteraction { get; private set; }
    [field: SerializeField] public EventReference vendingMachineActivation { get; private set; }
    [field: SerializeField] public EventReference vendingMachineItemPickUp { get; private set; }
    [field: SerializeField] public EventReference playerWakeUp { get; private set; }
    
    [field: Header("Maynard SFX")]
    [field: SerializeField] public EventReference maynardCloseAttack { get; private set; }
    [field: SerializeField] public EventReference maynardDistanceAttack1 { get; private set; }
    [field: SerializeField] public EventReference maynardDistanceAttack2 { get; private set; }
    [field: SerializeField] public EventReference maynardDie { get; private set; }
    [field: SerializeField] public EventReference maynardHitFall { get; private set; }
    [field: SerializeField] public EventReference maynardHitFromBack { get; private set; }
    [field: SerializeField] public EventReference maynardHitFromFront { get; private set; }
    [field: SerializeField] public EventReference maynardHitFromLeftOrRight { get; private set; }
    [field: SerializeField] public EventReference maynardIdle { get; private set; }
    [field: SerializeField] public EventReference maynardFootsteps { get; private set; }
    [field: SerializeField] public EventReference maynardStandUpRoar { get; private set; }
    [field: SerializeField] public EventReference maynardStandUpBreath { get; private set; }
    [field: SerializeField] public EventReference maynardFootstep1 { get; private set; }
    [field: SerializeField] public EventReference maynardFootstep2 { get; private set; }
    
    [field: Header("Drake SFX")]
    [field: SerializeField] public EventReference drakeCloseAttack1 { get; private set; }
    [field: SerializeField] public EventReference drakeCloseAttack2 { get; private set; }
    [field: SerializeField] public EventReference drakeDefense { get; private set; }
    [field: SerializeField] public EventReference drakeDieHit { get; private set; }
    [field: SerializeField] public EventReference drakeDieFoostep1 { get; private set; }
    [field: SerializeField] public EventReference drakeDieFoostep2 { get; private set; }
    [field: SerializeField] public EventReference drakeDieThud { get; private set; }
    [field: SerializeField] public EventReference drakeHitFromBack { get; private set; }
    [field: SerializeField] public EventReference drakeHitFromFront { get; private set; }
    [field: SerializeField] public EventReference drakeHitFromLeftOrRight { get; private set; }
    [field: SerializeField] public EventReference drakeIdle { get; private set; }
    [field: SerializeField] public EventReference drakeFootsteps { get; private set; }
    
    [field: Header("Ambience")]
    [field: SerializeField] public EventReference alarm { get; private set;  }
    [field: SerializeField] public EventReference serverNoise { get; private set;  }
    [field: SerializeField] public EventReference flickeringLED { get; private set;  }
    [field: SerializeField] public EventReference refrigeratorNoise { get; private set;  }
    [field: SerializeField] public EventReference vendingMachineNoise { get; private set;  }
    [field: SerializeField] public EventReference terminalNoise { get; private set;  }
    [field: SerializeField] public EventReference elevatorNoise { get; private set;  }
    [field: SerializeField] public EventReference ventilationNoise { get; private set;  }
    [field: SerializeField] public EventReference flushingWCNoise { get; private set;  }
    
    [field: Header("Music")]
    [field: SerializeField] public EventReference gameplayMusic { get; private set; }
    [field: SerializeField] public EventReference mainMenuMusic { get; private set; }
    
    public static FMODEvents instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one FMOD Events instance in the scene.");
        }
        instance = this;
    }
}