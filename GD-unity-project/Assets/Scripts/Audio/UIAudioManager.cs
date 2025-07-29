using UnityEngine;

namespace Audio
{
    public class UIAudioManager : MonoBehaviour
    {
        public void PlayOpenSound()
        {   
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PauseMenuOpen, transform.position);
        }
        
        public void PlayCloseSound()
        {       
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PauseMenuClose, transform.position);
        }
        
        public void PlayPositiveSelectionSound()
        {
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PauseMenuPositiveSelection, transform.position);
        }
        
        public void PlayNegativeSelectionSound()
        {       
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.PauseMenuNegativeSelection, transform.position);
        }

        public void PlayMasterVolumeControlSliderSound()
        {
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MasterVolumeControlSlider, transform.position);
        }
        
        public void PlayMusicVolumeControlSliderSound()
        {
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.MusicVolumeControlSlider, transform.position);
        }
        
        public void PlayAmbienceVolumeControlSliderSound()
        {
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.AmbienceVolumeControlSlider, transform.position);
        }
        
        public void PlaySfxVolumeControlSliderSound()
        {
            GamePlayAudioManager.instance.PlayOneShot(FMODEvents.Instance.SfxVolumeControlSlider, transform.position);
        }
    }
}
