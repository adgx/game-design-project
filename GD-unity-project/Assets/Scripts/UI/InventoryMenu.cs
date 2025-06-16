using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryMenu : MonoBehaviour {
	[SerializeField] private GameObject screenContainer;
	[SerializeField] private GameObject inventoryMenu;
	[SerializeField] private PapersMenu papersMenuScript;
	[SerializeField] private PowerUpMenu powerUpMenuScript;

	[SerializeField] private Sprite buttonHoverSprite;
	[SerializeField] private Sprite buttonNormalSprite;

	private PlayerInput playerInput;

	private bool inventoryMenuActive = false;

	private void Start() {
		playerInput = Player.Instance.GetComponent<PlayerInput>();

		screenContainer.SetActive(false);
		inventoryMenu.SetActive(false);
		papersMenuScript.CloseMenu();
		powerUpMenuScript.CloseMenu();
	}

	// Update is called once per frame
	void Update() {
		if(playerInput.IPressed()) {

			GameStatus.gamePaused = !GameStatus.gamePaused;
			if(GameStatus.gamePaused) {
				// Setting timeScale to 0 pauses the game
				Time.timeScale = 0f;
			}
			else {
				// Resume the game
				Time.timeScale = 1f;
				EventSystem.current.SetSelectedGameObject(null);
			}

			ToggleInventoryMenu();
		}
	}

	void ToggleInventoryMenu() {
		if(screenContainer.activeInHierarchy) {
			screenContainer.SetActive(false);
			inventoryMenu.SetActive(false);
			papersMenuScript.CloseMenu();
			powerUpMenuScript.CloseMenu();
		}
		else {
			screenContainer.SetActive(true);
			inventoryMenu.SetActive(true);
		}
	}

	async public void ResumeGameButtonClick(GameObject button) {
		button.GetComponent<Image>().sprite = buttonNormalSprite;
		button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;

		screenContainer.SetActive(false);
		inventoryMenu.SetActive(false);
		papersMenuScript.CloseMenu();
		powerUpMenuScript.CloseMenu();

		await Task.Delay(100);
		EventSystem.current.SetSelectedGameObject(null);
		GameStatus.gamePaused = false;
		// Resume the game
		Time.timeScale = 1f;
	}

	public void PapersButtonClick(GameObject button) {
		button.GetComponent<Image>().sprite = buttonNormalSprite;
		button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;

		inventoryMenu.SetActive(false);
		papersMenuScript.OpenMenu();
	}

	public void PowerUpButtonClick(GameObject button) {
		button.GetComponent<Image>().sprite = buttonNormalSprite;
		button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;

		inventoryMenu.SetActive(false);
		powerUpMenuScript.OpenMenu();
	}

	public void BackToInventoryButtonClick() {
		papersMenuScript.CloseMenu();
		powerUpMenuScript.CloseMenu();
		inventoryMenu.SetActive(true);
	}

	public void OnMouseEnter(GameObject button) {
		button.GetComponent<Image>().sprite = buttonHoverSprite;
		button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.black;
		EventSystem.current.SetSelectedGameObject(button);
	}

	public void OnMouseExit(GameObject button) {
		button.GetComponent<Image>().sprite = buttonNormalSprite;
		button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
	}
}
