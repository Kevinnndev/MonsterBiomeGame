using UnityEngine;
using TMPro;
using MonsterBiome.Core.Models;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Display")]
    public TextMeshProUGUI scoreText;

    private readonly ScoreCore model = new ScoreCore();
    private int displayedScore = 0;

    private void Awake()
    {
        model.OnScoreChanged += HandleScoreChanged;
    }

    private void OnDestroy()
    {
        model.OnScoreChanged -= HandleScoreChanged;
    }

    public void ResetScore()
    {
        displayedScore = 0;
        model.ResetScore();
    }

    public void AddScore(int amount)
    {
        model.AddScore(amount);
    }

    private void HandleScoreChanged(int newScore)
    {
        if (scoreText == null) return;
        Animations.Current.Kill(scoreText.transform, complete: true);
        float from = displayedScore;
        Animations.Current.ValueTo(scoreText.transform, from, newScore, 0.4f, x => {
            displayedScore = Mathf.RoundToInt(x);
            scoreText.text = "ĐIỂM: " + displayedScore.ToString();
        });
        Animations.Current.PunchScale(scoreText.transform, new Vector3(0.1f, 0.1f, 0f), 0.3f);
    }
}
