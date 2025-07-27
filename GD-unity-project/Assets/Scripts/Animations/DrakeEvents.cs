using Audio;
using FMOD.Studio;
using UnityEngine;
using System.Collections.Generic;

namespace Animations
{
    public class DrakeEvents : MonoBehaviour
    {
        // Audio management: looping sounds
        private EventInstance drakeFootsteps;
        private EventInstance drakeIdle;
        
        // Audio management: one-shot sounds
        private List<EventInstance> activeOneShotInstances = new List<EventInstance>();

        private DrakeAnimation drakeAnim;
        private Drake drake;
        
        private void Start()
        {
            drakeAnim = drake.anim;
        }
        
        private void Awake()
        {
            drake = GetComponent<Drake>();
            
            // Audio manager: record this script to the central manager
            DrakeAudioManager.Instance.Register(this);

            // Audio management
            drakeFootsteps = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.DrakeFootsteps);
            drakeFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            drakeIdle = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.DrakeIdle);
            drakeIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        }
        
        void FixedUpdate()
        {
            // Audio management: update Drake's position as he's a sound source
            UpdateAll3DAttributes();
        }
        
        private void OnDestroy()
        {
            // Audio management: de-register this script from the manager
            if (DrakeAudioManager.Instance != null)
            {
                DrakeAudioManager.Instance.Unregister(this);
            }
            
            // Audio management: stop events immediately to prevent the sound from continuing after destruction
            // and releases the resources used by the instances
            if (GamePlayAudioManager.instance != null)
            {
                // Release all active looping sound instances
                GamePlayAudioManager.instance.ReleaseInstance(drakeFootsteps);
                GamePlayAudioManager.instance.ReleaseInstance(drakeIdle);
                
                // Releases all active one-shot instances
                foreach (var instance in activeOneShotInstances)
                {
                    GamePlayAudioManager.instance.ReleaseInstance(instance);
                }
                activeOneShotInstances.Clear();
            }
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
            do
            {
                eventInstance.getPlaybackState(out playbackState);
                yield return null; // Waits for next frame
            } 
            while (playbackState != PLAYBACK_STATE.STOPPED);

            // Removes the instance from the active list
            activeOneShotInstances.Remove(eventInstance);
            // Releases FMOD resources
            GamePlayAudioManager.instance.ReleaseInstance(eventInstance);
        }
        
        // Method to update the 3D position of all sounds
        private void UpdateAll3DAttributes()
        {
            var attributes = FMODUnity.RuntimeUtils.To3DAttributes(transform);
            drakeFootsteps.set3DAttributes(attributes);
            drakeIdle.set3DAttributes(attributes);
            
            // It also updates one-shot instances
            foreach (var instance in activeOneShotInstances)
            {
                instance.set3DAttributes(attributes);
            }
        }

        public void Bite()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.DrakeCloseAttack1);

            drake.CheckBiteAttackDamage();
        }

        public void EndBite()
        {
            drakeAnim.EndBit = true;
        }

        public void Swiping()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.DrakeCloseAttack2);
        }
        
        public void EndSwiping()
        {
            drakeAnim.EndSwiping = true;
        }
        
        public void SwipingAttackHit()
        {
            drake.CheckSwipingAttackDamage();
        }

        public void Defense()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.DrakeDefense);
        }

        public void ReactLargeFromRight()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.DrakeHitFromLeftOrRight);
        }

        public void ReactLargeFromLeft()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.DrakeHitFromLeftOrRight);
        }

        public void ReactLargeFromFront()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.DrakeHitFromFrontOrBack);
        }

        public void ReactLargeFromBack()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.DrakeHitFromFrontOrBack);
        }

        public void DeathHit()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.DrakeDieHit);
        }

        public void DeathFootstep1()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.DrakeDieFoostep1);
        }

        public void DeathFootstep2()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.DrakeDieFoostep2);
        }

        public void DeathDrake()
        {
            drake.DestroyEnemy();
        }

        public void DeathThud()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.DrakeDieThud);
        }
        
        // Audio management: all the following functions are used to handle Drake's sounds
        public void StartRunningSound()
        {
            drakeFootsteps.start();
        }
        
        public void StopRunningSound()
        {
            drakeFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
        }
        
        public void StartIdleSound()
        {
            drakeIdle.start();
        }
        
        public void StopIdleSound()
        {
            drakeIdle.stop(STOP_MODE.ALLOWFADEOUT);
        }
        
        /// <summary>
        /// Pauses all sounds of this drake instance
        /// </summary>
        public void PauseAllSounds()
        {
            // Stop looping sounds
            if(drakeFootsteps.isValid())
                drakeFootsteps.setPaused(true);
            if(drakeIdle.isValid())
                drakeIdle.setPaused(true);
            
            // Stop one-shot sounds
            foreach (var instance in activeOneShotInstances)
            {
                if(instance.isValid()) instance.setPaused(true);
            }
        }

        /// <summary>
        /// Resumes all sounds of this drake instance
        /// </summary>
        public void ResumeAllSounds()
        {
            // Resume looping sounds
            if(drakeFootsteps.isValid())
                drakeFootsteps.setPaused(false);
            if(drakeIdle.isValid())
                drakeIdle.setPaused(false);
            
            // Resume one-shot sounds
            foreach (var instance in activeOneShotInstances)
            {
                if(instance.isValid()) instance.setPaused(false);
            }
        }
    }
}