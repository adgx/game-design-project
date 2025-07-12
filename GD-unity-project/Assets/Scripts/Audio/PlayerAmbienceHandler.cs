using UnityEngine;
using Utils; // Namespace dove si trova GameTimer

public class PlayerAmbienceHandler : MonoBehaviour
{
    private RoomAmbienceController currentRoomController;
    private GameTimer gameTimer;
    
    // All'avvio, trova il GameTimer e si iscrive all'evento dell'allarme.
    private void Start()
    {
        gameTimer = FindObjectOfType<GameTimer>();
        if (gameTimer != null)
        {
            GameTimer.OnTimerLow += HandleTimerLow;
        }
    }
    
    // Quando viene distrutto, si disiscrive per evitare errori.
    private void OnDestroy()
    {
        if (gameTimer != null)
        {
            GameTimer.OnTimerLow -= HandleTimerLow;
        }
    }

    // Quando entra in un'area trigger...
    private void OnTriggerEnter(Collider other)
    {
        var newRoomController = other.GetComponentInParent<RoomAmbienceController>();

        // Se non è una stanza valida o è la stessa in cui siamo già, non fare nulla.
        if (newRoomController == null || newRoomController == currentRoomController) return;

        // Se eravamo in un'altra stanza, spegni i suoi suoni.
        if (currentRoomController != null)
        {
            currentRoomController.DeactivateAllSounds();
        }

        // Aggiorna la stanza corrente e attiva i suoi suoni base.
        currentRoomController = newRoomController;
        currentRoomController.ActivateAmbience();
        
        // Se la condizione di allarme è già attiva, fai partire anche l'allarme.
        if (gameTimer != null && gameTimer.IsAlarmConditionActive)
        {
            currentRoomController.ActivateAlarm();
        }
    }
    
    // Metodo che viene chiamato dall'evento del GameTimer.
    private void HandleTimerLow()
    {
        // Se siamo in una stanza, fai partire il suo allarme.
        if (currentRoomController != null)
        {
            currentRoomController.ActivateAlarm();
        }
    }
}