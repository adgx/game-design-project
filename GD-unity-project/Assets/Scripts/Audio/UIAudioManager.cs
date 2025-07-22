using System;
using FMODUnity;
using UnityEngine;

namespace Audio
{
    [Obsolete("Obsolete")]
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
    }
}
