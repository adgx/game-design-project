using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Audio;
using FMOD.Studio;
using PlayerInteraction;
using UnityEngine;

namespace Animations
{
    public class RickEvents : MonoBehaviour
    {
        // Audio management: looping sounds
        private EventInstance rickLoadCloseAttackWithPowerUp1;
        private EventInstance rickLoadDistanceAttackWithPowerUp1;
        private EventInstance rickLoadCloseAttackWithPowerUp2;
        private EventInstance rickLoadDistanceAttackWithPowerUp2;
        private EventInstance rickWalkFootsteps;
        private EventInstance rickRunFootsteps;
        private EventInstance rickIdle;
        private EventInstance rickHeartbeat;
        private EventInstance rickSphereRotation;
        
        // Audio management: one-shot sounds
        private List<EventInstance> activeOneShotInstances = new List<EventInstance>();
        
        // Audio management
        [Header("Audio Sources")]
        [SerializeField] private GameObject rotatingSphereSource; 
        private bool isHitSoundPending = false;
        private bool shouldPlayHeartbeat = false;

        //Player
        [SerializeField] private Player _player;
        // Defense
        [SerializeField] private GameObject magneticShieldPrefab;
        private GameObject shield;

        // This flag must be set to 'true' by the input script when the attack key is pressed,
        // and to 'false' when released
        public bool ShouldPlayChargeSound { get; set; } = false;

        public PowerUp powerUp;
        public PlayerShoot playerShoot;
        [SerializeField] private FadeManagerLoadingScreen fadeManagerLoadingScreen;
        [NonSerialized] public HealthVendingMachineInteraction HealthVendingMachineInteraction;
		[NonSerialized] public PowerUpVendingMachineInteraction PowerUpVendingMachineInteraction;
        [NonSerialized] public string MachineType; // Can be "playerPowerUp" or "health"

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
            
