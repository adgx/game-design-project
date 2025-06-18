using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
	[SerializeField] private GameObject screenContainer;
	[SerializeField] private GameObject pauseMenu;
	[SerializeField] private GameObject confirmMenu;
	[SerializeField] private VolumeMenu volumeMenuScript;

	[SerializeField] private GameObject firstSelected;
	[SerializeField] private GameObject noButton;

	[SerializeField] private Sprite buttonHoverSprite;
	[SerializeField] private Sprite buttonNormalSprite;

	private PlayerInput playerInput;

	private void Start() {
		playerInput = Player.Instance.GetComponent<PlayerInput>();

		screenContainer.SetActive(false);
		pauseMenu.SetActive(false);
		confirmMenu.SetActive(false);
		volumeMenuScript.CloseVolumeMenu();
	}

	// Update is called once per frame
	void Update()
    {
		if(playerInput.PausePressed()) {
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

			TogglePauseMenu();
		}
	}

	void TogglePauseMenu() {
		if(screenContainer.activeInHierarchy) {
			screenContainer.SetActive(false);
			foreach(Transform child in screenContainer.transform) {
				child.gameObject.SetActive(false);
			}
		}
		else {
			screenContainer.SetActive(true);
			pauseMenu.SetActive(true);
			EventSystem.current.SetSelectedGameObject(firstSelected);
		}
	}

	async public void ResumeGameButtonClick(GameObject button) {
		button.GetComponent<Image>().sprite = buttonNormalSprite;
		button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;

		screenContainer.SetActive(false);
		pauseMenu.SetActive(false);
		confirmMenu.SetActive(false);
		volumeMenuScript.CloseVolumeMenu();

		await Task.Delay(100);
		EventSystem.current.SetSelectedGameObject(null);
		GameStatus.gamePaused = false;
		// Resume the game
		Time.timeScale = 1f;
	}

	public void NewGameButtonClick(GameObject button) {
		button.GetComponent<Image>().sprite = buttonNormalSprite;
		button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;

		// TODO: to be implemented
	}

	public void VolumeSettingsButtonClick(GameObject button) {
		button.GetComponent<Image>().sprite = buttonNormalSprite;
		button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;

		pauseMenu.SetActive(false);
		volumeMenuScript.OpenVolumeMenu();
	}

	public void BackToPauseButtonClick() {
		volumeMenuScript.CloseVolumeMenu();
		pauseMenu.SetActive(true);
		if (!EventSystem.current.alreadySelecting)
			EventSystem.current.SetSelectedGameObject(firstSelected);
	}

	public void QuitGameButtonClick(GameObject button) {
		button.GetComponent<Image>().sprite = buttonNormalSprite;
		button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;

		pauseMenu.SetActive(false);
		confirmMenu.SetActive(true);
		EventSystem.current.SetSelectedGameObject(noButton);
	}

	public void YesButtonClick(GameObject button) {
		button.GetComponent<Image>().sprite = buttonNormalSprite;
		button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;

		// TODO: to be implemented
	}

	public void NoButtonClick(GameObject button) {
		button.GetComponent<Image>().sprite = buttonNormalSprite;
		button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;

		confirmMenu.SetActive(false);
		pauseMenu.SetActive(true);
		if (!EventSystem.current.alreadySelecting)
			EventSystem.current.SetSelectedGameObject(firstSelected);
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
