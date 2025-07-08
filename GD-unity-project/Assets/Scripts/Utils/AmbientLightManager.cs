using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AmbientLightManager : MonoBehaviour
{
    [Header("Cyclic light settings")]
    [Tooltip("The light intensity at the start of the match and at the end of each cycle.")]
    private static float initialIntensity = 0.75f;
    
    [Tooltip("List of intensities to be applied each time the timer expires.")]
    public List<float> intensitySequence = new List<float> { 0.5f, 0.1f };

    [Tooltip("The duration of the transition in seconds.")]
    public float transitionDuration = 5.0f;

    // A static counter to track how many times the timer has expired
    private static int sequenceIndex = 0;

    void Start()
    {
        // At startup, immediately set the light intensity to its initial value.
        RenderSettings.ambientIntensity = initialIntensity;
        
        if (GameEvents.current != null)
        {
            GameEvents.current.onTimerEnd += OnTimerEnded;
        }
        else
        {
            Debug.LogError("AmbientLightManager: CRITICAL! GameEvents.current is NULL in Start().");
        }
    }

    private void OnDisable()
    {
        if (GameEvents.current != null)
        {
            GameEvents.current.onTimerEnd -= OnTimerEnded;
        }
    }

    // The method that is called by the event
    private void OnTimerEnded()
    {
        float targetIntensity;
        
        if (sequenceIndex < intensitySequence.Count)
        {
            // If we are still within the sequence, take the next value.
            targetIntensity = intensitySequence[sequenceIndex];
            
            // Increase the index for next time.
            sequenceIndex++;
        }
        else
        {
            // If the sequence is finished, it’s time to return to the initial value and reset the cycle.
            targetIntensity = initialIntensity;

            // Resets the index to 0 to restart the cycle next time.
            sequenceIndex = 0;
        }
        // ------------------------------------

        StartCoroutine(FadeAmbientIntensity(targetIntensity));
    }

    private IEnumerator FadeAmbientIntensity(float target)
    {
        float startIntensity = RenderSettings.ambientIntensity;
        float time = 0;

        while (time < transitionDuration)
        {
            RenderSettings.ambientIntensity = Mathf.Lerp(startIntensity, target, time / transitionDuration);
            time += Time.deltaTime;
            yield return null;
        }

        RenderSettings.ambientIntensity = target;
    }
    
    // Optional method to reset the counter if necessary (e.g. at the start of a new game)
    public static void ResetLightSequence()
    {
        sequenceIndex = 0;
        RenderSettings.ambientIntensity = initialIntensity;
    }
}