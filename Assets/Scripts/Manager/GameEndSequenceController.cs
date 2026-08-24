// Contract: GameEndSequenceController handles win/lose end-game animations and panel displays.
// Events:
//   - OnGameOverTriggered: Raised when game over sequence begins so GameManager centralizes isGameOver = true.
//   - OnGameWinTriggered: Raised when game win sequence begins so GameManager centralizes isGameOver = true.

using System;
using UnityEngine;
using DG.Tweening;
using MonsterBiome.Core.Models;

public class GameEndSequenceController : MonoBehaviour
{
    [Header("Game Over Effects")]
    public CanvasGroup darkOverlay;
    public CanvasGroup gameOverCanvasGroup;

    [Header("Dependencies")]
    [SerializeField] private TimerController timerController;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private UIPanelManager uiPanelManager;
    [SerializeField] private LivesManager livesManager;
    [SerializeField] private ScoreManager scoreManager;

    public event Action OnGameOverTriggered;
    public event Action OnGameWinTriggered;

    private Sequence activeEndSequence;

    public void CancelEndSequence()
    {
        activeEndSequence?.Kill();
        activeEndSequence = null;
    }

    public void Initialize(TimerController timer, AudioManager audio, UIPanelManager ui, LivesManager lives, ScoreManager score)
    {
        timerController = timer;
        audioManager = audio;
        uiPanelManager = ui;
        livesManager = lives;
        scoreManager = score;
    }

    public void PlayGameOverSequence(BoardState boardState, LevelBoardView currentBoardView)
    {
        OnGameOverTriggered?.Invoke();

        timerController.StopTimer();
        audioManager.PlayLose();

        CancelEndSequence();
        Sequence gameOverSeq = activeEndSequence = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

        if (Camera.main != null)
        {
            gameOverSeq.Insert(0, Camera.main.transform.DOShakePosition(0.3f, strength: 0.15f, vibrato: 10).SetUpdate(true));
        }

        if (darkOverlay != null)
        {
            darkOverlay.gameObject.SetActive(true);
            darkOverlay.alpha = 0f;
            gameOverSeq.Insert(0, darkOverlay.DOFade(0.5f, 0.6f).SetEase(Ease.OutQuad).SetUpdate(true));
        }

        gameOverSeq.Insert(0, DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0.3f, 0.4f).SetUpdate(true));

        if (boardState != null && currentBoardView != null)
        {
            for (int r = 0; r < boardState.Rows; r++)
            {
                for (int c = 0; c < boardState.Cols; c++)
                {
                    if (boardState.IsPlacedMonster(r, c))
                    {
                        BoardCell cell = currentBoardView.GetCell(r, c, boardState.Cols);
                        if (cell != null && cell.monsterSprite != null)
                        {
                            gameOverSeq.Insert(0, cell.monsterSprite.DOColor(Color.gray, 0.4f).SetUpdate(true));
                        }
                    }
                }
            }
        }

        gameOverSeq.InsertCallback(0.8f, () => {
            Time.timeScale = 1f;
            ShowGameOverPanel();
        });
    }

    public void ShowGameOverPanel()
    {
        uiPanelManager.gameOverUI.SetActive(true);
        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.alpha = 0f;
            uiPanelManager.gameOverUI.transform.localScale = Vector3.one * 1.1f;
            gameOverCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.OutQuad).SetUpdate(true).SetLink(gameObject);
            uiPanelManager.gameOverUI.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutQuad).SetUpdate(true).SetLink(gameObject);
        }

        uiPanelManager.restartButton.SetActive(true);
    }

    public void PlayGameWinSequence()
    {
        OnGameWinTriggered?.Invoke();

        timerController.StopTimer();

        int livesLeft = livesManager.Lives;
        scoreManager.AddScore(500 + (livesLeft * 100));

        uiPanelManager.winScreenUI.SetActive(true);
        uiPanelManager.ShowPopupScale(uiPanelManager.winScreenUI);

        uiPanelManager.restartButton.SetActive(false);
        uiPanelManager.nextLevelButton.SetActive(true);

        audioManager.PlayWin();
    }
}
