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

	private bool pauseScreenOpen = false;

	private void Start() {
		playerInput = Player.Instance.GetComponent<PlayerInput>();

		pauseScreenOpen = false;
		screenContainer.SetActive(false);
		pauseMenu.SetActive(false);
		confirmMenu.SetActive(false);
		volumeMenuScript.CloseVolumeMenu();
	}

	// Update is called once per frame
	void Update()
    {
		if(playerInput.PausePressed() || (pauseScreenOpen && pauseMenu.activeInHierarchy && playerInput.BackKeyPressed())) {
			ChangeGameState(!GameStatus.gamePaused);

			TogglePauseMenu();
		}

		if(pauseScreenOpen && !pauseMenu.activeInHierarchy && playerInput.BackKeyPressed()) {
			BackToPause();
		}
	}

	async void ChangeGameState(bool paused) {
		if(paused) {
			// Setting timeScale to 0 pauses the game
			Time.timeScale = 0f;
		}
		else {
			// Resume the game
			Time.timeScale = 1f;
			await Task.Delay(100);
			EventSystem.current.SetSelectedGameObject(null);
		}

		GameStatus.gamePaused = paused;
	}

	void TogglePauseMenu() {
		if(screenContainer.activeInHierarchy) {
			pauseScreenOpen = false;
			screenContainer.SetActive(false);
			foreach(Transform child in screenContainer.transform) {
				child.gameObject.SetActive(false);
			}
		}
		else {
			pauseScreenOpen = true;
			screenContainer.SetActive(true);
			pauseMenu.SetActive(true);
			EventSystem.current.SetSelectedGameObject(firstSelected);
		}
	}

	void BackToPause() {
		volumeMenuScript.CloseVolumeMenu();
		confirmMenu.SetActive(false);

		pauseMenu.SetActive(true);
		if(!EventSystem.current.alreadySelecting)
			EventSystem.current.SetSelectedGameObject(firstSelected);
	}

	async public void ResumeGameButtonClick(GameObject button) {
		button.GetComponent<Image>().sprite = buttonNormalSprite;
		button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;

		TogglePauseMenu();

		ChangeGameState(false);
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
		BackToPause();
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

		BackToPause();
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
