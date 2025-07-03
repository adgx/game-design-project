using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class RespawnScreen : MonoBehaviour
{
    [SerializeField] private GameObject respawnScreenContainer;
	[SerializeField] private CanvasGroup respawnScreenCanvas;

	[SerializeField] private string gameplaySceneName = "Player+Map";

	[SerializeField] private GameObject deathMessageContainer;
	[SerializeField] private GameObject confirmMenu;

	[SerializeField] private TextMeshProUGUI title;
	[SerializeField] private TextMeshProUGUI subtitle;

	[SerializeField] private GameObject firstSelected;
	[SerializeField] private GameObject noButton;

	private bool fadeOut = false, sceneIsLoading = false, changeScene = false;

	private void Start() {
		Destroy(GameObject.Find("RoomManager"));
		EventSystem.current.SetSelectedGameObject(firstSelected);

		if(GameStatus.gameEnded) {
			title.text = "Thanks for playing!";
			subtitle.text = "This demo ends here. If you want, you can play again";
		}
		else {
			title.text = "You died!";
			subtitle.text = "";
		}

		Cursor.lockState = CursorLockMode.None;
	}

	void Update() {
		if(fadeOut && !sceneIsLoading) {
			if(respawnScreenCanvas.alpha > 0) {
				respawnScreenCanvas.alpha -= Time.unscaledDeltaTime;
				if(respawnScreenCanvas.alpha <= 0) {
					respawnScreenCanvas.alpha = 0;
					respawnScreenContainer.SetActive(false);
					fadeOut = false;

					if(changeScene) {
						StartCoroutine(LoadGameplaySceneAsync());
					}
				}
			}
		}
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

		Cursor.lockState = CursorLockMode.Locked;
	}

	void BackToPause() {
		confirmMenu.SetActive(false);

		deathMessageContainer.SetActive(true);
		if(!EventSystem.current.alreadySelecting)
			EventSystem.current.SetSelectedGameObject(firstSelected);
	}

	public void RespawnClicked() {
		fadeOut = true;
		changeScene = true;
		GameStatus.gameEnded = false;
	}

	public void QuitGameClicked() {
		deathMessageContainer.SetActive(false);
		confirmMenu.SetActive(true);
		EventSystem.current.SetSelectedGameObject(noButton);
	}

	public void YesClicked() {
		Application.Quit();
	}

	public void NoClicked() {
		BackToPause();
	}
}
