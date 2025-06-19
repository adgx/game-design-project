using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class StartGameScreen : MonoBehaviour
{
    [SerializeField] private GameObject StartGameScreenContainer;
	[SerializeField] private CanvasGroup StartGameScreenCanvas;

	[SerializeField] private GameObject firstSelected;

	[SerializeField] private Sprite buttonHoverSprite;
	[SerializeField] private Sprite buttonNormalSprite;

	bool fadeOut = false;
	bool changeScene = false;
	
	[SerializeField] private string gameplaySceneName = "Player+Map";
	private bool sceneIsLoading = false;
	
    public void StartGameButtonClicked() {
        fadeOut = true;
        changeScene = true;
        GameStatus.gamePaused = false;
		GameStatus.gameStarted = true;
        Time.timeScale = 1f;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		StartGameScreenContainer.SetActive(true);
		StartGameScreenCanvas.alpha = 1f;
		EventSystem.current.SetSelectedGameObject(firstSelected);
		GameStatus.gamePaused = true;
		GameStatus.gameStarted = false;
        Time.timeScale = 0f;
    }

    // Update is called once per frame
    void Update()
    {
	    if (fadeOut && !sceneIsLoading) {
		    if (StartGameScreenCanvas.alpha > 0) {
			    StartGameScreenCanvas.alpha -= Time.unscaledDeltaTime;
			    if (StartGameScreenCanvas.alpha <= 0) {
				    StartGameScreenCanvas.alpha = 0;
				    StartGameScreenContainer.SetActive(false);
				    fadeOut = false;

				    if (changeScene) {
					    StartCoroutine(LoadGameplaySceneAsync());
				    }
			    }
		    }
	    }
    }

	public void OnMouseEnter(GameObject button) {
		button.GetComponent<Image>().sprite = buttonHoverSprite;
		button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.black;
		if(!EventSystem.current.alreadySelecting)
			EventSystem.current.SetSelectedGameObject(button);
	}

	public void OnMouseExit(GameObject button) {
		button.GetComponent<Image>().sprite = buttonNormalSprite;
		button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
	}
	
	private IEnumerator LoadGameplaySceneAsync()
	{
		sceneIsLoading = true;

		// Inizia il caricamento asincrono della scena
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameplaySceneName);
		asyncLoad.allowSceneActivation = false;

		// Attendi finché la scena è quasi pronta (>= 0.9)
		while (asyncLoad.progress < 0.9f)
		{
			yield return null;
		}

		// (Qui potresti visualizzare un "Loading..." o una schermata nera, se vuoi)
		// TODO: inserire la schermata di caricamento di Paolone

		// Ora attiva effettivamente la scena
		asyncLoad.allowSceneActivation = true;
	}

}
