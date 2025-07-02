using System.Threading.Tasks;
using Audio;
using FMOD.Studio;
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
        
        private bool isLoadingCloseAttack = false; // TODO: to be removed once we have Rick's FSM
        private bool isLoadingDistanceAttack = false; // TODO: to be removed once we have Rick's FSM
        private bool isWalking = false; // TODO: to be removed once we have Rick's FSM
        private bool isRunning = false; // TODO: to be removed once we have Rick's FSM
        private bool isIdle = false; // TODO: to be removed once we have Rick's FSM
        
        public PowerUp powerUp;
    
        public void Idle()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Rick's FSM
            isIdle = true; // TODO: to be removed once we have Rick's FSM
        }
        
        public void Walk()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Rick's FSM
            isWalking = true; // TODO: to be removed once we have Rick's FSM
        }
        
        public void Run()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Rick's FSM
            isRunning = true; // TODO: to be removed once we have Rick's FSM
        }
        
        public void CloseAttackLoad()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Rick's FSM
            isLoadingCloseAttack = true;
        }
        
        public void CloseAttackShoot()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Rick's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerCloseAttackShoot, transform.position);
        }
        
        public void DistanceAttackLoad()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Rick's FSM
            isLoadingDistanceAttack = true;
        }

        public void DistanceAttackShoot()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Rick's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDistanceAttackShoot, transform.position);
        }
        
        public void ShieldActivation()
        {
            // AnimationManager.Instance.DefenseVFX(transform.position);
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Rick's FSM
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
            ResetAudioState(); // TODO: to be removed once we have Rick's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDieForwardGrunt, transform.position);
        }
        
        public void DeathForwardThud1()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Rick's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDieForwardThud1, transform.position);
        }
        
        public void DeathForwardThud2()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Rick's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDieForwardThud2, transform.position);
        }
        
        public void DeathBackwardGrunt()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Rick's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDieBackwardGrunt, transform.position);
        }
        
        public void DeathBackwardThud()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Rick's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDieBackwardThud, transform.position);
        }
        
        public void Drink()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Rick's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerDrink, transform.position);
        }
        
        public void EatChips()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Rick's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerEatChips, transform.position);
        }
    
        public void EatChocolate()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Rick's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerEatChocolate, transform.position);
        }
        
        public void Hit()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Rick's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerHit, transform.position);
        }
        
        public void HitBySpit()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Rick's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerHitBySpit, transform.position);
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerHit, transform.position);
        }
        
        public void HitByBite()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Rick's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerHitByBite, transform.position);
        }
        
        public void VendingMachineItemPickup()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Rick's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerVendingMachineItemPickUp, transform.position);
        }
        
        public void WakeUp()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Rick's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerWakeUp, transform.position);
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
            
            // TODO: debug code used to test the power up mechanism related to loaded attacks 
            // powerUp.ObtainPowerUp(PowerUp.SpherePowerUpTypes.DefensePowerUp);
            // powerUp.ObtainPowerUp(PowerUp.SpherePowerUpTypes.DefensePowerUp);
            // powerUp.ObtainPowerUp(PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp);
            // powerUp.ObtainPowerUp(PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp);
            // powerUp.ObtainPowerUp(PowerUp.SpherePowerUpTypes.CloseAttackPowerUp);
            // powerUp.ObtainPowerUp(PowerUp.SpherePowerUpTypes.CloseAttackPowerUp);
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
            
            if (powerUp.powerUpsObtained.ContainsKey(PowerUp.SpherePowerUpTypes.CloseAttackPowerUp))
            {
                // Start loadCloseAttack event if Rick is loading the close attack 
                if (isLoadingCloseAttack) // TODO: check Rick's state (something like <<RickState != LoadingCloseAttack>>)
                {
                    // Get the playback state for the loadCloseAttack event
                    PLAYBACK_STATE loadCloseAttackPlaybackState;

                    if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.CloseAttackPowerUp] == 1)
                    {
                        rickLoadCloseAttackWithPowerUp1.getPlaybackState(out loadCloseAttackPlaybackState);
                        if (loadCloseAttackPlaybackState.Equals(PLAYBACK_STATE.STOPPED))
                        {
                            rickLoadCloseAttackWithPowerUp1.start();
                            _ = StopLoadingSoundAfterDelay(rickLoadCloseAttackWithPowerUp1, 1500);
                        }
                    }
                    else if(powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.CloseAttackPowerUp] == 2)
                    {
                        rickLoadCloseAttackWithPowerUp2.getPlaybackState(out loadCloseAttackPlaybackState);
                        if (loadCloseAttackPlaybackState.Equals(PLAYBACK_STATE.STOPPED))
                        {
                            rickLoadCloseAttackWithPowerUp2.start();
                            _ = StopLoadingSoundAfterDelay(rickLoadCloseAttackWithPowerUp2, 2500);
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
                if (isLoadingDistanceAttack) // TODO: check Rick's state (something like <<RickState != LoadingDistanceAttack>>)
                {
                    // Get the playback state for the loadDistanceAttack event
                    PLAYBACK_STATE loadDistanceAttackPlaybackState;
                   
                    if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp] == 1)
                    {
                        rickLoadDistanceAttackWithPowerUp1.getPlaybackState(out loadDistanceAttackPlaybackState);
                        if (loadDistanceAttackPlaybackState.Equals(PLAYBACK_STATE.STOPPED))
                        {
                            rickLoadDistanceAttackWithPowerUp1.start();
                            _ = StopLoadingSoundAfterDelay(rickLoadDistanceAttackWithPowerUp1, 1500);
                        }
                    }
                    else if (powerUp.powerUpsObtained[PowerUp.SpherePowerUpTypes.DistanceAttackPowerUp] == 2)
                    {
                        rickLoadDistanceAttackWithPowerUp2.getPlaybackState(out loadDistanceAttackPlaybackState);
                        if (loadDistanceAttackPlaybackState.Equals(PLAYBACK_STATE.STOPPED))
                        {
                            rickLoadDistanceAttackWithPowerUp2.start();
                            _ = StopLoadingSoundAfterDelay(rickLoadDistanceAttackWithPowerUp2, 2500);
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
            if (isWalking) // TODO: check Rick's state (something like <<RickState != Walk>>)
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
            if (isRunning) // TODO: check Rick's state (something like <<RickState != Run>>)
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
            if (isIdle) // TODO: check Rick's state (something like <<RickState != Idle>>)
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
        }
    
        // TODO: to be removed once we have Rick's FSM
        private void ResetAudioState()
        {
            isLoadingCloseAttack = false;
            isLoadingDistanceAttack = false;
            isWalking = false;
            isRunning = false;
            isIdle = false;
        }
        
        // Audio management
        private async Task StopLoadingSoundAfterDelay(EventInstance instance, int delayMs)
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
            
            ResetAudioState();
        }
        
        private async Task ShieldDeactivationAfterDelay(int delayMs)
        {
            await Task.Delay(delayMs);
            
            Debug.Log("ShieldDeactivation");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Rick's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PlayerShieldDeactivation, transform.position);
        }
    }
}

