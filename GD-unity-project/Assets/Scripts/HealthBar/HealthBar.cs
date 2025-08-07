using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthBar;
    [SerializeField] private Image fillImage; 
    private Coroutine flashingCoroutine;

    public void SetMaxHealth(float health) {
        healthBar.maxValue = health;
        healthBar.value = health;
    }

    public void SetHealth(float health) {
        healthBar.value = health;
    }
    
    public void SetFlashing(bool shouldFlash)
    {
        // If the health bar has to flash and isn't already doing so, start the coroutine
        if (shouldFlash)
        {
            if (flashingCoroutine == null)
            {
                flashingCoroutine = StartCoroutine(FlashRoutine());
            }
        }
        // If it doesn't have to flash anymore...
        else
        {
            // ...stop the coroutine if it’s running...
            if (flashingCoroutine != null)
            {
                StopCoroutine(flashingCoroutine);
                flashingCoroutine = null;
            }
            // ...and make sure the bar is visible
            if (fillImage != null)
            {
                fillImage.enabled = true; 
            }
        }
    }
    
    private IEnumerator FlashRoutine()
    {
        // This loop continues until the coroutine is stopped from the outside
        while (true)
        {
            //Check if fillImage has been assigned to avoid errors
            if (fillImage != null)
            {
                //Turn off the image
                fillImage.enabled = false;
                yield return new WaitForSeconds(0.25f); // Pause

                // Accendi l'immagine
                fillImage.enabled = true;
                yield return new WaitForSeconds(0.5f); // Longer pause
            }
            else
            {
                // If fillImage is not assigned, print an error and stop the loop
                Debug.LogError("Fill Image was not assigned to the HealthBar Inspector!");
                yield break; // Exits the coroutine
            }
        }
    }
}