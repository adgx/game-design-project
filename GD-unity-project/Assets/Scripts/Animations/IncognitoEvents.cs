using Audio;
using FMOD.Studio;
using UnityEngine;
using System.Collections.Generic;

namespace Animations
{
    public class IncognitoEvents : MonoBehaviour
    {
        // Audio management: looping sounds
        private EventInstance incognitoFootsteps;
        private EventInstance incognitoIdle;
        
        // Audio management: one-shot sounds
        private List<EventInstance> activeOneShotInstances = new List<EventInstance>();
        
        private Incognito incognito;
        private IncognitoAnimation incognitoAnim;

        private void Start()
        {
            incognitoAnim = incognito.anim;
        }

        private void Awake()
        {
            incognito = GetComponent<Incognito>();

            // Audio management: record this script to the central manager
            IncognitoAudioManager.Instance.Register(this);
            
            // Audio management
            incognitoFootsteps = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.IncognitoFootsteps);
            incognitoFootsteps.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            incognitoIdle = GamePlayAudioManager.instance.CreateInstance(FMODEvents.Instance.IncognitoIdle);
            incognitoIdle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        }
        
        private void FixedUpdate()
        {
            // Audio management: update Incognito's position as he's a sound source
            UpdateAll3DAttributes();
        }
        
        private void OnDestroy()
        {
            // Audio management: de-register this script from the manager
            if (IncognitoAudioManager.Instance != null)
            {
                IncognitoAudioManager.Instance.Unregister(this);
            }
            
            // Audio management: stop events immediately to prevent the sound from continuing after destruction
            // and releases the resources used by the instances
            if (GamePlayAudioManager.instance != null)
            {
                // Release all active looping sound instances
                GamePlayAudioManager.instance.ReleaseInstance(incognitoFootsteps);
                GamePlayAudioManager.instance.ReleaseInstance(incognitoIdle);
                
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
            incognitoFootsteps.set3DAttributes(attributes);
            incognitoIdle.set3DAttributes(attributes);
            
            // It also updates one-shot instances
            foreach (var instance in activeOneShotInstances)
            {
                instance.set3DAttributes(attributes);
            }
        }
        
        // This function is called when Incognito should emit its spit
        public void Spitting()
        {
            incognito.EmitSpit(shortSpit: true);
        }
        
        public void ShortDistanceSpit()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.IncognitoDistanceAttack1);
        }
        
        public void EndShortSpit()
        {
            incognitoAnim.EndShortSpit = true;
        }

        public void LongDistanceSpitLoad()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.IncognitoDistanceAttack2Load);
        }

        public void LongDistanceSpitShoot()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.IncognitoDistanceAttack2Spit);

			incognito.EmitSpit(shortSpit: false);
		}
        
        public void EndLongSpit()
        {
            incognitoAnim.EndLongSpit = true;
        }
        
        public void FallScream()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.IncognitoHitFallScream);
        }

        public void FallFootstep1()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.IncognitoHitFallFootstep1);
        }

        public void FallFootstep2()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.IncognitoHitFallFootstep2);
        }

        public void FallThud()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.IncognitoHitFallThud);
        }

        public void StandUpFootstep1()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.IncognitoStandUpFootstep1);
        }

        public void StandUpFootstep2()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.IncognitoStandUpFootstep2);
        }

        public void ReactLargeFromRight()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.IncognitoHitFromLeftOrRight);
        }

        public void ReactLargeFromLeft()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.IncognitoHitFromLeftOrRight);
        }

        public void ReactLargeGut()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.IncognitoHitFromFront2);
        }

        public void ReactLargeFromFront()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.IncognitoHitFromFront1);
        }

        public void DeathGrunt()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.IncognitoDieGrunt);
        }

        public void DeathThud1()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.IncognitoDieThud1);
        }

        public void DeathThud2()
        {
            // Audio management
            PlayManagedEvent(FMODEvents.Instance.IncognitoDieThud2);
        }

        public void DeathIncognito()
        {
            incognito.DestroyEnemy();
        }
        
        // Audio management: all the following functions are used to handle Incognito's sounds
        public void StartRunningSound()
        {
            incognitoFootsteps.start();
        }
        
        public void StopRunningSound()
        {
            incognitoFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
        }

        public void StartIdleSound()
        {
            incognitoIdle.start();
        }
        
        public void StopIdleSound()
        {
            incognitoIdle.stop(STOP_MODE.ALLOWFADEOUT);
        }
        
        /// <summary>
        /// Pauses all sounds of this Incognito instance
        /// </summary>
        public void PauseAllSounds()
        {
            // Stop looping sounds
            if(incognitoFootsteps.isValid())
                incognitoFootsteps.setPaused(true);
            if(incognitoIdle.isValid())
                incognitoIdle.setPaused(true);
            
            // Stop one-shot sounds
            foreach (var instance in activeOneShotInstances)
            {
                if(instance.isValid()) instance.setPaused(true);
            }
        }

        /// <summary>
        /// Resumes all sounds of this Incognito instance
        /// </summary>
        public void ResumeAllSounds()
        {
            // Resume looping sounds
            if(incognitoFootsteps.isValid())
                incognitoFootsteps.setPaused(false);
            if(incognitoIdle.isValid())
                incognitoIdle.setPaused(false);
            
            // Resume one-shot sounds
            foreach (var instance in activeOneShotInstances)
            {
                if(instance.isValid()) instance.setPaused(false);
            }
        }
    }
}
