using System;
using System.Threading.Tasks;
using CollectablePapers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PapersMenu : MonoBehaviour
{
    [Header("Components")] [SerializeField]
    private GameObject papersMenu;

    [SerializeField] private TextMeshProUGUI paperText;
    [SerializeField] private GameObject paperScrollContent;
    [SerializeField] private GameObject paperButton;

    public async void OpenMenu()
    {
        foreach (Transform child in paperScrollContent.transform)
        {
            Destroy(child.gameObject);
        }

        paperButton.transform.localPosition = Vector3.zero;
        paperText.text = "The papers collected will be shown here";
        papersMenu.gameObject.SetActive(true);

        int totalPapers = PaperManager.Instance.GetTotalPaperCount();

        for (int i = 0; i < totalPapers; i++)
        {
            int buttonIndex = i;
            GameObject button = Instantiate(paperButton, paperScrollContent.transform, true);

            button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Paper " + (i + 1);
            button.GetComponent<Button>().onClick.AddListener(() => ShowPaper(buttonIndex));

            bool isCollected = PaperManager.Instance.IsPaperCollected(buttonIndex);
            button.GetComponent<Button>().interactable = isCollected;

            await Task.Delay(1);
            button.transform.localPosition = new Vector3(141, -30 - (78 * i), 0);
            button.transform.localScale = Vector3.one;
            button.SetActive(true);
        }
    }

    public void CloseMenu()
    {
        papersMenu.gameObject.SetActive(false);
    }

    private void ShowPaper(int index)
    {
        if (PaperManager.Instance.TryGetPaperContent(index, out string content))
        {
            paperText.text = content;
        }
        else
        {
            paperText.text = $"Error: Could not find content for paper with ID {index}.";
        }
    }
}