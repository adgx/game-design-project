using UnityEngine;
using TMPro;
using System.Collections.Generic;
using CollectablePapers;

public class ScoreManagerUI : MonoBehaviour
{
    public static ScoreManagerUI Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI inGameTotalScoreText;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Make sure the ScoreDataCarrier exists
        if (ScoreDataCarrier.Instance == null)
        {
            Debug.LogError("ScoreDataCarrier.Instance not found! Make sure a GameObject with ScoreDataCarrier is present and persistent.");
            return;
        }
        
        // Make sure the in-game score text is visible at the beginning
        if (inGameTotalScoreText != null)
        {
            // Debug.Log("Activating inGameTotaleScoreText");
            inGameTotalScoreText.gameObject.SetActive(true);
        }
        
        UpdateScoreDisplay(); // Update your in-game score at the beginning
    }

    /// <summary>
    /// Called when an enemy is killed.
    /// </summary>
    /// <param name="enemyName">The name of the type of enemy killed (es. "Maynard").</param>
    public void EnemyKilled(string enemyName)
    {
        if (ScoreDataCarrier.Instance != null)
        {
            ScoreDataCarrier.Instance.AddEnemyKill(enemyName);
            UpdateScoreDisplay(); // Update the UI after the kill
            // Debug.Log($"Enemy killed: {enemyName}. Current total score: {ScoreDataCarrier.Instance.TotalScore}");
        }
    }

    /// <summary>
    /// Called when a paper is collected.
    /// </summary>
    public void PaperCollected()
    {
        if (ScoreDataCarrier.Instance != null)
        {
            ScoreDataCarrier.Instance.AddPaperCollected();
            UpdateScoreDisplay(); // Update the UI after collection
            // Debug.Log($"Paper collected. Total count: {ScoreDataCarrier.Instance.PapersCollectedCount}");
        }
    }
    
    private void UpdateScoreDisplay()
    {
        if (inGameTotalScoreText != null && ScoreDataCarrier.Instance != null)
        {
            inGameTotalScoreText.text = $"Score: {ScoreDataCarrier.Instance.TotalScore} PTS";
        }
    }

    public void ResetScore()
    {
        if (ScoreDataCarrier.Instance != null)
        {
            ScoreDataCarrier.Instance.ResetScore();
        }
    }
}