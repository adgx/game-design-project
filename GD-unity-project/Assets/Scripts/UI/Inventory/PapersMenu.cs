using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PapersMenu : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private GameObject papersMenu;
	[SerializeField] private TextMeshProUGUI paperText;
	[SerializeField] private GameObject paperScrollContent;

	[SerializeField] private GameObject paperButton;

	[SerializeField] private CollectablePapers collectablePapers;

	[SerializeField] private GameObject firstSelected;

	public async void OpenMenu() {
		foreach(Transform child in paperScrollContent.transform) {
			Destroy(child.gameObject);
		}

		paperButton.transform.localPosition = Vector3.zero;

		paperText.text = "The papers collected will be shown here";

		EventSystem.current.SetSelectedGameObject(firstSelected);
		papersMenu.gameObject.SetActive(true);

		for(int i = 0; i < collectablePapers.messages.Length; i++) {
			int buttonIndex = i;
			GameObject button = Instantiate(paperButton);

			button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Paper " + (i + 1);
			button.GetComponent<Button>().onClick.AddListener(() => ShowPaper(buttonIndex));

			button.transform.SetParent(paperScrollContent.transform, true);

			if(i >= collectablePapers.lastPaperCollected) {
				button.GetComponent<Button>().interactable = false;
			}

			// This await is needed because otherwise the localPosition of the button would be changed in a weird way
			await Task.Delay(1);

			button.transform.localPosition = new Vector3(141, -30 - (78 * i), 0);
			button.transform.localScale = Vector3.one;

			button.SetActive(true);
		}
	}

	public void CloseMenu() {
		papersMenu.gameObject.SetActive(false);
	}

	public void ShowPaper(int index) {
		paperText.text = collectablePapers.messages[index];
	}
}