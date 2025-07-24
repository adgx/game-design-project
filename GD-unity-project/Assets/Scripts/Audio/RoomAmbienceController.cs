using System;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

namespace Audio
{
    // This class can be in the same file or a separate
    [System.Serializable]
    public class AmbienceSoundDefinition
    {
        public string objectTag;
        public EventReference fmodEvent;
    }

    public class RoomAmbienceController : MonoBehaviour
    {
        [SerializeField]
        private List<AmbienceSoundDefinition> soundsToManage;

        private List<StudioEventEmitter> activeEmitters;
        private List<StudioEventEmitter> alarmEmitters;

        // When the room is created
        [Obsolete("Obsolete")]
        private void Awake()
        {
            InitializeRoomEmitters();
            // Register this room to the central system
            AmbienceSystem.Register(this);
        }

        // When the room is destroyed
        private void OnDestroy()
        {
            // It is essential to unregister the room to avoid references to destroyed objects
            AmbienceSystem.Unregister(this);
        }

        [Obsolete("Obsolete")]
        private void InitializeRoomEmitters()
        {
            activeEmitters = new List<StudioEventEmitter>();
            alarmEmitters = new List<StudioEventEmitter>();
        
            foreach (var definition in soundsToManage)
            {
                Transform[] children = GetComponentsInChildren<Transform>(true);
                foreach (Transform child in children)
                {
                    if (child.CompareTag(definition.objectTag))
                    {
                        // Assuming you have a GamePlayAudioManager to create the emitter
                        var emitter = GamePlayAudioManager.instance.InitializeEventEmitter(definition.fmodEvent, child.gameObject);
                        if (emitter != null)
                        {
                            if (definition.objectTag == "AlarmSpeaker")
                            {
                                alarmEmitters.Add(emitter);
                            }
                            else
                            {
                                activeEmitters.Add(emitter);
                            }
                        }
                    }
                }
            }
        }

        // Activate the base sounds (not the alarm)
        public void ActivateAmbience()
        {
            foreach (var emitter in activeEmitters)
            {
                // Play the sound only if:
                // 1. The emitter exists
                // 2. The attached GameObject is active in the scene
                // 3. Not playing yet
                if (emitter != null && emitter.gameObject.activeInHierarchy && !emitter.IsPlaying())
                {
                    emitter.Play();
                }
            }
        }

        // Turn off all sounds in this room immediately
        public void DeactivateAllSounds()
        {
            foreach (var emitter in activeEmitters)
            {
                if (emitter != null && emitter.IsPlaying())
                {
                    emitter.EventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                }
            }

            foreach (var alarm in alarmEmitters)
            {
                if (alarm != null && alarm.IsPlaying())
                {
                    alarm.EventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                }
            }
        }
    
        // Activate the alarm
        public void ActivateAlarm()
        {
            foreach (var alarm in alarmEmitters)
            {
                if (alarm != null && !alarm.IsPlaying())
                {
                    alarm.Play();
                }
            }
        }
    }
}