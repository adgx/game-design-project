using System;
using UnityEngine;
using Utils;

public class PlayerAmbienceHandler : MonoBehaviour
{
    private RoomAmbienceController currentRoomController;
    private GameTimer gameTimer;
    
    [Obsolete("Obsolete")]
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

        // If it is not a valid room or it is the same in which we are already, leave immediately
        if (newRoomController == null || newRoomController == currentRoomController)
        {
            return;
        }

        // Turn off the old room BEFORE doing anything else
        if (currentRoomController != null)
        {
            currentRoomController.DeactivateAllSounds();
        }

        // Update the reference and only after activate new sounds
        currentRoomController = newRoomController;
        currentRoomController.ActivateAmbience();
        
        // Activate the alarm if necessary
        if (gameTimer != null && gameTimer.IsAlarmConditionActive)
        {
            currentRoomController.ActivateAlarm();
        }
    }
    
    private void HandleTimerLow()
    {
        // If we are in a room, start his alarm
        if (currentRoomController != null)
        {
            currentRoomController.ActivateAlarm();
        }
    }
}