using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

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
    
    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        if (!Application.isPlaying || !RuntimeManager.IsInitialized)
            return;

        RuntimeManager.PlayOneShot(sound, worldPos);
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
    
    private void CleanUp()
    {
        // Stop and release any created instances
        foreach (EventInstance eventInstance in eventInstances)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
        
        // Stop all of the event emitters, because if we don't they may hang around in other scenes
        foreach (StudioEventEmitter emitter in eventEmitters)
        {
            if(emitter != null && emitter.IsActive)
                emitter.Stop();
        }
        
        // Clear lists for next boot
        eventInstances.Clear();
        eventEmitters.Clear();
    }

    private void OnDestroy()
    {
        CleanUp();
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
}