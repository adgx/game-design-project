using Audio;
using FMOD.Studio;
using UnityEngine;

namespace Animations
{
    public class MaynardEvents : MonoBehaviour
    {
        // Audio management
        private EventInstance maynardFootsteps;
        private EventInstance maynardIdle;
        private Maynard maynard;
        private bool isRunning = false; // TODO: to be removed once we have Maynard's FSM
        private bool isIdle = false; // TODO: to be removed once we have Maynard's FSM
    
        public void Idle()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            isIdle = true; // TODO: to be removed once we have Maynard's FSM
        }
        
        public void Run()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            isRunning = true; // TODO: to be removed once we have Maynard's FSM

        }
        
        public void Scream()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardDistanceAttack1, transform.position);
        }
    
        public void MutantRoaring()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardDistanceAttack2, transform.position);
        }
    
        public void Attack()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardCloseAttack, transform.position);
        }

        public void CloseAttackHit()
        {
            maynard.CheckCloseAttackDamage();
        }
    
        public void FallScream()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardHitFallScream, transform.position);
        }
        
        public void FallThud()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardHitFallThud, transform.position);
        }
    
        public void StandUpRoar()
        {
            // Audio management;
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardStandUpRoar, transform.position);
        }
    
        public void StandUpFootstep1()
        {
            // Audio management;
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardStandUpFootstep1, transform.position);
        }
    
        public void StandUpFootstep2()
        {
            // Audio management;
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardStandUpFootstep2, transform.position);
        }
    
        public void StandUpBreath()
        {
            // Audio management;
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardStandUpBreath, transform.position);
        }
    
        public void ReactLargeFromRight()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardHitFromLeftOrRight, transform.position);
        }
    
        public void ReactLargeFromLeft()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardHitFromLeftOrRight, transform.position);
        }
    
        public void ReactLargeFromFront()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardHitFromFront, transform.position);
        }
    
        public void ReactLargeFromBack()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardHitFromBack, transform.position);
        }

        public void DeathScream()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardDieScream, transform.position);
        }
        
        public void DeathThud()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Maynard's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MaynardDieThud, transform.position);
        }
        public void DeathMaynard()
        {
            maynard.DestroyEnemy();
        }
    
        // Audio management
        private void Start()
        {
            maynardFootsteps = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.MaynardFootsteps);
            maynardFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));

            maynardIdle = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.MaynardIdle);
            maynardIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));

            maynard = GetComponent<Maynard>();
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

