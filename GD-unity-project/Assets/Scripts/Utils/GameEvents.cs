using System;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    // Singleton: a static instance that anyone can access
    public static GameEvents current;

    private void Awake()
    {
        // Ensures that there is only one instance of GameEvents
        if (current == null)
        {
            current = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // The event that other scripts will "sign up for
    public event Action onTimerEnd;

    // Method the timer will call to trigger the event
    public void TimerEnded()
    {
        // Check if someone is listening before sending the event
        if (onTimerEnd != null)
        {
            onTimerEnd();
        }
    }
}