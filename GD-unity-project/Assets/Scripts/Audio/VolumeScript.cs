using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    private enum VolumeType {
        MASTER,
        MUSIC,
        AMBIENCE,
        SFX
    }

    [Header("Type")]
    [SerializeField] private VolumeType volumeType;

    private Slider volumeSlider;

    private void Awake()
    {
        volumeSlider = this.GetComponentInChildren<Slider>();
    }

    private void Update()
    {
        switch (volumeType)
        {
            case VolumeType.MASTER:
                volumeSlider.value = GamePlayAudioManager.instance.masterVolume;
                break;
            case VolumeType.MUSIC:
                volumeSlider.value = GamePlayAudioManager.instance.musicVolume;
                break;
            case VolumeType.AMBIENCE:
                volumeSlider.value = GamePlayAudioManager.instance.ambienceVolume;
                break;
            case VolumeType.SFX:
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
            case VolumeType.MASTER:
                GamePlayAudioManager.instance.masterVolume = volumeSlider.value;
                break;
            case VolumeType.MUSIC:
                GamePlayAudioManager.instance.musicVolume = volumeSlider.value;
                break;
            case VolumeType.AMBIENCE:
                GamePlayAudioManager.instance.ambienceVolume = volumeSlider.value;
                break;
            case VolumeType.SFX:
                GamePlayAudioManager.instance.SFXVolume = volumeSlider.value;
                break;
            default:
                Debug.LogWarning("Volume Type not supported: " + volumeType);
                break;
        }
    }
}