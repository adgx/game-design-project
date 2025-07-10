using System.Collections.Generic;
using UnityEngine;

public class StartTutorial : MonoBehaviour
{
    private List<string> tutorial = new List<string> {
        "Press left mouse button to shoot. Use the scroll wheel to select the attack type",
		"Press right mouse button to use the shield",
        "Interact with vending machines and terminals to recover health and obtain power ups"
	};

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(GameStatus.loopIteration == GameStatus.LoopIteration.FIRST_ITERATION) {
            
        }
    }
}
