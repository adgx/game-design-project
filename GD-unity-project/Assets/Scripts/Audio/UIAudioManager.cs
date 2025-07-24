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
    }
}
