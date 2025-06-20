using FMOD.Studio;
using UnityEngine;

namespace Animations
{
    public class DrakeEvents : MonoBehaviour
    {
        // Audio management
        private EventInstance drakeFootsteps;
        private EventInstance drakeIdle;
    
        private bool isRunning = false; // TODO: to be removed once we have Drake's FSM
        private bool isIdle = false; // TODO: to be removed once we have Drake's FSM
    
        public void Idle()
        {
            Debug.Log("Idle");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            isIdle = true; // TODO: to be removed once we have Drake's FSM
        }
        
        public void Run()
        {
            Debug.Log("Run");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            isRunning = true; // TODO: to be removed once we have Drake's FSM
        }
        
        public void Bite()
        {
            Debug.Log("Bite");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.drakeCloseAttack1, transform.position);
        }
    
        public void Swiping()
        {
            Debug.Log("Swiping");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.drakeCloseAttack2, transform.position);
        }
    
        public void Defense()
        {
            Debug.Log("Defense");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.drakeDefense, transform.position);
        }
    
        public void ReactLargeFromRight()
        {
            Debug.Log("ReactLargeFromRight");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.drakeHitFromLeftOrRight, transform.position);
        }
    
        public void ReactLargeFromLeft()
        {
            Debug.Log("ReactLargeFromLeft");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.drakeHitFromLeftOrRight, transform.position);
        }
    
        public void ReactLargeFromFront()
        {
            Debug.Log("ReactLargeFromFront");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.drakeHitFromFront, transform.position);
        }
    
        public void ReactLargeFromBack()
        {
            Debug.Log("ReactLargeFromBack");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.drakeHitFromBack, transform.position);
        }

        public void DeathHit()
        {
            Debug.Log("DeathHit");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.drakeDieHit, transform.position);
        }
        
        public void DeathFootstep1()
        {
            Debug.Log("DeathFootstep1");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.drakeDieFoostep1, transform.position);
        }
        
        public void DeathFootstep2()
        {
            Debug.Log("DeathFootstep2");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.drakeDieFoostep2, transform.position);
        }
        
        public void DeathThud()
        {
            Debug.Log("DeathThud");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.drakeDieThud, transform.position);
        }
    
        // Audio management
        private void Start()
        {
            drakeFootsteps = GamePlayAudioManager.instance.CreateInstance(FMODEvents.instance.drakeFootsteps);
            drakeFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        
            drakeIdle = GamePlayAudioManager.instance.CreateInstance(FMODEvents.instance.drakeIdle);
            drakeIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        }
    
        // FixedUpdate is called once per frame
        void FixedUpdate()
        {
            // Audio management
            UpdateSound();
        }
    
        private void UpdateSound()
        {
            drakeFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            drakeIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));

            // Start footsteps event if Drake is moving
            if (isRunning) // TODO: check Drake's state (something like <<DrakState != Run>>)
            {
                // Get the playback state for the footsteps event
                PLAYBACK_STATE footstepsPlaybackState;
                drakeFootsteps.getPlaybackState(out footstepsPlaybackState);
                if (footstepsPlaybackState.Equals(PLAYBACK_STATE.STOPPED))
                {
                    drakeFootsteps.start();
                }
            }
            // Otherwise, stop the footsteps event
            else
            {
                drakeFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
            }
        
            // Start idle event if Drake is using the idle animation
            if (isIdle) // TODO: check Drake's state (something like <<DrakState != Idle>>)
            {
                // Get the playback state for the idle event
                PLAYBACK_STATE idlePlaybackState;
                drakeIdle.getPlaybackState(out idlePlaybackState);
                if (idlePlaybackState.Equals(PLAYBACK_STATE.STOPPED))
                {
                    drakeIdle.start();
                }
            }
            // Otherwise, stop the idle event
            else
            {
                drakeIdle.stop(STOP_MODE.ALLOWFADEOUT);
            }
        
        }
    
        // TODO: to be removed once we have Drake's FSM
        private void ResetAudioState()
        {
            isRunning = false;
            isIdle = false;
        }
    }
}


