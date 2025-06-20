using TMPro;
using UnityEngine;

public class PaperTrigger : MonoBehaviour
{
    [SerializeField] private CollectablePapers collectablePapers;
    
    private bool playerInTrigger = false;
    private bool paperOpen = false;
    
    [SerializeField] private TextMeshProUGUI helpText;
    [SerializeField] private GameObject helpTextContainer;

    private PlayerInput playerInput;

	private void Start() {
		playerInput = Player.Instance.GetComponent<PlayerInput>();
	}

	void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            playerInTrigger = true;
            helpText.text = "Press E to collect paper";
            helpTextContainer.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            playerInTrigger = false;
            helpTextContainer.SetActive(false);
        }
    }

    void Update() {
        if (playerInTrigger && playerInput.InteractionPressed()) {
            if(!paperOpen) {
                paperOpen = true;
                collectablePapers.CollectPaper(this);
                helpTextContainer.SetActive(false);
            }
            else {
				collectablePapers.ClosePaper();
                paperOpen=false;
			}
        }
    }
}
