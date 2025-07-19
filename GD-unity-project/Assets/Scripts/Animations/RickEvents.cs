using System;
using System.Threading.Tasks;
using Audio;
using FMOD.Studio;
using PlayerInteraction;
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

        // Defense
	    [SerializeField] private GameObject magneticShieldPrefab;
        private GameObject shield;

        // This flag must be set to 'true' by the input script when the attack key is pressed,
        // and to 'false' when released
        public bool ShouldPlayChargeSound { get; set; } = false;

        public PowerUp powerUp;
        public PlayerShoot playerShoot;
        [SerializeField] private FadeManagerLoadingScreen fadeManagerLoadingScreen;
        [NonSerialized] public HealthVendingMachineInteraction healthVendingMachineInteraction;
		[NonSerialized] public PowerUpVendingMachineInteraction powerUpVendingMachineInteraction;
        [NonSerialized] public string machineType; // Can be "playerPowerUp" or "health"

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
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerCloseAttackShoot, transform.position);
            
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
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDistanceAttackShoot, transform.position);

            playerShoot.FireDistanceAttack();
        }

        public void DefenseVFX()
        {
            shield = Instantiate(magneticShieldPrefab, transform.position, Quaternion.identity, transform);
            shield.tag = "Shield";
            shield.gameObject.SetActive(true);
            shield.gameObject.SetActive(true);

            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerShieldActivation, transform.position);

            if (powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.DefensePowerUp))
            {
                if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DefensePowerUp] == 1)
                {
                    shield.GetComponent<ShieldTrigger>().SetLifeTime(3f);
                }
                else if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DefensePowerUp] == 2)
                {
                    shield.GetComponent<ShieldTrigger>().SetLifeTime(3.8f);
                }
            }
            else
            {
                shield.GetComponent<ShieldTrigger>().SetLifeTime(2f);
            }
            
        }

        public void ShieldActivation()
        {
            // Activate the shield
            DefenseVFX();
            
            // Let the player free to move during the usage of the shield
            _ = UnfreezePlayerAfterDelay(500);
            
            // Deactivate the shield
            ShieldDeactivation();
        }

        private void ShieldDeactivation()
        {
            // Audio management
            if (!powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.DefensePowerUp))
            {
                _ = ShieldDeactivationAfterDelay(2500);
            }
            else
            {
                if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DefensePowerUp] == 1)
                {
                    _ = ShieldDeactivationAfterDelay(3500);
                }

                else if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DefensePowerUp] == 2)
                {
                    _ = ShieldDeactivationAfterDelay(4500);
                }
            }
        }

        public void ShieldDestroy()
        {
            if (shield != null)
            {
                Destroy(shield);
            }
        }

        public void DeathForwardGrunt()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDieForwardGrunt, transform.position);

            playerShoot.SetLayerToZero();
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

            playerShoot.SetLayerToZero();
        }

        public void DeathBackwardThud()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDieBackwardThud, transform.position);
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
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDrink, transform.position);
        }

        public void EatChips()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerEatChips, transform.position);
        }

        public void EndPowerUp()
        {
            powerUpVendingMachineInteraction.TerminatePlayerPowerUp();
        }

        public void EatChocolate()
        {
            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerEatChocolate, transform.position);
        }

        public void EndHealthRecovery()
        {
            healthVendingMachineInteraction.TerminateHealthRecovery();
        }

        public void FreePlayerAfterAnimation()
        {
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

            if (machineType == "health")
                healthVendingMachineInteraction.PlaceSpecialSnackInHand();
            else
            {
                if (machineType == "playerPowerUp")
                {
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
        
        private async Task UnfreezePlayerAfterDelay(int delayMs)
        {
            await Task.Delay(delayMs);
            playerShoot.UnfreezePlayer();
            AnimationManager.Instance.DefenseToIdle();
        }
        
        private async Task ShieldDeactivationAfterDelay(int delayMs)
        {
            // The delay is split in two parts: for synchronization reasons, the first one goes
            // before the clip audio, while the second after that. 
            
            // First part of the delay
            await Task.Delay(delayMs - 1000);

            // Audio management
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerShieldDeactivation, transform.position);
            
            // Second part of the delay
            await Task.Delay(1000);
            
            // Deactivation and destruction of the shield
            playerShoot.CloseShield();
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
            rickWalkFootsteps.stop(STOP_MODE.IMMEDIATE);
            rickRunFootsteps.stop(STOP_MODE.IMMEDIATE);
            rickIdle.stop(STOP_MODE.IMMEDIATE);
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
                else Debug.Log("AAC=null");
            }
        }

        public void DestroyAreaAttack()
        {
            Debug.Log("Destroy AreaAttack");
            if (playerShoot.attackAreaInstance != null)
            {
                Destroy(playerShoot.attackAreaInstance);
            }
            
            playerShoot.ResetCloseAttackValues();
        }
    }
    
    
}