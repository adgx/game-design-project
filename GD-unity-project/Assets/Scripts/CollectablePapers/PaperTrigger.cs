using UnityEngine;

public class PaperTrigger : MonoBehaviour
{
    [SerializeField] private CollectablePapers collectablePapers;
    
    private bool playerInTrigger = false;
    private bool paperOpen = false;

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            playerInTrigger = true;
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            playerInTrigger = false;
        }
    }

    void Update() {
        if (playerInTrigger && Input.GetKeyDown(KeyCode.E)) {
            if(!paperOpen) {
                paperOpen = true;
                collectablePapers.CollectPaper(this);
            }
            else {
				collectablePapers.ClosePaper();
                paperOpen=false;
			}
        }
    }
}
