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
    [field: SerializeField] public EventReference MaynardCloseAttack { get; private set; }
    [field: SerializeField] public EventReference MaynardDistanceAttack1 { get; private set; }
    [field: SerializeField] public EventReference MaynardDistanceAttack2 { get; private set; }
    [field: SerializeField] public EventReference MaynardDie { get; private set; }
    [field: SerializeField] public EventReference MaynardHitFall { get; private set; }
    [field: SerializeField] public EventReference MaynardHitFromBack { get; private set; }
    [field: SerializeField] public EventReference MaynardHitFromFront { get; private set; }
    [field: SerializeField] public EventReference MaynardHitFromLeftOrRight { get; private set; }
    [field: SerializeField] public EventReference MaynardIdle { get; private set; }
    [field: SerializeField] public EventReference MaynardFootsteps { get; private set; }
    [field: SerializeField] public EventReference MaynardStandUpRoar { get; private set; }
    [field: SerializeField] public EventReference MaynardStandUpBreath { get; private set; }
    [field: SerializeField] public EventReference MaynardFootstep1 { get; private set; }
    [field: SerializeField] public EventReference MaynardFootstep2 { get; private set; }
    
    [field: Header("Drake SFX")]
    [field: SerializeField] public EventReference DrakeCloseAttack1 { get; private set; }
    [field: SerializeField] public EventReference DrakeCloseAttack2 { get; private set; }
    [field: SerializeField] public EventReference DrakeDefense { get; private set; }
    [field: SerializeField] public EventReference DrakeDieHit { get; private set; }
    [field: SerializeField] public EventReference DrakeDieFoostep1 { get; private set; }
    [field: SerializeField] public EventReference DrakeDieFoostep2 { get; private set; }
    [field: SerializeField] public EventReference DrakeDieThud { get; private set; }
    [field: SerializeField] public EventReference DrakeHitFromFrontOrBack { get; private set; }
    [field: SerializeField] public EventReference DrakeHitFromLeftOrRight { get; private set; }
    [field: SerializeField] public EventReference DrakeIdle { get; private set; }
    [field: SerializeField] public EventReference DrakeFootsteps { get; private set; }
    
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