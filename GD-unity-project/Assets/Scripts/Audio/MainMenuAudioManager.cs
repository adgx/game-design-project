using Audio;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
public class MainMenuAudioManager : MonoBehaviour
{
    private EventInstance musicEventInstance;
    
    private void Start()
    {
        InitializeMusic(FMODEvents.Instance.MainMenuMusic);
    }
    
    private void OnDestroy()
    {
        StopMusic();
    }
    
    private void InitializeMusic(EventReference musicEventReference)
    {
        musicEventInstance = RuntimeManager.CreateInstance(musicEventReference);
        musicEventInstance.start();
    }
    
    private void StopMusic()
    {
        musicEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        musicEventInstance.release();
    }
}
