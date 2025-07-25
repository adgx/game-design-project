using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Audio
{
    public class DeathMenuAudioManager : MonoBehaviour
    {
        private EventInstance musicEventInstance;
    
        private void Start()
        {
            InitializeMusic(FMODEvents.Instance.DeathMenuMusic);
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
}
