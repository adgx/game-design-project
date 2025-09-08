using UnityEngine;
using TMPro;
using CollectablePapers;

public class PaperCounterUI : MonoBehaviour
{
    [Tooltip("Reference to the TextMeshProUGUI component that will display the paper counter.")]
    [SerializeField]
    private TextMeshProUGUI paperCountText;

    private int totalPapers;

    void Start()
    {
        if (paperCountText == null)
        {
            Debug.LogError("PaperCounterUI: paperCountText was not assigned! Assign your TextMeshProUGUI from the Inspector.");
            enabled = false;
            return;
        }
        
        if (PaperManager.Instance != null)
        {
            totalPapers = PaperManager.Instance.GetTotalPaperCount();
            UpdatePaperCounterUI();
        }
        else
        {
            Debug.LogError("PaperCounterUI: PaperManager.Instance is not available at the beginning. Make sure PaperManager initializes first.");
        }
    }

    // This method should be called every time a paper is collected
    public void UpdatePaperCounterUI()
    {
        if (PaperManager.Instance != null && paperCountText != null)
        {
            int collectedPapers = PaperManager.Instance._collectedPapers.Count;
            paperCountText.SetText($"Papers: {collectedPapers}/{totalPapers}");
        }
    }
}