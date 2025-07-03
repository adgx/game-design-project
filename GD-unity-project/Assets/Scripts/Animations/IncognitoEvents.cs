using Audio;
using FMOD.Studio;
using UnityEngine;

namespace Animations
{
    public class IncognitoEvents : MonoBehaviour
    {
        // Audio management
        private EventInstance incognitoFootsteps;
        private EventInstance incognitoIdle;
        private Incognito incognito; 
        private bool isRunning = false; // TODO: to be removed once we have Incognito's FSM
        private bool isIdle = false; // TODO: to be removed once we have Incognito's FSM

        public void Idle()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
            isIdle = true; // TODO: to be removed once we have Incognito's FSM
        }

        public void Run()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
            isRunning = true; // TODO: to be removed once we have Incognito's FSM
        }

        public void ShortDistanceSpit()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoDistanceAttack1, transform.position);
        }

        public void LongDistanceSpitLoad()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoDistanceAttack2Load, transform.position);
        }
        
        public void LongDistanceSpitShoot()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoDistanceAttack2Spit, transform.position);
        }
        
        public void FallScream()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoHitFallScream, transform.position);
        }
        
        public void FallFootstep1()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoHitFallFootstep1, transform.position);
        }
        
        public void FallFootstep2()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoHitFallFootstep2, transform.position);
        }
        
        public void FallThud()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoHitFallThud, transform.position);
        }
    
        public void StandUpFootstep1()
        {
            // Audio management;
            ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoStandUpFootstep1, transform.position);
        }
    
        public void StandUpFootstep2()
        {
            // Audio management;
            ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoStandUpFootstep2, transform.position);
        }

        public void ReactLargeFromRight()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoHitFromLeftOrRight, transform.position);
        }

        public void ReactLargeFromLeft()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoHitFromLeftOrRight, transform.position);
        }

        public void ReactLargeGut()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoHitFromFront2, transform.position);
        }

        public void ReactLargeFromFront()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoHitFromFront1, transform.position);
        }

        public void DeathGrunt()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoDieGrunt, transform.position);
        }

        public void DeathThud1()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoDieThud1, transform.position);
        }

        public void DeathThud2()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Incognito's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.IncognitoDieThud2, transform.position);
        }

        public void DeathIncognito()
        {
            incognito.DestroyEnemy();
        }

        // Audio management
        private void Start()
        {
            incognito = GetComponent<Incognito>();
            
            incognitoFootsteps = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.IncognitoFootsteps);
            incognitoFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));

            incognitoIdle = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.IncognitoIdle);
            incognitoIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        }

        // FixedUpdate is called once per frame
        void FixedUpdate()
        {
            // Audio management
            UpdateSound();
        }

        private void UpdateSound()
        {
            incognitoFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            incognitoIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));

            // Start footsteps event if incognito is moving
            if (isRunning) // TODO: check Incognito's state (something like <<IncognitoState != Run>>)
            {
                // Get the playback state for the footsteps event
                PLAYBACK_STATE footstepsPlaybackState;
                incognitoFootsteps.getPlaybackState(out footstepsPlaybackState);
                if (footstepsPlaybackState.Equals(PLAYBACK_STATE.STOPPED))
                {
                    incognitoFootsteps.start();
                }
            }
            // Otherwise, stop the footsteps event
            else
            {
                incognitoFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
            }

            // Start idle event if incognito is using the idle animation
            if (isIdle) // TODO: check Incognito's state (something like <<IncognitoState != Idle>>)
            {
                // Get the playback state for the idle event
                PLAYBACK_STATE idlePlaybackState;
                incognitoIdle.getPlaybackState(out idlePlaybackState);
                if (idlePlaybackState.Equals(PLAYBACK_STATE.STOPPED))
                {
                    incognitoIdle.start();
                }
            }
            // Otherwise, stop the idle event
            else
            {
                incognitoIdle.stop(STOP_MODE.ALLOWFADEOUT);
            }

        }

        // TODO: to be removed once we have Incognito's FSM
        private void ResetAudioState()
        {
            isRunning = false;
            isIdle = false;
        }
    }
}