            rickHeartbeat = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.PlayerHeartbeat);
            rickHeartbeat.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            
            rickSphereRotation = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.PlayerSphereRotation);
            rickSphereRotation.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(rotatingSphereSource.transform));
        }

        // FixedUpdate is called once per frame
        void FixedUpdate()
        {
            // Audio management
            UpdateSound();
        }

        public void DisableRickState()
        {
            AnimationManager.Instance.rickState = RickStates.None;
        }

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
            PlayManagedEvent(FMODEvents.Instance.PlayerCloseAttackShoot);
            
            AreaAttackController areaController = playerShoot.attackAreaInstance.GetComponent<AreaAttackController>();
            if (areaController != null)
            {
                areaController.Initialize(playerShoot.chargedCloseAttackDamage);
            }
            
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
            PlayManagedEvent(FMODEvents.Instance.PlayerCloseAttackShoot);

            playerShoot.FireDistanceAttack();
        }

        private void DefenseVFX()
        {
            shield = Instantiate(magneticShieldPrefab, transform.position, Quaternion.identity, transform);
            shield.tag = "Shield";
            shield.gameObject.SetActive(true);

            // Audio management
            PlayManagedEvent(FMODEvents.Instance.PlayerShieldActivation);

            if (powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.DefensePowerUp))
            {
                if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DefensePowerUp] == 1)
                {
                    shield.GetComponent<ShieldTrigger>().SetLifeTime(3.5f);
                }
                else if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DefensePowerUp] == 2)
                {
                    shield.GetComponent<ShieldTrigger>().SetLifeTime(4.5f);
                }
            }
            else
            {
                shield.GetComponent<ShieldTrigger>().SetLifeTime(2.5f);
            }

        }

        public void ShieldActivation()
        {
            // Activate the shield
            DefenseVFX();
            
            // Let the player free to move during the usage of the shield
            _ = UnfreezePlayerAfterDelay(700);
            
            // Deactivate the shield
            ShieldDeactivation();
        }

        private void ShieldDeactivation()
        {
            //reset the idle state
            AnimationManager.Instance.rickState = RickStates.Idle;
            // Audio management
            float delay = 2.5f; // Default value
            if (powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.DefensePowerUp))
            {
                if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DefensePowerUp] == 1) delay = 3.5f;
                else if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DefensePowerUp] == 2) delay = 4.5f;
            }
            StartCoroutine(ShieldDestructionAfterDelay(delay));
        }

        private void ShieldDestroy()
        {
            if (shield != null)
            {
                // Disable the entire GameObject instantly. This removes it from both view and all physical
                // systems, including that of particles
                shield.SetActive(false);
        
                // Queue ultimate destruction to free memory. This will happen at the end of the frame,
                // but it does not matter anymore because the object is already inactive
                Destroy(shield);
                
                // Explicitly sets the variable to null, allowing the creation of a new shield
                shield = null; 
                
                // Notify PlayerShoot that the shield is no longer active
                if (playerShoot != null)
                {
                    playerShoot.SetShieldIsActive(false);
                }
            }
        }

        public void DeathForwardGrunt()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.PlayerDieForwardGrunt);

            playerShoot.SetLayerToZero();
        }

        public void DeathForwardThud1()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.PlayerDieForwardThud1);
        }

        public void DeathForwardThud2()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.PlayerDieForwardThud2);
        }

        public void DeathBackwardGrunt()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.PlayerDieBackwardGrunt);

            playerShoot.SetLayerToZero();
        }

        public void DeathBackwardThud()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.PlayerDieBackwardThud);
        }

        public void PlayDeathAnimation()
        {
            playerShoot.DeathAnimation(1, 1);
        }

        public void DeathAnimationEnd()
        {
            playerShoot.LoadRespawnScene();
        }

        public void Drink()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.PlayerDrink);
        }

        public void EatChips()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.PlayerEatChips);
        }

        public void EndPowerUp()
        {
            PowerUpVendingMachineInteraction.TerminatePlayerPowerUp();
        }

        public void EatChocolate()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.PlayerEatChocolate);
        }

        public void EndHealthRecovery()
        {
            HealthVendingMachineInteraction.TerminateHealthRecovery();
        }

        public void FreePlayerAfterAnimation()
        {
            playerShoot.FreePlayer();
        }

        public void Hit()
        {
            // Audio management
            if (isHitSoundPending)
            {
                isHitSoundPending = false; // "Consume" the request

                // Play the sound
                PlayManagedEvent(FMODEvents.Instance.PlayerHit);
            }
        }

        public void HitBySpit()
        {
            // Audio management
            if (isHitSoundPending)
            {
                isHitSoundPending = false; // "Consume" the request

                // Play the sounds
                PlayManagedEvent(FMODEvents.Instance.PlayerHitBySpit);
                PlayManagedEvent(FMODEvents.Instance.PlayerHit);
            }
        }

        public void HitByBite()
        {
            // Audio management
            if (isHitSoundPending)
            {
                isHitSoundPending = false; // "Consume" the request

                // Play the sound
                PlayManagedEvent(FMODEvents.Instance.PlayerHitByBite);
            }
        }

        public void VendingMachineItemPickup()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.PlayerVendingMachineItemPickUp);

            if (MachineType == "health")
                HealthVendingMachineInteraction.PlaceSpecialSnackInHand();
            else
            {
                if (MachineType == "playerPowerUp")
                {
                    PowerUpVendingMachineInteraction.PlaceItemInHand();
                }
            }
        }

        public void WakeUp()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.PlayerWakeUp);

            fadeManagerLoadingScreen.Hide();
        }
        
        private void OnDestroy()
        {
            // Audio management: stops and release looping sounds
            StopAllLoopingSounds();

            // Audio management: stops and releases one-shots
            foreach (var instance in activeOneShotInstances)
            {
                instance.stop(STOP_MODE.IMMEDIATE);
                GamePlayAudioManager.instance.ReleaseInstance(instance);
            }
            activeOneShotInstances.Clear();
        }
        
        private void PlayManagedEvent(FMODUnity.EventReference fmodEvent)
        {   
            // Instances the event
            EventInstance eventInstance = GamePlayAudioManager.instance.CreateInstance(fmodEvent);
            // Set the 3D position before starting the event
            eventInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            // Adds it to the list so it can be checked
            activeOneShotInstances.Add(eventInstance);
            // Starts playing
            eventInstance.start();
            // Removes the instance from the list once it is finished, preventing the list from growing indefinitely
            StartCoroutine(ReleaseInstanceWhenFinished(eventInstance));
        }
        
        private System.Collections.IEnumerator ReleaseInstanceWhenFinished(EventInstance eventInstance)
        {
            // Waits until the event is no longer playing
            PLAYBACK_STATE playbackState;
            do {
                eventInstance.getPlaybackState(out playbackState);
                yield return null; // Waits for next frame
            } while (playbackState != PLAYBACK_STATE.STOPPED);

            // Removes the instance from the active list
            activeOneShotInstances.Remove(eventInstance);
            // Releases FMOD resources
            GamePlayAudioManager.instance.ReleaseInstance(eventInstance);
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
            rickHeartbeat.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            
            if (rotatingSphereSource != null)
            {
                rickSphereRotation.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(rotatingSphereSource.transform));
            }

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
        
        private async Task UnfreezePlayerAfterDelay(int delayMs)
        {
            AnimationManager.Instance.DefenseToIdle();
            await Task.Delay(delayMs);
            playerShoot.UnfreezePlayer();
        }
        
        private IEnumerator ShieldDestructionAfterDelay(float delaySeconds)
        {
            // The delay is split in two parts: for synchronization reasons, the first one goes
            // before the clip audio, while the second after that. 
            
            // First part of the delay
            yield return new WaitForSeconds(delaySeconds - 1.0f);

            // Audio management
            GamePlayAudioManager.instance.PlayManagedOneShot(FMODEvents.Instance.PlayerShieldDeactivation, transform.position);
            
            // Second part of the delay
            yield return new WaitForSeconds(1.0f);
            
            // Destroy the shield
            ShieldDestroy();
        }

        // Audio management
        public void StopAllLoopingSounds()
        {
            // Use STOP_MODE.IMMEDIATE to ensure they stop instantly, without waiting for the fade-out.
            // This is crucial in a reset
            rickLoadCloseAttackWithPowerUp1.stop(STOP_MODE.IMMEDIATE);
            rickLoadDistanceAttackWithPowerUp1.stop(STOP_MODE.IMMEDIATE);
            rickLoadCloseAttackWithPowerUp2.stop(STOP_MODE.IMMEDIATE);
            rickLoadDistanceAttackWithPowerUp2.stop(STOP_MODE.IMMEDIATE);
            if(AnimationManager.Instance.rickState == RickStates.Walk)
                rickWalkFootsteps.stop(STOP_MODE.IMMEDIATE);
            if(AnimationManager.Instance.rickState == RickStates.Run)    
                rickRunFootsteps.stop(STOP_MODE.IMMEDIATE);
            rickIdle.stop(STOP_MODE.IMMEDIATE);
            rickHeartbeat.stop(STOP_MODE.IMMEDIATE);
            rickSphereRotation.stop(STOP_MODE.IMMEDIATE);
        }

        public void SpawnAreaAttack()
        {
            playerShoot.attackAreaVFXPrefab.gameObject.SetActive(false);
            playerShoot.attackAreaInstance = Instantiate(playerShoot.attackAreaVFXPrefab, transform.position, Quaternion.identity);
            playerShoot.attackAreaInstance.SetActive(true);
            playerShoot.attackAreaVFXPrefab.gameObject.SetActive(true);
        }

        public void StartIncreaseSizeAreaAttack()
        {
            if (playerShoot.attackAreaInstance != null)
            {
                AreaAttackController AAC = playerShoot.attackAreaInstance.GetComponent<AreaAttackController>();
                if (AAC != null)
                    AAC.SetDestSize(playerShoot.damageRadius);
            }
        }

        public void DestroyAreaAttack()
        {
            if (playerShoot.attackAreaInstance != null)
            {
                Destroy(playerShoot.attackAreaInstance);
            }
            
            playerShoot.ResetCloseAttackValues();
        }
        
        // Audio management
        public void RequestHitSound(PlayerShoot.DamageTypes damageType)
        {
            // If there is already a request in progress, don't start another one to avoid chaos
            if (isHitSoundPending) return;

            isHitSoundPending = true;
            // Imported the fallback chamber which, also the internal compartment of RickEvents
            PlayHitSoundFallback();
        }
        
        // Audio management
        private void PlayHitSoundFallback()
        {
            // If the request is still pending after the wait, we will handle it
            if (isHitSoundPending)
            {
                isHitSoundPending = false; // "Consume" the request

                // Play the hit sound
                PlayManagedEvent(FMODEvents.Instance.PlayerHit);
            }
        }
        
        // Audio management
        public void SetHeartbeatStatus(bool shouldPlay)
        {
            shouldPlayHeartbeat = shouldPlay;
        }
        
        // Audio management
        public void StartSphereRotationSound()
        {
            HandleLoopingSound(rickSphereRotation, true);
        }

        // Audio management
        public void StopSphereRotationSound()
        {
            HandleLoopingSound(rickSphereRotation, false);
        }
        
        /// <summary>
        /// Pauses all Rick's sounds
        /// </summary>
        public void PauseAllRickSounds()
        {
            // Stop looping sounds
            rickLoadCloseAttackWithPowerUp1.setPaused(true);
            rickLoadDistanceAttackWithPowerUp1.setPaused(true);
            rickLoadCloseAttackWithPowerUp2.setPaused(true);
            rickLoadDistanceAttackWithPowerUp2.setPaused(true);
            rickWalkFootsteps.setPaused(true);
            rickRunFootsteps.setPaused(true);
            rickIdle.setPaused(true);
            rickHeartbeat.setPaused(true);
            rickSphereRotation.setPaused(true);

            // Stop one-shot sounds
            foreach (var instance in activeOneShotInstances)
            {
                instance.setPaused(true);
            }
        }
        
        /// <summary>
        /// Resumes all Rick's sounds
        /// </summary>
        public void ResumeAllRickSounds()
        {
            // Resume looping sounds
            rickLoadCloseAttackWithPowerUp1.setPaused(false);
            rickLoadDistanceAttackWithPowerUp1.setPaused(false);
            rickLoadCloseAttackWithPowerUp2.setPaused(false);
            rickLoadDistanceAttackWithPowerUp2.setPaused(false);
            rickWalkFootsteps.setPaused(false);
            rickRunFootsteps.setPaused(false);
            rickIdle.setPaused(false);
            rickHeartbeat.setPaused(false);
            rickSphereRotation.setPaused(false);
            
            // Resume one-shot sounds
            foreach (var instance in activeOneShotInstances)
            {
                instance.setPaused(false);
            }
        }
    }
}