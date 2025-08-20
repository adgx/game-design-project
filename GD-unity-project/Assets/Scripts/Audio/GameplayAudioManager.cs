using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Audio
{
    public class GamePlayAudioManager : MonoBehaviour
    {
        [Header("Volume")]
        [Range(0, 1)]
        public float masterVolume = 1;
        [Range(0, 1)]
        public float musicVolume = 1;
        [Range(0, 1)]
        public float ambienceVolume = 1;
        [Range(0, 1)]
        public float SFXVolume = 1;

        private Bus masterBus;
        private Bus musicBus;
        private Bus ambienceBus;
        private Bus sfxBus;

        private List<EventInstance> eventInstances;
        private List<StudioEventEmitter> eventEmitters;
    
        private EventInstance musicEventInstance;
        public static GamePlayAudioManager instance { get; private set; }
        private List<EventInstance> managedOneShotInstances = new List<EventInstance>();

        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("Found more than one Audio Manager in the scene.");
            }
            instance = this;
        
            eventInstances = new List<EventInstance>();
            eventEmitters = new List<StudioEventEmitter>();
        
            masterBus = RuntimeManager.GetBus("bus:/");
            musicBus = RuntimeManager.GetBus("bus:/Music");
            ambienceBus = RuntimeManager.GetBus("bus:/Ambience");
            sfxBus = RuntimeManager.GetBus("bus:/SFX");
            InitializeMusic(FMODEvents.Instance.GameplayMusic); 
        }
    
        private void Update()
        {
            masterBus.setVolume(masterVolume);
            musicBus.setVolume(musicVolume);
            ambienceBus.setVolume(ambienceVolume);
            sfxBus.setVolume(SFXVolume);
        }
        
        private void OnDestroy()
        {
            StopAndReleaseAllEvents();
        }
        
        public void PlayManagedOneShot(EventReference sound, Vector3 worldPos)
        {
            if (!Application.isPlaying || !RuntimeManager.IsInitialized) return;

            EventInstance eventInstance = CreateInstance(sound);
            eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(worldPos));
            
            managedOneShotInstances.Add(eventInstance);
            
            eventInstance.start();
            StartCoroutine(ReleaseManagedInstanceWhenFinished(eventInstance));
        }
        
        private IEnumerator PlayWithDelayCoroutine(EventReference sound, Vector3 worldPos, float delay)
        {
            yield return new WaitForSeconds(delay);
            PlayManagedOneShot(sound, worldPos);
        }
        
        private IEnumerator ReleaseManagedInstanceWhenFinished(EventInstance eventInstance)
        {
            PLAYBACK_STATE playbackState;
            do {
                eventInstance.getPlaybackState(out playbackState);
                yield return null;
            } while (playbackState != PLAYBACK_STATE.STOPPED);

            // Remove from the list of managed one-shots
            managedOneShotInstances.Remove(eventInstance);
            
            // Release the instance using your existing method, which also removes it from the main list.
            ReleaseInstance(eventInstance);
        }
        
        public void PauseAllManagedOneShots()
        {
            foreach (var instance in managedOneShotInstances)
            {
                if(instance.isValid()) instance.setPaused(true);
            }
        }
        
        public void ResumeAllManagedOneShots()
        {
            foreach (var instance in managedOneShotInstances)
            {
                if(instance.isValid()) instance.setPaused(false);
            }
        }
        
        private void InitializeMusic(EventReference musicEventReference)
        {
            if (musicEventInstance.isValid())
            {
                return;
            }
            musicEventInstance = CreateInstance(musicEventReference);
            musicEventInstance.start();
        }

        public void SetMusicLoopIteration()
        {
            if (musicEventInstance.isValid())
            {
                musicEventInstance.setParameterByName("loopIteration", (float)GameStatus.loopIteration);
            }
            else
            {
                Debug.LogWarning("SetMusicLoopIteration called, but music instance is not valid yet.");
            }
        }
    
        public EventInstance CreateInstance(EventReference eventReference)
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
            eventInstances.Add(eventInstance);
            return eventInstance;
        }
    
        [Obsolete("Obsolete")]
        public StudioEventEmitter InitializeEventEmitter(EventReference eventReference, GameObject emitterGameObject)
        {
            if (emitterGameObject == null)
            {
                Debug.LogError("InitializeEventEmitter failed: emitterGameObject is null.");
                return null;
            }

            // Check if an emitter already exists
            StudioEventEmitter emitter = emitterGameObject.GetComponent<StudioEventEmitter>();

            // If it does not exist, we create it
            if (emitter == null)
            {
                emitter = emitterGameObject.AddComponent<StudioEventEmitter>();
            }
    
            // Now that we are sure it exists, let’s configure it
            emitter.EventReference = eventReference;
    
            // We disable the auto-play options of FMOD to have total control
            emitter.PlayEvent = EmitterGameEvent.None;
            emitter.StopEvent = EmitterGameEvent.None;

            return emitter;
        }

        public void StopAndReleaseAllEvents()
        {
            // Stop and release any created instances
            foreach (EventInstance eventInstance in eventInstances)
            {
                // Safety check
                if (eventInstance.isValid())
                { 
                    eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE); 
                    eventInstance.release();   
                }
            }
        
            // Stop all the event emitters, because if we don't they may hang around in other scenes
            foreach (StudioEventEmitter emitter in eventEmitters)
            {
                if(emitter != null && emitter.IsPlaying())
                    emitter.Stop();
            }
        
            // Clear lists for next boot
            eventInstances.Clear();
            eventEmitters.Clear();
            managedOneShotInstances.Clear();

            Debug.Log("GamePlayAudioManager: All events were stopped and released manually.");
        }

        // Allows any script to request the release of a specific auio instance
        public void ReleaseInstance(EventInstance eventInstance)
        {
            if (eventInstance.isValid())
            {
                // Stop and release a given instance
                eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                eventInstance.release(); 
            
                // Remove it from the list so you don’t try to release it again in CleanUp()
                eventInstances.Remove(eventInstance);
            }
        }
    
        /// <summary>
        /// Pause background music.
        /// </summary>
        public void PauseMusic()
        {
            if (musicEventInstance.isValid())
            {
                musicEventInstance.setPaused(true);
            }
        }

        /// <summary>
        /// Background music resumes playback.
        /// </summary>
        public void ResumeMusic()
        {
            if (musicEventInstance.isValid())
            {
                musicEventInstance.setPaused(false);
            }
        }
    
        /// <summary>
        /// Pauses all sounds that pass through the Ambience bus.
        /// </summary>
        public void PauseAmbience()
        {
            if (ambienceBus.isValid())
            {
                ambienceBus.setPaused(true);
            }
        }

        /// <summary>
        /// Resumes playback of all sounds in the Ambience bus.
        /// </summary>
        public void ResumeAmbience()
        {
            if (ambienceBus.isValid())
            {
                ambienceBus.setPaused(false);
            }
        }
    }
}