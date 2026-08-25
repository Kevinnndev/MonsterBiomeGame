using UnityEngine;
using TMPro;
using DG.Tweening;
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
        scoreText.transform.DOKill(true);
        DOTween.To(() => displayedScore, x => {
            displayedScore = x;
            scoreText.text = "ĐIỂM: " + x.ToString();
        }, newScore, 0.4f).SetEase(Ease.OutQuad).SetTarget(scoreText.transform).SetLink(scoreText.gameObject);
        scoreText.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.3f, 2, 0.5f);
    }
}
