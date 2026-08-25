// Contract: GameEndSequenceController orchestrates win/lose end-game flow (timer, score bonus, panel order, timeScale).
// All visual effects (tweens) live in GameEndSequenceFx.
// Events:
//   - OnGameOverTriggered: Raised when game over sequence begins so GameManager centralizes isGameOver = true.
//   - OnGameWinTriggered: Raised when game win sequence begins so GameManager centralizes isGameOver = true.

using System;
using System.Collections;
using UnityEngine;
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

    private GameEndSequenceFx fx;
    private Coroutine gameOverTimeline;
    private GameTheme theme;

    private const float SlowMoDuration = 0.4f;
    private const float SlowMoScale = 0.3f;
    private const float GameOverPanelDelay = 0.8f;

    public void CancelEndSequence()
    {
        if (gameOverTimeline != null)
        {
            StopCoroutine(gameOverTimeline);
            gameOverTimeline = null;
        }
        fx?.KillActiveEffects();
    }

    public void Initialize(TimerController timer, AudioManager audio, UIPanelManager ui, LivesManager lives, ScoreManager score, GameTheme gameTheme)
    {
        timerController = timer;
        audioManager = audio;
        uiPanelManager = ui;
        livesManager = lives;
        scoreManager = score;
        theme = gameTheme;
        fx = new GameEndSequenceFx(darkOverlay, gameOverCanvasGroup, gameObject);
    }

    public void PlayGameOverSequence(BoardState boardState, LevelBoardView currentBoardView)
    {
        OnGameOverTriggered?.Invoke();

        timerController.StopTimer();
        audioManager.PlayLose();

        CancelEndSequence();
        fx.PlayGameOverEffects();
        if (currentBoardView != null) currentBoardView.GrayOutAllMonsters(theme.loseGray);

        gameOverTimeline = StartCoroutine(GameOverTimeline());
    }

    private IEnumerator GameOverTimeline()
    {
        float elapsed = 0f;
        while (elapsed < SlowMoDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(1f, SlowMoScale, Mathf.Clamp01(elapsed / SlowMoDuration));
            yield return null;
        }
        Time.timeScale = SlowMoScale;

        yield return new WaitForSecondsRealtime(GameOverPanelDelay - SlowMoDuration);

        Time.timeScale = 1f;
        gameOverTimeline = null;
        ShowGameOverPanel();
    }

    public void ShowGameOverPanel()
    {
        fx.ShowGameOverPanel(uiPanelManager.gameOverUI);
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
