using FMODUnity;
using UnityEngine;

namespace Audio
{
    public class FMODEvents : MonoBehaviour
    {
        [field: Header("Player SFX")]
        [field: SerializeField] public EventReference PlayerCloseAttackLoadWithPowerUp1 { get; private set; }
        [field: SerializeField] public EventReference PlayerCloseAttackLoadWithPowerUp2 { get; private set; }
        [field: SerializeField] public EventReference PlayerCloseAttackShoot { get; private set; }
        [field: SerializeField] public EventReference PlayerDistanceAttackLoadWithPowerUp1 { get; private set; }
        [field: SerializeField] public EventReference PlayerDistanceAttackLoadWithPowerUp2 { get; private set; }
        [field: SerializeField] public EventReference PlayerDistanceAttackShoot { get; private set; }
        [field: SerializeField] public EventReference PlayerShieldActivation { get; private set; }
        [field: SerializeField] public EventReference PlayerShieldDeactivation { get; private set; }
        [field: SerializeField] public EventReference PlayerShieldHit { get; private set; }
        [field: SerializeField] public EventReference PlayerDieBackwardGrunt { get; private set; }
        [field: SerializeField] public EventReference PlayerDieBackwardThud { get; private set; }
        [field: SerializeField] public EventReference PlayerDieForwardGrunt { get; private set; }
        [field: SerializeField] public EventReference PlayerDieForwardThud1 { get; private set; }
        [field: SerializeField] public EventReference PlayerDieForwardThud2 { get; private set; }
        [field: SerializeField] public EventReference PlayerDoorOpen { get; private set; }
        [field: SerializeField] public EventReference PlayerDoorClose { get; private set; }
        [field: SerializeField] public EventReference PlayerDrink { get; private set; }
        [field: SerializeField] public EventReference PlayerEatChips { get; private set; }
        [field: SerializeField] public EventReference PlayerEatChocolate { get; private set; }
        [field: SerializeField] public EventReference PlayerHit { get; private set; }
        [field: SerializeField] public EventReference PlayerHitBySpit { get; private set; }
        [field: SerializeField] public EventReference PlayerHitByBite { get; private set; }
        [field: SerializeField] public EventReference PlayerIdle { get; private set; }
        [field: SerializeField] public EventReference PlayerPaperInteraction { get; private set; }
        [field: SerializeField] public EventReference PlayerWalkFootsteps { get; private set; }
        [field: SerializeField] public EventReference PlayerRunFootsteps { get; private set; }
        [field: SerializeField] public EventReference PlayerSphereDischarge { get; private set; }
        [field: SerializeField] public EventReference PlayerSphereFullRecharge { get; private set; }
        [field: SerializeField] public EventReference PlayerSphereRotation { get; private set; }
        [field: SerializeField] public EventReference PlayerTerminalInteraction { get; private set; }
        [field: SerializeField] public EventReference PlayerVendingMachineActivation { get; private set; }
        [field: SerializeField] public EventReference PlayerVendingMachineItemPickUp { get; private set; }
        [field: SerializeField] public EventReference PlayerWakeUp { get; private set; }
    
        [field: Header("Maynard SFX")]
        [field: SerializeField] public EventReference MaynardCloseAttack { get; private set; }
        [field: SerializeField] public EventReference MaynardDistanceAttack1 { get; private set; }
        [field: SerializeField] public EventReference MaynardDistanceAttack2 { get; private set; }
        [field: SerializeField] public EventReference MaynardDieScream { get; private set; }
        [field: SerializeField] public EventReference MaynardDieThud { get; private set; }
        [field: SerializeField] public EventReference MaynardHitFallScream { get; private set; }
        [field: SerializeField] public EventReference MaynardHitFallThud { get; private set; }
        [field: SerializeField] public EventReference MaynardHitFromBack { get; private set; }
        [field: SerializeField] public EventReference MaynardHitFromFront { get; private set; }
        [field: SerializeField] public EventReference MaynardHitFromLeftOrRight { get; private set; }
        [field: SerializeField] public EventReference MaynardIdle { get; private set; }
        [field: SerializeField] public EventReference MaynardFootsteps { get; private set; }
        [field: SerializeField] public EventReference MaynardStandUpRoar { get; private set; }
        [field: SerializeField] public EventReference MaynardStandUpFootstep1 { get; private set; }
        [field: SerializeField] public EventReference MaynardStandUpFootstep2 { get; private set; }
        [field: SerializeField] public EventReference MaynardStandUpBreath { get; private set; }
    
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
        
        [field: Header("Incognito SFX")]
        [field: SerializeField] public EventReference IncognitoDistanceAttack1 { get; private set; }
        [field: SerializeField] public EventReference IncognitoDistanceAttack2Load { get; private set; }
        [field: SerializeField] public EventReference IncognitoDistanceAttack2Spit { get; private set; }
        [field: SerializeField] public EventReference IncognitoDieGrunt { get; private set; }
        [field: SerializeField] public EventReference IncognitoDieThud1 { get; private set; }
        [field: SerializeField] public EventReference IncognitoDieThud2 { get; private set; }
        [field: SerializeField] public EventReference IncognitoHitFromFront1 { get; private set; }
        [field: SerializeField] public EventReference IncognitoHitFromFront2 { get; private set; }
        [field: SerializeField] public EventReference IncognitoHitFromLeftOrRight { get; private set; }
        [field: SerializeField] public EventReference IncognitoHitFallScream { get; private set; }
        [field: SerializeField] public EventReference IncognitoHitFallFootstep1 { get; private set; }
        [field: SerializeField] public EventReference IncognitoHitFallFootstep2 { get; private set; }
        [field: SerializeField] public EventReference IncognitoHitFallThud { get; private set; }
        [field: SerializeField] public EventReference IncognitoIdle { get; private set; }
        [field: SerializeField] public EventReference IncognitoFootsteps { get; private set; }
        [field: SerializeField] public EventReference IncognitoStandUpFootstep1 { get; private set; }
        [field: SerializeField] public EventReference IncognitoStandUpFootstep2 { get; private set; }
    
        [field: Header("Ambience")]
        [field: SerializeField] public EventReference Alarm { get; private set;  }
        [field: SerializeField] public EventReference ServerNoise { get; private set;  }
        [field: SerializeField] public EventReference FlickeringLed { get; private set;  }
        [field: SerializeField] public EventReference RefrigeratorNoise { get; private set;  }
        [field: SerializeField] public EventReference VendingMachineNoise { get; private set;  }
        [field: SerializeField] public EventReference TerminalNoise { get; private set;  }
        [field: SerializeField] public EventReference ElevatorNoise { get; private set;  }
        [field: SerializeField] public EventReference VentilationNoise { get; private set;  }
        [field: SerializeField] public EventReference FlushingWcNoise { get; private set;  }
    
        [field: Header("Music")]
        [field: SerializeField] public EventReference MainMenuMusic { get; private set; }
        [field: SerializeField] public EventReference GameplayMusic { get; private set; }
        [field: SerializeField] public EventReference DeathMenuMusic { get; private set; }
    
        public static FMODEvents Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("Found more than one FMOD Events instance in the scene.");
            }
            Instance = this;
        }
    }
}