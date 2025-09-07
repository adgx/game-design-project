using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class RespawnScreen : MonoBehaviour
{
    [SerializeField] private GameObject respawnScreenContainer;
	[SerializeField] private CanvasGroup respawnScreenCanvas;

	[SerializeField] private string gameplaySceneName = "Player+Map";
	[SerializeField] private GameObject confirmMenu;

	[SerializeField] private GameObject GameEndMessageContainer;
	[SerializeField] private GameObject DiedMessageContainer;

	[SerializeField] private GameObject firstSelected;
	[SerializeField] private GameObject noButton;
	
	[Header("Score Display UI (Respawn Screen)")]
	[SerializeField] private TextMeshProUGUI respawnEnemiesKilledText;
	[SerializeField] private TextMeshProUGUI respawnPapersCollectedText;
	[SerializeField] private TextMeshProUGUI respawnTotalScoreText;
	[SerializeField] private GameObject respawnScorePanel;

	private bool fadeOut = false, sceneIsLoading = false, changeScene = false;

	private void Start() {
		Destroy(GameObject.Find("RoomManager"));
		EventSystem.current.SetSelectedGameObject(firstSelected);

		if(GameStatus.gameEnded) {
			GameEndMessageContainer.SetActive(true);
			DiedMessageContainer.SetActive(false);
		}
		else {
			GameEndMessageContainer.SetActive(false);
			DiedMessageContainer.SetActive(true);
		}
		
		DisplayRespawnScore();

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
		if (GameStatus.gameEnded)
		{
			GameEndMessageContainer.SetActive(true);
		}
		else
		{
			DiedMessageContainer.SetActive(true);
		}
		if(!EventSystem.current.alreadySelecting)
			EventSystem.current.SetSelectedGameObject(firstSelected);
	}

	public void RespawnClicked() {
		fadeOut = true;
		changeScene = true;
		GameStatus.gameEnded = false;
		ScoreManagerUI.Instance.ResetScore();
		
		// Ambient light management
		AmbientLightManager.ResetLightSequence();
	}

	public void QuitGameClicked() {
		if (GameStatus.gameEnded)
		{
			GameEndMessageContainer.SetActive(false);
		}
		else
		{
			DiedMessageContainer.SetActive(false);
		}
		confirmMenu.SetActive(true);
		EventSystem.current.SetSelectedGameObject(noButton);
	}

	public void YesClicked() {
		Application.Quit();
	}

	public void NoClicked() {
		BackToPause();
	}
	
	// Function to display the score on the respawn screen
	private void DisplayRespawnScore()
	{
	    if (ScoreDataCarrier.Instance == null)
	    {
	        Debug.LogError("ScoreDataCarrier.Instance not found! Cannot display score on respawn screen.");
	        return;
	    }

	    ScoreDataCarrier.Instance.CalculateTotalScore(); // Make sure the total is up to date

	    string enemiesText = "";
	    
	    var orderedLoops = 
		    ScoreDataCarrier.Instance.EnemiesKilledCount.OrderBy(entry => entry.Key);

	    foreach (var loopEntry in orderedLoops)
	    {
	        GameStatus.LoopIteration loop = loopEntry.Key;
	        Dictionary<string, int> enemiesInLoop = loopEntry.Value;

	        // Add loop header
	        enemiesText += $"Loop {((int)loop) + 1}\n";

	        int loopMultiplier = 1; 
	        
	        // Loop multiplier logic
	        // switch (loop)
	        // {
	        //     case GameStatus.LoopIteration.FIRST_ITERATION:
	        //         loopMultiplier = 1;
	        //         break;
	        //     case GameStatus.LoopIteration.SECOND_ITERATION:
	        //         loopMultiplier = 1;
	        //         break;
	        //     case GameStatus.LoopIteration.THIRD_ITERATION:
	        //         loopMultiplier = 1;
	        //         break;
	        // }

	        foreach (var enemyEntry in enemiesInLoop)
	        {
	            string enemyName = enemyEntry.Key;
	            int count = enemyEntry.Value;
	            int enemyScore = 0;

	            switch (enemyName)
	            {
	                case "Maynard":
	                    enemyScore = ScoreDataCarrier.Instance.GetMaynardScore(loop);
	                    break;
	                case "Drake":
	                    enemyScore = ScoreDataCarrier.Instance.GetDrakeScore(loop);
	                    break;
	                case "Incognito":
	                    enemyScore = ScoreDataCarrier.Instance.GetIncognitoScore(loop);
	                    break;
	            }

	            int scoreForEnemyType = count * enemyScore * loopMultiplier;
	            enemiesText += $"\t{enemyName}s: {count} x {enemyScore} PTS = {scoreForEnemyType} PTS\n";
	        }
	        enemiesText += "\n";
	    }
	    
	    if (respawnEnemiesKilledText != null) respawnEnemiesKilledText.text = enemiesText;
	    Debug.Log("respawnEnemiesKilledText: " + enemiesText);

	    int papersScoreTotal = ScoreDataCarrier.Instance.PapersCollectedCount * ScoreDataCarrier.Instance.paperScore;
	    if (respawnPapersCollectedText != null) respawnPapersCollectedText.text = $"Papers Collected: " +
		    $"{ScoreDataCarrier.Instance.PapersCollectedCount} x {ScoreDataCarrier.Instance.paperScore} PTS = " +
		    $"{papersScoreTotal} PTS\n";

	    if (respawnTotalScoreText != null) respawnTotalScoreText.text = $"Total Score: {ScoreDataCarrier.Instance.TotalScore} PTS";
	    
	    if (respawnScorePanel != null) respawnScorePanel.SetActive(true); // Show the final score panel
	}
}