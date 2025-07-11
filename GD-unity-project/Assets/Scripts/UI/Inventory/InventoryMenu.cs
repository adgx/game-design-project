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
	
	[SerializeField] private GameObject firstSelected;

	[SerializeField] private Sprite buttonHoverSprite;
	[SerializeField] private Sprite buttonNormalSprite;

	private PlayerInput playerInput;

	private bool inventoryScreenOpen = false;

	private void Start() {
		playerInput = Player.Instance.GetComponent<PlayerInput>();

		inventoryScreenOpen = false;
		screenContainer.SetActive(false);
		inventoryMenu.SetActive(false);
		papersMenuScript.CloseMenu();
		powerUpMenuScript.CloseMenu();
	}

	// Update is called once per frame
	void Update() {
		if(playerInput.InventoryPressed() || (inventoryScreenOpen && inventoryMenu.activeInHierarchy && playerInput.BackKeyPressed())) {

			ChangeGameState(!GameStatus.gamePaused);

			ToggleInventoryMenu();
		}

		if(inventoryScreenOpen && !inventoryMenu.activeInHierarchy && playerInput.BackKeyPressed()) {
			BackToInventory();
		}
	}

	async void ChangeGameState(bool status) {
		if(status) {
			// Setting timeScale to 0 pauses the game
			Time.timeScale = 0f;
			Cursor.lockState = CursorLockMode.None;
		}
		else {
			// Resume the game
			Time.timeScale = 1f;
			await Task.Delay(100);
			EventSystem.current.SetSelectedGameObject(null);
			Cursor.lockState = CursorLockMode.Locked;
		}

		GameStatus.gamePaused = status;
	}

	void ToggleInventoryMenu() {
		if(screenContainer.activeInHierarchy) {
			inventoryScreenOpen = false;
			screenContainer.SetActive(false);
			inventoryMenu.SetActive(false);
			papersMenuScript.CloseMenu();
			powerUpMenuScript.CloseMenu();
		}
		else {
			inventoryScreenOpen = true;
			screenContainer.SetActive(true);
			inventoryMenu.SetActive(true);
			
			EventSystem.current.SetSelectedGameObject(firstSelected);
		}
	}

	void BackToInventory() {
		papersMenuScript.CloseMenu();
		powerUpMenuScript.CloseMenu();
		inventoryMenu.SetActive(true);

		if(!EventSystem.current.alreadySelecting)
			EventSystem.current.SetSelectedGameObject(firstSelected);
	}

	public void ResumeGameButtonClick(GameObject button) {
		button.GetComponent<Image>().sprite = buttonNormalSprite;
		button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;

		ToggleInventoryMenu();

		ChangeGameState(false);
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
		BackToInventory();
	}

	public void OnMouseEnter(GameObject button) {
		button.GetComponent<Image>().sprite = buttonHoverSprite;
		button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.black;
		if (!EventSystem.current.alreadySelecting)
			EventSystem.current.SetSelectedGameObject(button);
	}

	public void OnMouseExit(GameObject button) {
		button.GetComponent<Image>().sprite = buttonNormalSprite;
		button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
	}
}
