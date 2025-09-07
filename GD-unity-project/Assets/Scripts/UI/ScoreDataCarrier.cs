using UnityEngine;
using System.Collections.Generic;
using CollectablePapers;
using UnityEngine.Serialization;

public class ScoreDataCarrier : MonoBehaviour
{
    public static ScoreDataCarrier Instance { get; private set; }

    // Data that must persist between scenes
    public Dictionary<GameStatus.LoopIteration, Dictionary<string, int>> EnemiesKilledCount { get; private set; }
    public int PapersCollectedCount { get; private set; }
    public int TotalScore { get; private set; }

    // Basic score references for enemies
    [Header("Enemy Scoring")]
    [SerializeField] private int maynardBaseScoreLoop1 = 50;
    [SerializeField] private int maynardBaseScoreLoop2 = 75;
    [SerializeField] private int maynardBaseScoreLoop3 = 100;

    [SerializeField] private int drakeBaseScoreLoop1 = 100;
    [SerializeField] private int drakeBaseScoreLoop2 = 150;
    [SerializeField] private int drakeBaseScoreLoop3 = 200;

    [SerializeField] private int incognitoBaseScoreLoop1 = 40;
    [SerializeField] private int incognitoBaseScoreLoop2 = 60;
    [SerializeField] private int incognitoBaseScoreLoop3 = 80;

    [Header("Paper Scoring")]
    [Tooltip("Points for each paper collected.")]
    [SerializeField]
    public int paperScore = 100;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Important: Do not destroy this object when changing scenes
            InitializeScoreData();
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Destroy any duplicate instances
        }
    }

    private void InitializeScoreData()
    {
        EnemiesKilledCount = new Dictionary<GameStatus.LoopIteration, Dictionary<string, int>>();
        foreach (GameStatus.LoopIteration iteration in System.Enum.GetValues(typeof(GameStatus.LoopIteration)))
        {
            EnemiesKilledCount[iteration] = new Dictionary<string, int>
            {
                { "Incognito", 0 },
                { "Maynard", 0 },
                { "Drake", 0 }
            };
        }
        PapersCollectedCount = 0;
        TotalScore = 0;
    }

    // Methods for updating data
    public void AddEnemyKill(string enemyName)
    {
        GameStatus.LoopIteration currentLoop = GameStatus.loopIteration;
        if (EnemiesKilledCount.ContainsKey(currentLoop) && EnemiesKilledCount[currentLoop].ContainsKey(enemyName))
        {
            EnemiesKilledCount[currentLoop][enemyName]++;
            CalculateTotalScore();
        }
        else
        {
            Debug.LogWarning($"Enemy type '{enemyName}' not recognized or loop '{currentLoop}' not initialized in ScoreDataCarrier.");
        }
    }

    public void AddPaperCollected()
    {
        // Get the paper count from the PaperManager, which is assumed to be already persistent or manages its data cross-scenes.
        // If PaperManager is not persistent, you will need to pass the paper count to the ScoreDataCarrier before changing scenes.
        PapersCollectedCount = PaperManager.Instance._collectedPapers.Count; 
        CalculateTotalScore();
    }

    // Helper methods for obtaining basic scores
    public int GetMaynardScore(GameStatus.LoopIteration loop)
    {
        switch (loop)
        {
            case GameStatus.LoopIteration.FIRST_ITERATION: return maynardBaseScoreLoop1;
            case GameStatus.LoopIteration.SECOND_ITERATION: return maynardBaseScoreLoop2;
            case GameStatus.LoopIteration.THIRD_ITERATION: return maynardBaseScoreLoop3;
            default: return 0;
        }
    }

    public int GetDrakeScore(GameStatus.LoopIteration loop)
    {
        switch (loop)
        {
            case GameStatus.LoopIteration.FIRST_ITERATION: return drakeBaseScoreLoop1;
            case GameStatus.LoopIteration.SECOND_ITERATION: return drakeBaseScoreLoop2;
            case GameStatus.LoopIteration.THIRD_ITERATION: return drakeBaseScoreLoop3;
            default: return 0;
        }
    }

    public int GetIncognitoScore(GameStatus.LoopIteration loop)
    {
        switch (loop)
        {
            case GameStatus.LoopIteration.FIRST_ITERATION: return incognitoBaseScoreLoop1;
            case GameStatus.LoopIteration.SECOND_ITERATION: return incognitoBaseScoreLoop2;
            case GameStatus.LoopIteration.THIRD_ITERATION: return incognitoBaseScoreLoop3;
            default: return 0;
        }
    }

    public void CalculateTotalScore()
    {
        int currentTotal = 0;

        // Calculate the enemy score
        foreach (var loopEntry in EnemiesKilledCount)
        {
            GameStatus.LoopIteration loop = loopEntry.Key;
            Dictionary<string, int> enemiesInLoop = loopEntry.Value;

            int loopMultiplier = 1; // You can change this logic if the loops have different multipliers
            
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
                    currentTotal += count * enemyScore * loopMultiplier;
                }
            }
        }

        // Calculate the score of the papers
        currentTotal += PapersCollectedCount * paperScore;
        TotalScore = currentTotal;
    }

    // Method to reset all score data
    public void ResetScore()
    {
        InitializeScoreData();
        // Debug.Log("Score data reset.");
    }
}