using Audio;
using FMOD.Studio;
using UnityEngine;
using System.Collections.Generic;

namespace Animations
{
    public class MaynardEvents : MonoBehaviour
    {
        // Audio management: looping sounds
        private EventInstance maynardFootsteps;
        private EventInstance maynardIdle;
        
        // Audio management: one shot sounds
        private List<EventInstance> activeOneShotInstances = new List<EventInstance>();
        
        private MaynardAnimation maynardAnim;
        private Maynard maynard;

        private void Start()
        {
            maynardAnim = maynard.anim;
        }
        
        private void Awake()
        {
            maynard = GetComponent<Maynard>();
            
            // Audio management: record this script to the central manager
            MaynardAudioManager.Instance.Register(this);
            
            // Audio management
            maynardFootsteps = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.MaynardFootsteps);
            maynardFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            maynardIdle = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.MaynardIdle);
            maynardIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        }
        
        private void FixedUpdate()
        {
            // Audio management: update Maynard's position as he's a sound source
            maynardFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            maynardIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        }
        
        private void OnDestroy()
        {
            // Audio management: de-register this script from the manager to avoid errors
            if (MaynardAudioManager.Instance != null)
            {
                MaynardAudioManager.Instance.Unregister(this);
            }
            
            // Audio management: stop events immediately to prevent the sound from continuing after destruction
            // and releases the resources used by the instances
            if (GamePlayAudioManager.instance != null)
            {
                // Release all active looping sound instances
                GamePlayAudioManager.instance.ReleaseInstance(maynardFootsteps);
                GamePlayAudioManager.instance.ReleaseInstance(maynardIdle);
                
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
            
            // Set the 3D position BEFORE starting the event
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
            maynardFootsteps.set3DAttributes(attributes);
            maynardIdle.set3DAttributes(attributes);
            
            // It also updates one-shot instances
            foreach (var instance in activeOneShotInstances)
            {
                instance.set3DAttributes(attributes);
            }
        }

        public void Scream()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.MaynardDistanceAttack1);

            maynard.EmitScream();
        }
        
        public void EndScream()
        {
            maynardAnim.EndScream = true;
        }

        public void MutantRoaring()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.MaynardDistanceAttack2);
        }

        public void Attack()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.MaynardCloseAttack);
        }

        public void CloseAttackHit()
        {
            maynard.CheckCloseAttackDamage();
        }
        
        public void EndCloseAttack()
        {
            maynardAnim.EndCloseAttack=true;
        }

        public void FallScream()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.MaynardHitFallScream);
        }

        public void FallThud()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.MaynardHitFallThud);
        }

        public void StandUpRoar()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.MaynardStandUpRoar);
        }

        public void StandUpFootstep1()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.MaynardStandUpFootstep1);
        }

        public void StandUpFootstep2()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.MaynardStandUpFootstep2);
        }

        public void StandUpBreath()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.MaynardStandUpBreath);
        }

        public void ReactLargeFromRight()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.MaynardHitFromLeftOrRight);
        }

        public void ReactLargeFromLeft()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.MaynardHitFromLeftOrRight);
        }

        public void ReactLargeFromFront()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.MaynardHitFromFront);
        }

        public void ReactLargeFromBack()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.MaynardHitFromBack);
        }

        public void DeathScream()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.MaynardDieScream);
        }

        public void DeathThud()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.MaynardDieThud);
        }
        public void DeathMaynard()
        {
            maynard.DestroyEnemy();
        }
        
        // Audio management: all the following functions are used to handle Maynard's sounds
        public void StartRunningSound()
        {
            maynardFootsteps.start();
        }
        
        public void StopRunningSound()
        {
            maynardFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
        }
        
        public void StartIdleSound()
        {
            maynardIdle.start();
        }
        
        public void StopIdleSound()
        {
            maynardIdle.stop(STOP_MODE.ALLOWFADEOUT);
        }
        
        /// <summary>
        /// Pauses all sounds of this Maynard instance
        /// </summary>
        public void PauseAllSounds()
        {
            // Stop looping sounds
            if(maynardFootsteps.isValid())
                maynardFootsteps.setPaused(true);
            if(maynardIdle.isValid())
                maynardIdle.setPaused(true);
            
            // Stop one-shot sounds
            foreach (var instance in activeOneShotInstances)
            {
                if(instance.isValid()) instance.setPaused(true);
            }
        }

        /// <summary>
        /// Resumes all sounds of this Maynard instance
        /// </summary>
        public void ResumeAllSounds()
        {
            // Resume looping sounds
            if(maynardFootsteps.isValid())
                maynardFootsteps.setPaused(false);
            if(maynardIdle.isValid())
                maynardIdle.setPaused(false);
            
            // Resume one-shot sounds
            foreach (var instance in activeOneShotInstances)
            {
                if(instance.isValid()) instance.setPaused(false);
            }
        }
    }
}