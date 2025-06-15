using UnityEngine;

public class PapersMenu : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private GameObject papersMenu;

	public void OpenMenu() {
		papersMenu.gameObject.SetActive(true);
	}

	public void CloseMenu() {
		papersMenu.gameObject.SetActive(false);
	}
}