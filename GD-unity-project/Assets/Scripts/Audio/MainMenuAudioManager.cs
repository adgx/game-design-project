using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Audio
{
    public class MainMenuAudioManager : MonoBehaviour
    {
        private EventInstance musicEventInstance;
    
        private void Start()
        {
            InitializeMusic(FMODEvents.Instance.MainMenuMusic);
        }
    
        private void OnDestroy()
        {
            if (FMODUnity.RuntimeManager.IsInitialized)
            {
                StopMusic();   
            }
        }
    
        private void InitializeMusic(EventReference musicEventReference)
        {
            musicEventInstance = RuntimeManager.CreateInstance(musicEventReference);
            musicEventInstance.start();
        }
    
        private void StopMusic()
        {
            if (musicEventInstance.isValid())
            {
                musicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                musicEventInstance.release();   
            }
        }
    }
}
