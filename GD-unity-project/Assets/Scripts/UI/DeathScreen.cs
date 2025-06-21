using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
    [SerializeField] private GameObject deathScreenContainer;
	[SerializeField] private CanvasGroup deathScreenCanvas;

	[SerializeField] private string gameplaySceneName = "Player+Map";

	[SerializeField] private GameObject deathMessageContainer;
	[SerializeField] private GameObject confirmMenu;

	[SerializeField] private GameObject firstSelected;
	[SerializeField] private GameObject noButton;

	private bool fadeOut = false, sceneIsLoading = false, changeScene = false;

	private void Start() {
		Destroy(GameObject.Find("RoomManager"));
		EventSystem.current.SetSelectedGameObject(firstSelected);
	}

	void Update() {
		if(fadeOut && !sceneIsLoading) {
			if(deathScreenCanvas.alpha > 0) {
				deathScreenCanvas.alpha -= Time.unscaledDeltaTime;
				if(deathScreenCanvas.alpha <= 0) {
					deathScreenCanvas.alpha = 0;
					deathScreenContainer.SetActive(false);
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

		// Inizia il caricamento asincrono della scena
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameplaySceneName);
		asyncLoad.allowSceneActivation = false;

		// Attendi finché la scena è quasi pronta (>= 0.9)
		while(asyncLoad.progress < 0.9f) {
			yield return null;
		}

		// Ora attiva effettivamente la scena
		asyncLoad.allowSceneActivation = true;
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
	}

	public void QuitGameClicked() {
		deathMessageContainer.SetActive(false);
		confirmMenu.SetActive(true);
		EventSystem.current.SetSelectedGameObject(noButton);
	}

	public void YesClicked() {
		// TODO: to be implemented
	}

	public void NoClicked() {
		BackToPause();
	}
}
