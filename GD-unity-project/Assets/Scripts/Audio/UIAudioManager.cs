using FMODUnity;
using UnityEngine;

namespace Audio
{
    public class UIAudioManager : MonoBehaviour
    {
        public void PlayOpenSound()
        {   
            RuntimeManager.PlayOneShot(FMODEvents.Instance.PauseMenuOpen, transform.position);
        }
        
        public void PlayCloseSound()
        {       
            RuntimeManager.PlayOneShot(FMODEvents.Instance.PauseMenuClose, transform.position);
        }
        
        public void PlayPositiveSelectionSound()
        {
            RuntimeManager.PlayOneShot(FMODEvents.Instance.PauseMenuPositiveSelection, transform.position);
        }
        
        public void PlayNegativeSelectionSound()
        {       
            RuntimeManager.PlayOneShot(FMODEvents.Instance.PauseMenuNegativeSelection, transform.position);
        }

        public void PlayMasterVolumeControlSliderSound()
        {
            RuntimeManager.PlayOneShot(FMODEvents.Instance.MasterVolumeControlSlider, transform.position);
        }
        
        public void PlayMusicVolumeControlSliderSound()
        {
            RuntimeManager.PlayOneShot(FMODEvents.Instance.MusicVolumeControlSlider, transform.position);
        }
        
        public void PlayAmbienceVolumeControlSliderSound()
        {
            RuntimeManager.PlayOneShot(FMODEvents.Instance.AmbienceVolumeControlSlider, transform.position);
        }
        
        public void PlaySfxVolumeControlSliderSound()
        {
            RuntimeManager.PlayOneShot(FMODEvents.Instance.SfxVolumeControlSlider, transform.position);
        }
    }
}
