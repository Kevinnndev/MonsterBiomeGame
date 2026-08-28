// Contract: GameEndSequenceController orchestrates win/lose end-game flow (timer, score bonus, panel order, timeScale).
// All visual effects (tweens) live in GameEndSequenceFx.
// Events:
//   - OnGameOverTriggered: Raised when game over sequence begins so GameManager centralizes isGameOver = true.
//   - OnGameWinTriggered: Raised when game win sequence begins so GameManager centralizes isGameOver = true.

using System;
using System.Collections;
using UnityEngine;
using MonsterBiome.Core.Models;

public enum GameOverReason
{
    OutOfTime,
    OutOfLife
}

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
    public event Action OnContinueTriggered;

    private GameEndSequenceFx fx;
    private Coroutine gameOverTimeline;
    private GameTheme theme;
    private GameOverReason lastGameOverReason;

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
        fx = new GameEndSequenceFx(darkOverlay, gameOverCanvasGroup);
    }

    public void PlayGameOverSequence(GameOverReason reason, BoardState boardState, LevelBoardView currentBoardView)
    {
        lastGameOverReason = reason;
        OnGameOverTriggered?.Invoke();

        if (timerController != null) timerController.StopTimer();
        if (audioManager != null) audioManager.PlayLose();

        CancelEndSequence();
        fx ??= new GameEndSequenceFx(darkOverlay, gameOverCanvasGroup);
        fx.PlayGameOverEffects();
        if (currentBoardView != null && theme != null) currentBoardView.GrayOutAllMonsters(theme.loseGray);

        gameOverTimeline = StartCoroutine(GameOverTimeline(reason));
    }


    private IEnumerator GameOverTimeline(GameOverReason reason)
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
        ShowGameOverPanel(reason);
    }

    private void ShowGameOverPanel(GameOverReason reason)
    {
        if (uiPanelManager == null) return;

        GameObject targetPanel = reason == GameOverReason.OutOfTime
            ? uiPanelManager.gameOverOutOfTimeUI
            : uiPanelManager.gameOverOutOfLifeUI;

        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
            uiPanelManager.ShowPopupScale(targetPanel);
        }

        uiPanelManager.DimGameUI();

        if (uiPanelManager.restartButton != null) uiPanelManager.restartButton.SetActive(true);
    }


    public void ContinueGame(LevelBoardView currentBoardView)
    {
        CancelEndSequence();

        // Hide the active game over panel
        GameObject activePanel = lastGameOverReason == GameOverReason.OutOfTime
            ? uiPanelManager.gameOverOutOfTimeUI
            : uiPanelManager.gameOverOutOfLifeUI;
        fx.HideGameOverPanel(activePanel);
        fx.HideGameOverEffects();

        if (uiPanelManager != null) uiPanelManager.RestoreGameUI(0.3f);

        if (uiPanelManager?.restartButton != null) uiPanelManager.restartButton.SetActive(false);

        // Restore monster visuals
        if (currentBoardView != null) currentBoardView.RestoreAllMonsters();

        // Apply continue logic based on reason
        if (lastGameOverReason == GameOverReason.OutOfLife)
        {
            livesManager.ResetLives(theme.startingLives);
            timerController.ResumeTimer();
        }
        else // OutOfTime
        {
            timerController.AddTime(60f);
        }

        OnContinueTriggered?.Invoke();
    }


    public void PlayGameWinSequence()
    {
        OnGameWinTriggered?.Invoke();

        timerController.StopTimer();

        int livesLeft = livesManager.Lives;
        scoreManager.AddScore(theme.winBonus + livesLeft * theme.scorePerRemainingLife);

        uiPanelManager.winScreenUI.SetActive(true);
        uiPanelManager.ShowPopupScale(uiPanelManager.winScreenUI);
        uiPanelManager.DimGameUI();

        uiPanelManager.restartButton.SetActive(false);
        uiPanelManager.nextLevelButton.SetActive(true);

        audioManager.PlayWin();
    }
}
