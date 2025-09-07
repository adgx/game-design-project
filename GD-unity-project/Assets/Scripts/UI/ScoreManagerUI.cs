using UnityEngine;
using TMPro;
using System.Collections.Generic;
using CollectablePapers;

public class ScoreManagerUI : MonoBehaviour
{
    public static ScoreManagerUI Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private TextMeshProUGUI inGameTotalScoreText;
    [SerializeField] private TextMeshProUGUI enemiesKilledText;
    [SerializeField] private TextMeshProUGUI papersCollectedText;
    [SerializeField] private TextMeshProUGUI totalScoreText;

    [Header("Enemy Scoring")]
    [Tooltip("Maynard kill basis points for loop iteration.")]
    [SerializeField] private int maynardBaseScoreLoop1 = 50;
    [SerializeField] private int maynardBaseScoreLoop2 = 75;
    [SerializeField] private int maynardBaseScoreLoop3 = 100;

    [Tooltip("Drake kill basis points for loop iteration. (Example, suitable for your enemies)")]
    [SerializeField] private int drakeBaseScoreLoop1 = 100;
    [SerializeField] private int drakeBaseScoreLoop2 = 150;
    [SerializeField] private int drakeBaseScoreLoop3 = 200;

    [Tooltip("Incognito kill basis points (other enemy types) for loop iteration.")]
    [SerializeField] private int IncognitoBaseScoreLoop1 = 40;
    [SerializeField] private int IncognitoBaseScoreLoop2 = 60;
    [SerializeField] private int IncognitoBaseScoreLoop3 = 80;


    [Header("Paper Scoring")]
    [Tooltip("Points for each paper collected.")]
    [SerializeField] private int paperScore = 100;

    // Dictionaries to track enemies killed by type and by loop iteration
    private Dictionary<GameStatus.LoopIteration, Dictionary<string, int>> _enemiesKilledCount;
    private int _papersCollectedCount;
    private int _totalScore;
    
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

