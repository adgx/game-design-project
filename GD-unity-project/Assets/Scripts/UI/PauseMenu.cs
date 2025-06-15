using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
	[SerializeField] private GameObject screenContainer;
	[SerializeField] private GameObject pauseMenu;
	[SerializeField] private GameObject confirmMenu;
	[SerializeField] private VolumeMenu volumeMenuScript;

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
			}

			TogglePauseMenu();
		}
	}

	void TogglePauseMenu() {
		if(screenContainer.activeInHierarchy) {
			screenContainer.SetActive(false);
			pauseMenu.SetActive(false);
			confirmMenu.SetActive(false);
			volumeMenuScript.CloseVolumeMenu();
		}
		else {
			screenContainer.SetActive(true);
			pauseMenu.SetActive(true);
		}
	}

	public void ResumeGameButtonClick() {
		screenContainer.SetActive(false);
		pauseMenu.SetActive(false);
		confirmMenu.SetActive(false);
		volumeMenuScript.CloseVolumeMenu();

		GameStatus.gamePaused = false;
		// Resume the game
		Time.timeScale = 1f;
	}

	public void NewGameButtonClick() {
		// TODO: to be implemented
	}

	public void VolumeSettingsButtonClick() {
		pauseMenu.SetActive(false);
		volumeMenuScript.OpenVolumeMenu();
	}

	public void BackToPauseButtonClick() {
		volumeMenuScript.CloseVolumeMenu();
		pauseMenu.SetActive(true);
	}

	public void QuitGameButtonClick() {
		pauseMenu.SetActive(false);
		confirmMenu.SetActive(true);
	}

	public void YesButtonClick() {
		// TODO: to be implemented
	}

	public void NoButtonClick() {
		confirmMenu.SetActive(false);
		pauseMenu.SetActive(true);
	}
}
