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

    private List<StudioEventEmitter> activeEmitters = new List<StudioEventEmitter>();
    private StudioEventEmitter alarmEmitter;

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
                            alarmEmitter = emitter;
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
            if (emitter != null && !emitter.IsPlaying())
            {
                emitter.Play();
            }
        }
    }

    // Attiva l'allarme
    public void ActivateAlarm()
    {
        if (alarmEmitter != null && !alarmEmitter.IsPlaying())
        {
            alarmEmitter.Play();
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

        if (alarmEmitter != null && alarmEmitter.IsPlaying())
        {
            alarmEmitter.EventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }
    
    /// <summary>
    /// Resetta lo stato degli emitter speciali (come l'allarme)
    /// senza spegnere i suoni ambientali di base.
    /// Viene chiamato dal sistema centrale all'inizio di un nuovo ciclo.
    /// </summary>
    public void ResetSpecialEmittersState()
    {
        // Al momento, l'unico emitter con uno "stato" è l'allarme.
        // Se l'allarme stava suonando, lo spegniamo.
        // Questo previene che un allarme attivato in un ciclo
        // venga considerato "già attivo" nel ciclo successivo.
        if (alarmEmitter != null && alarmEmitter.IsPlaying())
        {
            alarmEmitter.EventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }

}