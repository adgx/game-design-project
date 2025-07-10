using System.Threading.Tasks;
using Audio;
using FMOD.Studio;
using PlayerInteraction;
using Unity.VisualScripting;
using UnityEngine;

namespace Animations
{
    public class RickEvents : MonoBehaviour
    {
        // Audio management
        private EventInstance rickLoadCloseAttackWithPowerUp1;
        private EventInstance rickLoadDistanceAttackWithPowerUp1;
        private EventInstance rickLoadCloseAttackWithPowerUp2;
        private EventInstance rickLoadDistanceAttackWithPowerUp2;
        private EventInstance rickWalkFootsteps;
        private EventInstance rickRunFootsteps;
        private EventInstance rickIdle;
        
        // This flag must be set to 'true' by the input script when the attack key is pressed,
        // and to 'false' when released
        public bool ShouldPlayChargeSound { get; set; } = false;

        public PowerUp powerUp;
        public PlayerShoot playerShoot;
        [SerializeField] private FadeManagerLoadingScreen fadeManagerLoadingScreen;
        [DoNotSerialize] public HealthVendingMachineInteraction healthVendingMachineInteraction;
		[DoNotSerialize] public PowerUpVendingMachineInteraction powerUpVendingMachineInteraction;
        [DoNotSerialize] public string machineType; // Can be "playerPowerUp" or "health"

		public void SetHitState()
        {
            AnimationManager.Instance.rickState = RickStates.Hit;
        }
        public void SetIdleState()
        {
            AnimationManager.Instance.rickState = RickStates.Idle;
            playerShoot.FreePlayer();
        }
    
        public void SetHitSpitState()
        {
            AnimationManager.Instance.rickState = RickStates.HitSpit;
        }

        public void Idle()
        {
            // Audio management
            AnimationManager.Instance.rickState = RickStates.Idle;
        }

        public void Walk()
        {
            // Audio management
            AnimationManager.Instance.rickState = RickStates.Walk;
        }

        public void Run()
        {
            // Audio management
            AnimationManager.Instance.rickState = RickStates.Run;
        }

        public void CloseAttackLoad()
        {
            // Audio management
            AnimationManager.Instance.rickState = RickStates.LoadingCloseAttack;
        }

        public void CloseAttackShoot()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerCloseAttackShoot, transform.position);

