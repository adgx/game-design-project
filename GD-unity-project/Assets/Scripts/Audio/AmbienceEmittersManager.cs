using Helper;
using System.Collections;
using System.Collections.Generic;
using Audio;
using TMPro;
using UnityEngine;

// Audio management
using FMODUnity;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Audio
{
    public class AmbienceEmittersManager : MonoBehaviour
    {
        public void InitializeEventEmittersWithTag(string tagValue, EventReference eventRef, List<StudioEventEmitter> emitters) {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tagValue);
            foreach(GameObject obj in objects) {
                StudioEventEmitter emitter = GamePlayAudioManager.instance.InitializeEventEmitter(eventRef, obj);
                if(emitter != null) {
                    emitters.Add(emitter);
                }
                else {
                    Debug.LogWarning($"Missing StudioEventEmitter on {obj.name}");
                }
            }
        }

        public void playEventEmitters(List<StudioEventEmitter> emitters, bool condition, ref bool isTriggered) {
            if(condition) {
                foreach(var emitter in emitters) {
                    if(emitter != null && emitter.gameObject != null) {
                        emitter.Play();
                    }
                }
                isTriggered = true;
            }
        }

        public void resetEventEmitters(List<StudioEventEmitter> emitters, ref bool isTriggered) {
            isTriggered = false;
            emitters.Clear();
        }

        
        
        
        
        
    }
}
