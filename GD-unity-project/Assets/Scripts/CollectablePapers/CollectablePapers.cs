using System;
using TMPro;
using UnityEngine;
using ORF;

public class CollectablePapers : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI paperText;
    public GameObject paperTextContainer;
    [SerializeField] private Player player;
    
    private const int N_PAPERS = 7;
    
    private bool[] papers = new bool[N_PAPERS];
    private int lastPaperCollected = 0;

    private string[] messages =
    {
        "- What happened? Where am I?\n" +
        "- Something with your experiments went horribly wrong, you are trapped here now\n" +
        "- Wait... who are you?\n" +
        "- Find the next paper and you'll know\n" +
        "Press E to continue",

        "- Oh, here you are again. I am yourself... from the past\n" +
        "- What do you mean... from the past?\n" +
        "- Oh so you don't remember. You wrote these notes, while making all your fancy experiments on magnetism. Those experiments were not so fancy, I guess\n" +
        "Press E to continue",
        
        "Paper 3",
        
        "Paper 4",
        
        "Paper 5",
        
        "Paper 6",
        
        "Paper 7"
    };

    public void CollectPaper(PaperTrigger caller) {
        if (lastPaperCollected < N_PAPERS) {
            papers[lastPaperCollected] = true;
            
            player.isFrozen = true;
            paperText.SetText(messages[lastPaperCollected]);
            paperTextContainer.SetActive(true);

            lastPaperCollected++;

			// Audio management
			AudioManager.instance.PlayOneShot(FMODEvents.instance.paperInteraction, caller.transform.position);
		}
    }

    public void ClosePaper() {
		paperTextContainer.SetActive(false);
		player.isFrozen = false;
	}

    private void Update()
    {
    }
}
