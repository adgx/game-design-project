using UnityEngine;

public class PowerUpMenu : MonoBehaviour {
	[Header("Components")]
	[SerializeField] private GameObject powerUpMenu;

	public void OpenMenu() {
		powerUpMenu.gameObject.SetActive(true);
	}

	public void CloseMenu() {
		powerUpMenu.gameObject.SetActive(false);
	}
}