using System.Collections;
using System.Collections.Generic;
using PlayerInteraction;
using TMPro;
using UnityEngine;

public class StartTutorial : MonoBehaviour
{
    [SerializeField]
    public List<string> tutorial = new List<string> {
        "There are more papers around the laboratory. Find them all! Remember, the sphere's LEDs show its charge, from blue to off.",
        "Press left mouse button to shoot. Use the scroll wheel to select the attack type.",
        "You can press the right mouse button to use the shield.",
        "Interact with vending machines and terminals to recover health and obtain power ups."
    };

    [SerializeField] private GameObject helpTextContainer;
    [SerializeField] private TextMeshProUGUI helpText;
    [SerializeField] private int tipDuration = 5;
    [SerializeField] private PlayerInteractor _playerInteractor;

    public IEnumerator ShowTip(int i) {
        if(i < tutorial.Count) {
            
            // Force the PlayerInteractor to show the health recovery prompt
            if (_playerInteractor != null)
            {
                _playerInteractor.ShowForcedPrompt(tutorial[i]);
            }

            yield return new WaitForSeconds(tipDuration);
            
            // Once the feedback message is gone, make sure the UI prompt updates
            if (_playerInteractor != null)
            {
                _playerInteractor.ClearForcedPrompt(); // Send the signal to PlayerInteractor
            }
        }
    }
}