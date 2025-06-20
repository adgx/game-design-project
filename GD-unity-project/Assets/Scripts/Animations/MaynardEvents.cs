using FMOD.Studio;
using UnityEngine;

namespace Animations
{
    public class MaynardEvents : MonoBehaviour
    {
        // Audio management
        private EventInstance maynardFootsteps;
        private EventInstance maynardIdle;
    
        private bool isRunning = false; // TODO: to be removed once we have Maynard's FSM
        private bool isIdle = false; // TODO: to be removed once we have Maynard's FSM
    
        public void Idle()
        {
            Debug.Log("Idle");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            isIdle = true; // TODO: to be removed once we have Maynard's FSM
        }
        
        public void Run()
        {
            Debug.Log("Run");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            isRunning = true; // TODO: to be removed once we have Maynard's FSM

        }
        
        public void Scream()
        {
            Debug.Log("Scream");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.MaynardDistanceAttack1, transform.position);
        }
    
        public void MutantRoaring()
        {
            Debug.Log("MutantRoaring");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.MaynardDistanceAttack2, transform.position);
        }
    
        public void Attack()
        {
            Debug.Log("Attack");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.MaynardCloseAttack, transform.position);
        }
    
        public void Fall()
        {
            Debug.Log("Fall");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.MaynardHitFall, transform.position);
        }
    
        public void StandUpRoar()
        {
            Debug.Log("StandUpRoar");
        
            // Audio management;
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.MaynardStandUpRoar, transform.position);
        }
    
        public void StandUpFootstep1()
        {
            Debug.Log("StandUpFootstep1");
        
            // Audio management;
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.MaynardFootstep1, transform.position);
        }
    
        public void StandUpFootstep2()
        {
            Debug.Log("StandUpFootstep2");
        
            // Audio management;
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.MaynardFootstep2, transform.position);
        }
    
        public void StandUpBreath()
        {
            Debug.Log("StandUpBreath");
        
            // Audio management;
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.MaynardStandUpBreath, transform.position);
        }
    
        public void ReactLargeFromRight()
        {
            Debug.Log("ReactLargeFromRight");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.MaynardHitFromLeftOrRight, transform.position);
        }
    
        public void ReactLargeFromLeft()
        {
            Debug.Log("ReactLargeFromLeft");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.MaynardHitFromLeftOrRight, transform.position);
        }
    
        public void ReactLargeFromFront()
        {
            Debug.Log("ReactLargeFromFront");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.MaynardHitFromFront, transform.position);
        }
    
        public void ReactLargeFromBack()
        {
            Debug.Log("ReactLargeFromBack");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.MaynardHitFromBack, transform.position);
        }

        public void Death()
        {
            Debug.Log("Death");
        
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.instance.MaynardDie, transform.position);
        }
    
        // Audio management
        private void Start()
        {
            maynardFootsteps = GamePlayAudioManager.instance.CreateInstance(FMODEvents.instance.MaynardFootsteps);
            maynardFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        
            maynardIdle = GamePlayAudioManager.instance.CreateInstance(FMODEvents.instance.MaynardIdle);
            maynardIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        }
    
        // FixedUpdate is called once per frame
        void FixedUpdate()
        {
            // Audio management
            UpdateSound();
        }
    
        private void UpdateSound()
        {
            maynardFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            maynardIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));

            // Start footsteps event if Maynard is moving
            if (isRunning) // TODO: check Maynard's state (something like <<MaynardState != Run>>)
            {
                // Get the playback state for the footsteps event
                PLAYBACK_STATE footstepsPlaybackState;
                maynardFootsteps.getPlaybackState(out footstepsPlaybackState);
                if (footstepsPlaybackState.Equals(PLAYBACK_STATE.STOPPED))
                {
                    maynardFootsteps.start();
                }
            }
            // Otherwise, stop the footsteps event
            else
            {
                maynardFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
            }
        
            // Start idle event if Maynard is using the idle animation
            if (isIdle) // TODO: check Maynard's state (something like <<MaynardState != Idle>>)
            {
                // Get the playback state for the idle event
                PLAYBACK_STATE idlePlaybackState;
                maynardIdle.getPlaybackState(out idlePlaybackState);
                if (idlePlaybackState.Equals(PLAYBACK_STATE.STOPPED))
                {
                    maynardIdle.start();
                }
            }
            // Otherwise, stop the idle event
            else
            {
                maynardIdle.stop(STOP_MODE.ALLOWFADEOUT);
            }
        
        }
    
        // TODO: to be removed once we have Maynard's FSM
        private void ResetAudioState()
        {
            isRunning = false;
            isIdle = false;
        }
    }
}

