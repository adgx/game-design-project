using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

// Questa classe può stare nello stesso file o in uno separato.
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

    // Quando la stanza viene creata
    private void Awake()
    {
        InitializeRoomEmitters();
        // Registra questa stanza al sistema centrale
        AmbienceSystem.Register(this);
    }

    // Quando la stanza viene distrutta
    private void OnDestroy()
    {
        // È fondamentale deregistrare la stanza per evitare riferimenti a oggetti distrutti
        AmbienceSystem.Unregister(this);
    }

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
                    // Assumendo che tu abbia un GamePlayAudioManager per creare gli emitter
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

    // Attiva i suoni base (non l'allarme)
    public void ActivateAmbience()
    {
        foreach (var emitter in activeEmitters)
        {
            // Fai partire il suono solo se:
            // 1. L'emitter esiste
            // 2. Il GameObject a cui è attaccato è attivo nella scena
            // 3. Non sta già suonando
            if (emitter != null && emitter.gameObject.activeInHierarchy && !emitter.IsPlaying())
            {
                emitter.Play();
            }
        }
    }

    // Spegne TUTTI i suoni di questa stanza IMMEDIATAMENTE
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
    
    // Attiva l'allarme
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