            playerShoot.FireCloseAttack();
        }

		public void DistanceAttackLoad()
        {
            // Audio management
            AnimationManager.Instance.rickState = RickStates.LoadingDistanceAttack;
        }

        public void DistanceAttackShoot()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDistanceAttackShoot, transform.position);

            playerShoot.FireDistanceAttack();
        }

        public void ShieldActivation()
        {
            AnimationManager.Instance.DefenseVFX(transform.position);

            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerShieldActivation, transform.position);

            if (powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.DefensePowerUp))
            {
                if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DefensePowerUp] == 1)
                {
                    _ = ShieldDeactivationAfterDelay(5000);
                }

                else if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DefensePowerUp] == 2)
                {
                    _ = ShieldDeactivationAfterDelay(10000);
                }
            }
        }

        public void ShieldDeactivation() {
            AnimationManager.Instance.RemoveDefenseVfx();
			playerShoot.CloseShield();
        }

        public void DeathForwardGrunt()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDieForwardGrunt, transform.position);
        }

        public void DeathForwardThud1()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDieForwardThud1, transform.position);
        }

        public void DeathForwardThud2()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDieForwardThud2, transform.position);
        }

        public void DeathBackwardGrunt()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDieBackwardGrunt, transform.position);
        }

        public void DeathBackwardThud()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDieBackwardThud, transform.position);
        }

        public void PlayDeathAnimation() {
            playerShoot.DeathAnimation(0, 1);
        }

        public void DeathAnimationEnd() {
            playerShoot.LoadRespawnScene();
        }

		public void Drink()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDrink, transform.position);
        }

        public void EatChips()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerEatChips, transform.position);
        }

		public void EndPowerUp() {
			powerUpVendingMachineInteraction.TerminatePlayerPowerUp();
		}

		public void EatChocolate()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerEatChocolate, transform.position);
        }

        public void EndHealthRecovery() {
            healthVendingMachineInteraction.TerminateHealthRecovery();
        }

		public void FreePlayerAfterAnimation() {
			playerShoot.FreePlayer();
		}

		public void Hit()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerHit, transform.position);
        }

        public void HitBySpit()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerHitBySpit, transform.position);
        }

        public void HitByBite()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerHitByBite, transform.position);
        }

        public void VendingMachineItemPickup()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerVendingMachineItemPickUp, transform.position);

            if(machineType == "health")
                healthVendingMachineInteraction.PlaceSpecialSnackInHand();
            else {
                if(machineType == "playerPowerUp") {
                    powerUpVendingMachineInteraction.PlaceItemInHand();
                }
			}
        }

        public void WakeUp()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerWakeUp, transform.position);

			fadeManagerLoadingScreen.Hide();
		}

        // Audio management
        private void Start()
        {
            rickLoadCloseAttackWithPowerUp1 = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.PlayerCloseAttackLoadWithPowerUp1);
            rickLoadCloseAttackWithPowerUp1.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));

            rickLoadDistanceAttackWithPowerUp1 = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.PlayerDistanceAttackLoadWithPowerUp1);
            rickLoadDistanceAttackWithPowerUp1.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));

            rickLoadCloseAttackWithPowerUp2 = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.PlayerCloseAttackLoadWithPowerUp2);
            rickLoadCloseAttackWithPowerUp2.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));

            rickLoadDistanceAttackWithPowerUp2 = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.PlayerDistanceAttackLoadWithPowerUp2);
            rickLoadDistanceAttackWithPowerUp2.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));

            rickWalkFootsteps = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.PlayerWalkFootsteps);
            rickWalkFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));

            rickRunFootsteps = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.PlayerRunFootsteps);
            rickRunFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));

            rickIdle = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.PlayerIdle);
            rickIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        }

        // FixedUpdate is called once per frame
        void FixedUpdate()
        {
            // Audio management
            UpdateSound();
        }

        private void UpdateSound()
        {
            rickLoadCloseAttackWithPowerUp1.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            rickLoadDistanceAttackWithPowerUp1.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            rickLoadCloseAttackWithPowerUp2.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            rickLoadDistanceAttackWithPowerUp2.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            rickWalkFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            rickRunFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            rickIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            
            RickStates currentState = AnimationManager.Instance.rickState;
            
            // Condition for close-loading audio:
            // Must be in the correct state and the isLoadingSoundPlaying flag must be true
            bool shouldPlayCloseLoad = currentState == RickStates.LoadingCloseAttack && ShouldPlayChargeSound;
            if (powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.CloseAttackPowerUp))
            {
                HandleLoopingSound(rickLoadCloseAttackWithPowerUp1, shouldPlayCloseLoad && powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.CloseAttackPowerUp] == 1);
                HandleLoopingSound(rickLoadCloseAttackWithPowerUp2, shouldPlayCloseLoad && powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.CloseAttackPowerUp] == 2);
            }

            // Condition for remote loading audio:
            bool shouldPlayDistanceLoad = currentState == RickStates.LoadingDistanceAttack && ShouldPlayChargeSound;
            if (powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp))
            {
                HandleLoopingSound(rickLoadDistanceAttackWithPowerUp1, shouldPlayDistanceLoad && powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp] == 1);
                HandleLoopingSound(rickLoadDistanceAttackWithPowerUp2, shouldPlayDistanceLoad && powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp] == 2);
            }

            // Motion and idle sounds
            HandleLoopingSound(rickWalkFootsteps, currentState == RickStates.Walk);
            HandleLoopingSound(rickRunFootsteps, currentState == RickStates.Run);
            HandleLoopingSound(rickIdle, currentState == RickStates.Idle);
        }

        // Helper method to reduce code duplication for loop sounds
        private void HandleLoopingSound(EventInstance instance, bool shouldBePlaying)
        {
            instance.getPlaybackState(out PLAYBACK_STATE playbackState);
    
            if (shouldBePlaying)
            {
                if (playbackState == PLAYBACK_STATE.STOPPED)
                {
                    instance.start();
                }
            }
            else
            {
                if (playbackState != PLAYBACK_STATE.STOPPING && playbackState != PLAYBACK_STATE.STOPPED)
                {
                    instance.stop(STOP_MODE.ALLOWFADEOUT);
                }
            }
        }

        // Audio management
        private async Task ShieldDeactivationAfterDelay(int delayMs)
        {
            await Task.Delay(delayMs);

            Debug.Log("ShieldDeactivation");

            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerShieldDeactivation, transform.position);
        }
    }
}