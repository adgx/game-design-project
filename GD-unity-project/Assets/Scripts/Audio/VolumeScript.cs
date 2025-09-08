using UnityEngine.EventSystems; 
using UnityEngine;
using UnityEngine.UI;

namespace Audio
{
    public class VolumeSlider : MonoBehaviour, IPointerUpHandler
    {
        private enum VolumeType {
            Master,
            Music,
            Ambience,
            Sfx
        }

        [Header("Type")]
        [SerializeField] private VolumeType volumeType;
        
        [Header("Dependencies")]
        [SerializeField] private UIAudioManager uiAudioManager;

        private Slider volumeSlider;

        private void Awake()
        {
            volumeSlider = this.GetComponentInChildren<Slider>();
        }

        private void Update()
        {
            switch (volumeType)
            {
                case VolumeType.Master:
                    volumeSlider.value = GamePlayAudioManager.instance.masterVolume;
                    break;
                case VolumeType.Music:
                    volumeSlider.value = GamePlayAudioManager.instance.musicVolume;
                    break;
                case VolumeType.Ambience:
                    volumeSlider.value = GamePlayAudioManager.instance.ambienceVolume;
                    break;
                case VolumeType.Sfx:
                    volumeSlider.value = GamePlayAudioManager.instance.SFXVolume;
                    break;
                default:
                    Debug.LogWarning("Volume Type not supported: " + volumeType);
                    break;
            }
        }

        public void OnSliderValueChanged()
        {
            switch (volumeType)
            {
                case VolumeType.Master:
                    GamePlayAudioManager.instance.masterVolume = volumeSlider.value;
                    break;
                case VolumeType.Music:
                    GamePlayAudioManager.instance.musicVolume = volumeSlider.value;
                    break;
                case VolumeType.Ambience:
                    GamePlayAudioManager.instance.ambienceVolume = volumeSlider.value;
                    break;
                case VolumeType.Sfx:
                    GamePlayAudioManager.instance.SFXVolume = volumeSlider.value;
                    break;
                default:
                    Debug.LogWarning("Volume Type not supported: " + volumeType);
                    break;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // Check if the reference has been assigned to avoid errors
            if (uiAudioManager != null)
            {
                // Check the type of this slider and call the correct function
                switch (volumeType)
                {
                    case VolumeType.Master:
                        uiAudioManager.PlayMasterVolumeControlSliderSound();
                        break;
                    case VolumeType.Music:
                        uiAudioManager.PlayMusicVolumeControlSliderSound();
                        break;
                    case VolumeType.Ambience:
                        uiAudioManager.PlayAmbienceVolumeControlSliderSound();
                        break;
                    case VolumeType.Sfx:
                        uiAudioManager.PlaySfxVolumeControlSliderSound();
                        break;
                    default:
                        Debug.LogWarning("No specific sound for the type slider: " + volumeType);
                        break;
                }
            }
            else
            {
                Debug.LogWarning("The reference to UIAudioManager was not set to the slider: " + gameObject.name);
            }
        }
    }
}