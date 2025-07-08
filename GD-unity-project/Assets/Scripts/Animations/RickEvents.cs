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

            if (powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.CloseAttackPowerUp))
            {
                // Start loadCloseAttack event if Rick is loading the close attack 
                if (currentState == RickStates.LoadingCloseAttack) 
                {
                    // Get the playback state for the loadCloseAttack event
                    PLAYBACK_STATE loadCloseAttackPlaybackState;

                    if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.CloseAttackPowerUp] == 1)
                    {
                        rickLoadCloseAttackWithPowerUp1.getPlaybackState(out loadCloseAttackPlaybackState);
                        if (loadCloseAttackPlaybackState.Equals(PLAYBACK_STATE.STOPPED))
                        {
                            rickLoadCloseAttackWithPowerUp1.start();
                            _ = StopLoadingSoundAfterDelay(rickLoadCloseAttackWithPowerUp1, 1500, currentState);
                        }
                    }
                    else if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.CloseAttackPowerUp] == 2)
                    {
                        rickLoadCloseAttackWithPowerUp2.getPlaybackState(out loadCloseAttackPlaybackState);
                        if (loadCloseAttackPlaybackState.Equals(PLAYBACK_STATE.STOPPED))
                        {
                            rickLoadCloseAttackWithPowerUp2.start();
                            _ = StopLoadingSoundAfterDelay(rickLoadCloseAttackWithPowerUp2, 2500, currentState);
                        }
                    }
                }
                // Otherwise, stop the loadCloseAttack event
                else
                {
                    if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.CloseAttackPowerUp] == 1)
                    {
                        rickLoadCloseAttackWithPowerUp1.stop(STOP_MODE.ALLOWFADEOUT);
                    }
                    else if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.CloseAttackPowerUp] == 2)
                    {
                        rickLoadCloseAttackWithPowerUp2.stop(STOP_MODE.ALLOWFADEOUT);
                    }
                }
            }

            if (powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp))
            {
                // Start loadDistanceAttack event if Rick is loading the distance attack 
                if (currentState == RickStates.LoadingDistanceAttack)
                {
                    // Get the playback state for the loadDistanceAttack event
                    PLAYBACK_STATE loadDistanceAttackPlaybackState;

                    if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp] == 1)
                    {
                        rickLoadDistanceAttackWithPowerUp1.getPlaybackState(out loadDistanceAttackPlaybackState);
                        if (loadDistanceAttackPlaybackState.Equals(PLAYBACK_STATE.STOPPED))
                        {
                            rickLoadDistanceAttackWithPowerUp1.start();
                            _ = StopLoadingSoundAfterDelay(rickLoadDistanceAttackWithPowerUp1, 1500, currentState);
                        }
                    }
                    else if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp] == 2)
                    {
                        rickLoadDistanceAttackWithPowerUp2.getPlaybackState(out loadDistanceAttackPlaybackState);
                        if (loadDistanceAttackPlaybackState.Equals(PLAYBACK_STATE.STOPPED))
                        {
                            rickLoadDistanceAttackWithPowerUp2.start();
                            _ = StopLoadingSoundAfterDelay(rickLoadDistanceAttackWithPowerUp2, 2500, currentState);
                        }
                    }
                }
                // Otherwise, stop the loadDistanceAttack event
                else
                {
                    if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp] == 1)
                    {
                        rickLoadDistanceAttackWithPowerUp1.stop(STOP_MODE.ALLOWFADEOUT);
                    }
                    else if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp] == 2)
                    {
                        rickLoadDistanceAttackWithPowerUp2.stop(STOP_MODE.ALLOWFADEOUT);
                    }
                }
            }

            // Start walkFootsteps event if Rick is moving
            if (currentState == RickStates.Walk)
            {
                // Get the playback state for the walkFootsteps event
                PLAYBACK_STATE walkFootstepsPlaybackState;
                rickWalkFootsteps.getPlaybackState(out walkFootstepsPlaybackState);
                if (walkFootstepsPlaybackState.Equals(PLAYBACK_STATE.STOPPED))
                {
                    rickWalkFootsteps.start();
                }
            }
            // Otherwise, stop the walkFootsteps event
            else
            {
                rickWalkFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
            }

            // Start runFootsteps event if Rick is moving fast
            if (currentState == RickStates.Run)
            {
                // Get the playback state for the runFootsteps event
                PLAYBACK_STATE runFootstepsPlaybackState;
                rickRunFootsteps.getPlaybackState(out runFootstepsPlaybackState);
                if (runFootstepsPlaybackState.Equals(PLAYBACK_STATE.STOPPED))
                {
                    rickRunFootsteps.start();
                }
            }
            // Otherwise, stop the runFootsteps event
            else
            {
                rickRunFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
            }

            // Start idle event if Rick is using the idle animation
            if (currentState == RickStates.Idle)
            {
                // Get the playback state for the idle event
                PLAYBACK_STATE idlePlaybackState;
                rickIdle.getPlaybackState(out idlePlaybackState);
                if (idlePlaybackState.Equals(PLAYBACK_STATE.STOPPED))
                {
                    rickIdle.start();
                }
            }
            // Otherwise, stop the idle event
            else
            {
                rickIdle.stop(STOP_MODE.ALLOWFADEOUT);
            }
            
            HandleLoopingSound(rickWalkFootsteps, currentState == RickStates.Walk);
            HandleLoopingSound(rickRunFootsteps, currentState == RickStates.Run);
            HandleLoopingSound(rickIdle, currentState == RickStates.Idle);
        }

        // Helper method to reduce code duplication for loop sounds
        private void HandleLoopingSound(EventInstance instance, bool shouldBePlaying)
        {
            if (shouldBePlaying)
            {
                instance.getPlaybackState(out PLAYBACK_STATE playbackState);
                if (playbackState == PLAYBACK_STATE.STOPPED)
                {
                    instance.start();
                }
            }
            else
            {
                instance.stop(STOP_MODE.ALLOWFADEOUT);
            }
        }

        // Audio management
        private async Task StopLoadingSoundAfterDelay(EventInstance instance, int delayMs, RickStates currentState)
        {
            await Task.Delay(delayMs);

            if (!instance.isValid())
                return;

            PLAYBACK_STATE state;
            var result = instance.getPlaybackState(out state);
            if (result != FMOD.RESULT.OK)
                return;

            if (state != PLAYBACK_STATE.STOPPED)
                instance.stop(STOP_MODE.ALLOWFADEOUT);

            if (currentState == RickStates.LoadingCloseAttack && AnimationManager.Instance.rickState == currentState)
            {
                AnimationManager.Instance.rickState = RickStates.EndAreaAttack;
            }
            else if(AnimationManager.Instance.rickState == currentState)
            {
                AnimationManager.Instance.rickState = RickStates.EndAttack;   
            }
        }

        private async Task ShieldDeactivationAfterDelay(int delayMs)
        {
            await Task.Delay(delayMs);

            Debug.Log("ShieldDeactivation");

            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerShieldDeactivation, transform.position);
        }
    }
}