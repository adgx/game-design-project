using UnityEngine;

public class PaperTrigger : MonoBehaviour
{
    [SerializeField] private CollectablePapers collectablePapers;
    
    private bool playerInTrigger = false;

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
            Debug.Log("Paper triggered");
            collectablePapers.CollectPaper();
        }
    }
}
