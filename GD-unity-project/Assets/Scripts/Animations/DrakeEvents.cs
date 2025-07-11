using Audio;
using FMOD.Studio;
using UnityEngine;

namespace Animations
{
    public class DrakeEvents : MonoBehaviour
    {
        // Audio management
        private EventInstance drakeFootsteps;
        private EventInstance drakeIdle;
        private DrakeAnimation drakeAnim;
        private Drake drake;

        private bool isRunning = false; // TODO: to be removed once we have Drake's FSM
        private bool isIdle = false; // TODO: to be removed once we have Drake's FSM


        public void Awake()
        {
            drake = GetComponent<Drake>();

            if (drake == null)
            {
                Debug.LogError($"{ToString()}: Drake not found");
            }
        }

        // Audio management
        private void Start()
        {
            drakeAnim = drake.anim;

            drakeFootsteps = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.DrakeFootsteps);
            drakeFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));

            drakeIdle = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.DrakeIdle);
            drakeIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        }
        public void Idle()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            isIdle = true; // TODO: to be removed once we have Drake's FSM
        }

        public void Run()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            isRunning = true; // TODO: to be removed once we have Drake's FSM
        }

        public void Bite()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.DrakeCloseAttack1, transform.position);

            drake.CheckBiteAttackDamage();
        }

        public void Swiping()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.DrakeCloseAttack2, transform.position);
        }

        public void SwipingAttackHit()
        {
            drake.CheckSwipingAttackDamage();
        }

        public void Defense()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.DrakeDefense, transform.position);
        }

        public void ReactLargeFromRight()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.DrakeHitFromLeftOrRight, transform.position);
        }

        public void ReactLargeFromLeft()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.DrakeHitFromLeftOrRight, transform.position);
        }

        public void ReactLargeFromFront()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.DrakeHitFromFrontOrBack, transform.position);
        }

        public void ReactLargeFromBack()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.DrakeHitFromFrontOrBack, transform.position);
        }

        public void DeathHit()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.DrakeDieHit, transform.position);
        }

        public void DeathFootstep1()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.DrakeDieFoostep1, transform.position);
        }

        public void DeathFootstep2()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.DrakeDieFoostep2, transform.position);
        }

        public void DeathDrake()
        {
            drake.DestroyEnemy();
        }

        public void DeathThud()
        {
            // Audio management
            ResetAudioState(); // TODO: to be removed once we have Drake's FSM
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.DrakeDieThud, transform.position);
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
            if (isRunning) // TODO: check Drake's state (something like <<DrakeState != Run>>)
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
            if (isIdle) // TODO: check Drake's state (something like <<DrakeState != Idle>>)
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

        public void EndSwiping()
        {
            drakeAnim.EndSwiping = true;
        }

        public void EndBite()
        {
            drakeAnim.EndBit = true;
        }
    }
    
    
}


