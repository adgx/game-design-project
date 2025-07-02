using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
	private enum ActionToConfirm {
		QuitGame,
		StartNewGame
	}
	private ActionToConfirm actionToConfirm;

	[SerializeField] private GameObject screenContainer;
	[SerializeField] private GameObject pauseMenu;
	[SerializeField] private GameObject confirmMenu;
	[SerializeField] private VolumeMenu volumeMenuScript;

	[SerializeField] private TextMeshProUGUI confirmMenuText;

	[SerializeField] private GameObject firstSelected;
	[SerializeField] private GameObject noButton;

	[SerializeField] private ButtonEffects buttonEffects;

	private PlayerInput playerInput;

	private bool pauseScreenOpen = false;

	[SerializeField] private string gameplaySceneName = "Player+Map";
	private bool sceneIsLoading = false;

	private void Start() {
		playerInput = Player.Instance.GetComponent<PlayerInput>();

		pauseScreenOpen = false;
		screenContainer.SetActive(false);
		pauseMenu.SetActive(false);
		confirmMenu.SetActive(false);
		volumeMenuScript.CloseVolumeMenu();

		actionToConfirm = ActionToConfirm.QuitGame;
	}

	// Update is called once per frame
	void Update()
    {
		if(!sceneIsLoading) {
			if(playerInput.PausePressed() || (pauseScreenOpen && pauseMenu.activeInHierarchy && playerInput.BackKeyPressed())) {
				ChangeGameState(!GameStatus.gamePaused);

				TogglePauseMenu();
			}

			if(pauseScreenOpen && !pauseMenu.activeInHierarchy && playerInput.BackKeyPressed()) {
				BackToPause();
			}
		}
	}

	async void ChangeGameState(bool paused) {
		if(paused) {
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

	public void ResumeGameButtonClick(GameObject button) {
		buttonEffects.OnMouseExit(button);

		TogglePauseMenu();

		ChangeGameState(false);
	}

	public void NewGameButtonClick(GameObject button) {
		buttonEffects.OnMouseExit(button);

		actionToConfirm = ActionToConfirm.StartNewGame;
		OpenConfirmMenu();
	}

	public void VolumeSettingsButtonClick(GameObject button) {
		buttonEffects.OnMouseExit(button);

		pauseMenu.SetActive(false);
		volumeMenuScript.OpenVolumeMenu();
	}

	public void BackToPauseButtonClick() {
		BackToPause();
	}

	public void QuitGameButtonClick(GameObject button) {
		buttonEffects.OnMouseExit(button);

		actionToConfirm = ActionToConfirm.QuitGame;
		OpenConfirmMenu();
	}

	private void OpenConfirmMenu() {
		if(actionToConfirm == ActionToConfirm.QuitGame) {
			confirmMenuText.text = "Are you sure you want to quit?";
		}
		else {
			confirmMenuText.text = "Are you sure you want to start a new game?";
		}

		pauseMenu.SetActive(false);
		confirmMenu.SetActive(true);
		EventSystem.current.SetSelectedGameObject(noButton);
	}

	public void YesButtonClick(GameObject button) {
		buttonEffects.OnMouseExit(button);

		if(actionToConfirm == ActionToConfirm.StartNewGame) {
			Destroy(GameObject.Find("RoomManager"));
			StartCoroutine(LoadGameplaySceneAsync());
		}
		else {
			// This only works with the compiled game (.exe)
			Application.Quit();
		}
	}

	public void NoButtonClick(GameObject button) {
		buttonEffects.OnMouseExit(button);

		BackToPause();
	}

	private IEnumerator LoadGameplaySceneAsync() {
		sceneIsLoading = true;

		// Starts async loading of the scene
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameplaySceneName);
		asyncLoad.allowSceneActivation = false;

		// Wait until the scene is almost ready (>= 0.9)
		while(asyncLoad.progress < 0.9f) {
			yield return null;
		}

		// Activate the scene
		asyncLoad.allowSceneActivation = true;

		ChangeGameState(false);
	}
}
