using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    [field: Header("Player SFX")]
    [field: SerializeField] public EventReference playerFootsteps { get; private set; }
    [field: SerializeField] public EventReference playerWakeUp { get; private set; }
    [field: SerializeField] public EventReference doorOpen { get; private set; }
    [field: SerializeField] public EventReference doorClose { get; private set; }
    [field: SerializeField] public EventReference paperInteraction { get; private set; }
    [field: SerializeField] public EventReference terminalInteraction { get; private set; }
    [field: SerializeField] public EventReference vendingMachineActivation { get; private set; }
    [field: SerializeField] public EventReference vendingMachineItemPickUp { get; private set; }
    [field: SerializeField] public EventReference sphereDischarge { get; private set; }
    [field: SerializeField] public EventReference sphereRotation { get; private set; }
    [field: SerializeField] public EventReference playerEatChips { get; private set; }
    [field: SerializeField] public EventReference playerEatChocolate { get; private set; }
    [field: SerializeField] public EventReference playerDrink { get; private set; }
    [field: SerializeField] public EventReference shieldActivation { get; private set; }
    [field: SerializeField] public EventReference shieldDeactivation { get; private set; }
    [field: SerializeField] public EventReference shieldHit { get; private set; }
    [field: SerializeField] public EventReference playerHit { get; private set; }
    [field: SerializeField] public EventReference playerDie { get; private set; }
    [field: SerializeField] public EventReference distanceAttackLoad { get; private set; }
    [field: SerializeField] public EventReference distanceAttackShoot { get; private set; }
    [field: SerializeField] public EventReference closeAttackLoad { get; private set; }
    [field: SerializeField] public EventReference closeAttackShoot { get; private set; }

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
    [field: SerializeField] public EventReference music { get; private set; }
    
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