        _enemiesKilledCount = new Dictionary<GameStatus.LoopIteration, Dictionary<string, int>>();
        InitializeEnemyCountsForLoops();
    }
    
    private void Start()
    {
        // Make sure the in-game score text is visible at the beginning
        if (inGameTotalScoreText != null)
        {
            Debug.Log("Activating inGameTotaleScoreText");
            inGameTotalScoreText.gameObject.SetActive(true);
        }
        
        // scorePanel.SetActive(false); // Hides the endgame score panel
        UpdateScoreDisplay(); // Update your in-game score at the beginning
    }

    private void InitializeEnemyCountsForLoops()
    {
        foreach (GameStatus.LoopIteration iteration in System.Enum.GetValues(typeof(GameStatus.LoopIteration)))
        {
            _enemiesKilledCount[iteration] = new Dictionary<string, int>();
            
            // Initialize counters for your enemy types
            _enemiesKilledCount[iteration]["Incognito"] = 0;
            _enemiesKilledCount[iteration]["Maynard"] = 0;
            _enemiesKilledCount[iteration]["Drake"] = 0;
        }
    }

    /// <summary>
    /// Called when an enemy is killed.
    /// </summary>
    /// <param name="enemyName">The name of the type of enemy killed (es. "Maynard").</param>
    public void EnemyKilled(string enemyName)
    {
        GameStatus.LoopIteration currentLoop = GameStatus.loopIteration;

        if (_enemiesKilledCount.ContainsKey(currentLoop) && _enemiesKilledCount[currentLoop].ContainsKey(enemyName))
        {
            _enemiesKilledCount[currentLoop][enemyName]++;
            Debug.Log($"Enemy killed: {enemyName}. Current score for {currentLoop}: {_enemiesKilledCount[currentLoop][enemyName]}");
            CalculateTotalScore(); // Recalculate the total score
            UpdateScoreDisplay(); // Update the in-game score UI
        }
        else
        {
            Debug.LogWarning($"Enemy type '{enemyName}' not recognized or loop '{currentLoop}' not initialized in the scoring system.");
        }
    }

    /// <summary>
    /// Called when a paper is collected.
    /// </summary>
    public void PaperCollected()
    {
        _papersCollectedCount = PaperManager.Instance._collectedPapers.Count; // Get the total number of papers collected
        Debug.Log($"Paper collected. Total count: {_papersCollectedCount}");
        CalculateTotalScore(); // Recalculate the total score
        UpdateScoreDisplay(); // Update the in-game score UI
    }

    /// <summary>
    /// compute and show the final score.
    /// </summary>
    public void ShowFinalScore()
    {
        CalculateTotalScore(); // Make sure the total score is up to date
        
        _totalScore = 0;
        string enemiesText = "";

        // Re-compute the text for killed enemies to display on the final screen
        foreach (var loopEntry in _enemiesKilledCount)
        {
            GameStatus.LoopIteration loop = loopEntry.Key;
            Dictionary<string, int> enemiesInLoop = loopEntry.Value;

            int loopMultiplier = 1; // Base multiplier, may vary if the score per loop increases
            // Example: Loop 2 enemies give more points, Loop 3 enemies give even more.
            switch (loop)
            {
                case GameStatus.LoopIteration.FIRST_ITERATION:
                    loopMultiplier = 1;
                    break;
                case GameStatus.LoopIteration.SECOND_ITERATION:
                    loopMultiplier = 1;
                    break;
                case GameStatus.LoopIteration.THIRD_ITERATION:
                    loopMultiplier = 1;
                    break;
            }

            foreach (var enemyEntry in enemiesInLoop)
            {
                string enemyName = enemyEntry.Key;
                int count = enemyEntry.Value;
                int enemyScore = 0;

                if (count > 0) // Show only enemies who have been killed
                {
                    switch (enemyName)
                    {
                        case "Maynard":
                            enemyScore = GetMaynardScore(loop);
                            break;
                        case "Drake":
                            enemyScore = GetDrakeScore(loop);
                            break;
                        case "Incognito":
                            enemyScore = GetIncognitoScore(loop);
                            break;
                        // Add more cases for your enemies
                    }

                    int scoreForEnemyType = count * enemyScore * loopMultiplier;
                    _totalScore += scoreForEnemyType;
                    enemiesText += $"Loop {((int)loop) + 1} {enemyName}s: {count} x {enemyScore} PTS = {scoreForEnemyType} PTS\n";
                }
            }
        }
        enemiesKilledText.text = enemiesText;

        // Populates the text of the papers
        int papersScoreTotal = _papersCollectedCount * paperScore;
        papersCollectedText.text = $"Papers Collected: {_papersCollectedCount} x {paperScore} PTS = {papersScoreTotal} PTS\n";

        // Populate the text of the total score
        totalScoreText.text = $"Total Score: {_totalScore} PTS";
        scorePanel.SetActive(true); // Show endgame score panel
    }

    // Helper methods to get enemy-specific scores for iteration
    private int GetMaynardScore(GameStatus.LoopIteration loop)
    {
        switch (loop)
        {
            case GameStatus.LoopIteration.FIRST_ITERATION: return maynardBaseScoreLoop1;
            case GameStatus.LoopIteration.SECOND_ITERATION: return maynardBaseScoreLoop2;
            case GameStatus.LoopIteration.THIRD_ITERATION: return maynardBaseScoreLoop3;
            default: return 0;
        }
    }

    private int GetDrakeScore(GameStatus.LoopIteration loop)
    {
        switch (loop)
        {
            case GameStatus.LoopIteration.FIRST_ITERATION: return drakeBaseScoreLoop1;
            case GameStatus.LoopIteration.SECOND_ITERATION: return drakeBaseScoreLoop2;
            case GameStatus.LoopIteration.THIRD_ITERATION: return drakeBaseScoreLoop3;
            default: return 0;
        }
    }

    private int GetIncognitoScore(GameStatus.LoopIteration loop)
    {
        switch (loop)
        {
            case GameStatus.LoopIteration.FIRST_ITERATION: return IncognitoBaseScoreLoop1;
            case GameStatus.LoopIteration.SECOND_ITERATION: return IncognitoBaseScoreLoop2;
            case GameStatus.LoopIteration.THIRD_ITERATION: return IncognitoBaseScoreLoop3;
            default: return 0;
        }
    }
    
    private void CalculateTotalScore()
    {
        _totalScore = 0;

        // compute the enemy score
        foreach (var loopEntry in _enemiesKilledCount)
        {
            GameStatus.LoopIteration loop = loopEntry.Key;
            Dictionary<string, int> enemiesInLoop = loopEntry.Value;

            int loopMultiplier = 1; 
            switch (loop)
            {
                case GameStatus.LoopIteration.FIRST_ITERATION:
                    loopMultiplier = 1;
                    break;
                case GameStatus.LoopIteration.SECOND_ITERATION:
                    loopMultiplier = 1;
                    break;
                case GameStatus.LoopIteration.THIRD_ITERATION:
                    loopMultiplier = 1;
                    break;
            }

            foreach (var enemyEntry in enemiesInLoop)
            {
                string enemyName = enemyEntry.Key;
                int count = enemyEntry.Value;
                int enemyScore = 0;

                if (count > 0)
                {
                    switch (enemyName)
                    {
                        case "Maynard":
                            enemyScore = GetMaynardScore(loop);
                            break;
                        case "Drake":
                            enemyScore = GetDrakeScore(loop);
                            break;
                        case "Incognito":
                            enemyScore = GetIncognitoScore(loop);
                            break;
                    }

                    int scoreForEnemyType = count * enemyScore * loopMultiplier;
                    _totalScore += scoreForEnemyType;
                }
            }
        }

        // compute the score of the papers
        _papersCollectedCount = PaperManager.Instance._collectedPapers.Count;
        int papersScoreTotal = _papersCollectedCount * paperScore;
        _totalScore += papersScoreTotal;
    }
    
    private void UpdateScoreDisplay()
    {
        if (inGameTotalScoreText != null)
        {
            inGameTotalScoreText.text = $"Score: {_totalScore} PTS";
        }
    }
}