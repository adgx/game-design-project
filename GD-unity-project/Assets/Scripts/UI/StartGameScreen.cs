using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StartGameScreen : MonoBehaviour
{
    [SerializeField] private GameObject StartGameScreenContainer;
	[SerializeField] private CanvasGroup StartGameScreenCanvas;

	[SerializeField] private GameObject firstSelected;

	[SerializeField] private Sprite buttonHoverSprite;
	[SerializeField] private Sprite buttonNormalSprite;

	bool fadeOut = false;

    public void StartGameButtonClicked() {
        fadeOut = true;
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
		if(fadeOut) {
			if(StartGameScreenCanvas.alpha > 0) {
				StartGameScreenCanvas.alpha -= Time.deltaTime;
				if(StartGameScreenCanvas.alpha <= 0) {
					StartGameScreenCanvas.alpha = 0;
					StartGameScreenContainer.SetActive(false);
					fadeOut = false;
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
}
