using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartTutorial : MonoBehaviour
{
    private List<string> tutorial = new List<string> {
        "There are other papers like this around the laboratory. Find all of them!",
        "Press left mouse button to shoot. Use the scroll wheel to select the attack type",
		"You can press the right mouse button to use the shield",
        "Interact with vending machines and terminals to recover health and obtain power ups"
	};

    [SerializeField] private GameObject helpTextContainer;
	[SerializeField] private TextMeshProUGUI helpText;

    [SerializeField] private int tipDuration = 3;

	public IEnumerator ShowTip(int i) {
        if(i < tutorial.Count) {
            helpText.text = tutorial[i];
            helpTextContainer.SetActive(true);

            yield return new WaitForSeconds(tipDuration);

            helpTextContainer.SetActive(false);
        }
    }
}
