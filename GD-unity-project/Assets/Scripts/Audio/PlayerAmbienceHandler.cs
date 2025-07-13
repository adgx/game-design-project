// In PlayerAmbienceHandler.cs

using UnityEngine;
using Utils;

public class PlayerAmbienceHandler : MonoBehaviour
{
    private RoomAmbienceController currentRoomController;
    private GameTimer gameTimer;
    
    private void Start()
    {
        gameTimer = FindObjectOfType<GameTimer>();
        if (gameTimer != null)
        {
            GameTimer.OnTimerLow += HandleTimerLow;
        }
    }
    
    private void OnDestroy()
    {
        if (gameTimer != null)
        {
            GameTimer.OnTimerLow -= HandleTimerLow;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var newRoomController = other.GetComponentInParent<RoomAmbienceController>();

        // Se non è una stanza valida o è la stessa in cui siamo già, esci subito.
        if (newRoomController == null || newRoomController == currentRoomController)
        {
            return;
        }

        Debug.Log($"Cambiando stanza da '{(currentRoomController != null ? currentRoomController.name : "NULL")}' a '{newRoomController.name}'");

        // Spegni la vecchia stanza PRIMA di fare qualsiasi altra cosa.
        if (currentRoomController != null)
        {
            currentRoomController.DeactivateAllSounds();
        }

        // Aggiorna il riferimento e SOLO DOPO attiva i suoni nuovi.
        currentRoomController = newRoomController;
        currentRoomController.ActivateAmbience();
        
        // Attiva l'allarme se necessario.
        if (gameTimer != null && gameTimer.IsAlarmConditionActive)
        {
            currentRoomController.ActivateAlarm();
        }
    }
    
    private void HandleTimerLow()
    {
        // Se siamo in una stanza, fai partire il suo allarme.
        if (currentRoomController != null)
        {
            currentRoomController.ActivateAlarm();
        }
    }
}