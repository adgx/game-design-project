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
    
    public StudioEventEmitter InitializeEventEmitter(EventReference eventReference, GameObject emitterGameObject)
    {
        if (emitterGameObject == null)
        {
            Debug.LogError("InitializeEventEmitter failed: emitterGameObject is null.");
            return null;
        }

        StudioEventEmitter emitter = emitterGameObject.GetComponent<StudioEventEmitter>();
        if (emitter == null)
        {
            Debug.LogError($"InitializeEventEmitter failed: No StudioEventEmitter found on {emitterGameObject.name}.");
            return null;
        }

        emitter.EventReference = eventReference;
        eventEmitters.Add(emitter);
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
            if(emitter.IsActive)
                emitter.Stop();
        }
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