using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[System.Serializable]
public class ScorePanelUI
{
    public TextMeshProUGUI enemiesKilledText;
    public TextMeshProUGUI papersCollectedText;
    public TextMeshProUGUI totalScoreText;
    public GameObject scorePanelContainer;

    public void DisplayScore(ScoreDataCarrier scoreData)
    {
        if (scoreData == null)
        {
            Debug.LogError("ScoreDataCarrier è nullo. Impossibile visualizzare il punteggio.");
            return;
        }

        scoreData.CalculateTotalScore(); // Make sure the total is up to date

        string enemiesText = "";
        var orderedLoops = scoreData.EnemiesKilledCount.OrderBy(entry => entry.Key);

        foreach (var loopEntry in orderedLoops)
        {
            GameStatus.LoopIteration loop = loopEntry.Key;
            Dictionary<string, int> enemiesInLoop = loopEntry.Value;

            enemiesText += $"Loop {((int)loop) + 1}\n";

            int loopMultiplier = 1; // Loop multiplier logic can be added here if necessary

            foreach (var enemyEntry in enemiesInLoop)
            {
                string enemyName = enemyEntry.Key;
                int count = enemyEntry.Value;
                int enemyScore = 0;

                switch (enemyName)
                {
                    case "Maynard":
                        enemyScore = scoreData.GetMaynardScore(loop);
                        break;
                    case "Drake":
                        enemyScore = scoreData.GetDrakeScore(loop);
                        break;
                    case "Incognito":
                        enemyScore = scoreData.GetIncognitoScore(loop);
                        break;
                }

                int scoreForEnemyType = count * enemyScore * loopMultiplier;
                enemiesText += $"\t{enemyName}s: {count} x {enemyScore} PTS = {scoreForEnemyType} PTS\n";
            }
            enemiesText += "\n";
        }

        if (enemiesKilledText != null) enemiesKilledText.text = enemiesText;
        else { Debug.LogWarning("enemiesKilledText è nullo per questo pannello di punteggio."); }

        int papersScoreTotal = scoreData.PapersCollectedCount * scoreData.paperScore;
        if (papersCollectedText != null) papersCollectedText.text = $"Papers Collected: " +
            $"{scoreData.PapersCollectedCount} x {scoreData.paperScore} PTS = " +
            $"{papersScoreTotal} PTS\n";
        else { Debug.LogWarning("papersCollectedText è nullo per questo pannello di punteggio."); }

        if (totalScoreText != null) totalScoreText.text = $"Total Score: {scoreData.TotalScore} PTS";
        else { Debug.LogWarning("totalScoreText è nullo per questo pannello di punteggio."); }

        if (scorePanelContainer != null) scorePanelContainer.SetActive(true); // Show score panel
        else { Debug.LogWarning("scorePanelContainer è nullo per questo pannello di punteggio."); }
    }
}