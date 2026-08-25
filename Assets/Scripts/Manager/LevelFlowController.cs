// Contract: LevelFlowController manages level lifecycle (Load, Clear, Next, Restart, Exit).
// Events:
//   - OnLevelLoadedSuccessfully: Raised whenever a level is successfully loaded so GameManager resets isGameOver = false.
//   - OnReturnToMainMenu: Raised when exiting to the main menu.

using System;
using UnityEngine;
using MonsterBiome.Core.Models;

public class LevelFlowController : MonoBehaviour
{
    [Header("Level Configuration")]
    public int currentLevel { get; private set; } = 0;

    [Header("Dependencies")]
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private LivesManager livesManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private UIPanelManager uiPanelManager;
    [SerializeField] private BoosterController boosterController;
    [SerializeField] private TimerController timerController;
    [SerializeField] private AudioManager audioManager;

    private BoardState boardState;
    private LevelBoardView currentBoardView;
    private GameObject currentBoardInstance;
    private GameTheme theme;

    public BoardState CurrentBoardState => boardState;
    public LevelBoardView CurrentBoardView => currentBoardView;

    public event Action OnLevelLoadedSuccessfully;
    public event Action OnReturnToMainMenu;

    public void Initialize(LevelLoader loader, LivesManager lives, ScoreManager score, UIPanelManager ui,
        BoosterController booster, TimerController timer, AudioManager audio, GameTheme gameTheme)
    {
        levelLoader = loader;
        livesManager = lives;
        scoreManager = score;
        uiPanelManager = ui;
        boosterController = booster;
        timerController = timer;
        audioManager = audio;
        theme = gameTheme;
    }

    public void StartGame(GameManager gm)
    {
        audioManager.PlayClick();
        scoreManager.ResetScore();

        currentLevel = 0;
        LoadLevel(currentLevel, gm);
    }

    public void LoadLevel(int levelIndex, GameManager gm)
    {
        currentLevel = levelIndex;
        Time.timeScale = 1f;

        uiPanelManager.ShowLevelUI();
        boosterController.EnsureBoosterButtons();

        livesManager.ResetLives(theme.startingLives);
        ClearCurrentBoard();

        bool success = levelLoader.LoadLevel(currentLevel, gm, out boardState, out currentBoardView, out currentBoardInstance);
        if (!success)
        {
            ClearCurrentBoard();
            ExitToMainMenu();
            return;
        }

        timerController.StartTimer(currentBoardView.timeLimitSeconds);
        boosterController.ResetBoosters();

        OnLevelLoadedSuccessfully?.Invoke();
    }

    public void ClearCurrentBoard()
    {
        levelLoader.ClearCurrentBoard(ref currentBoardInstance, ref currentBoardView);
        boardState = null;
    }

    public void NextLevel(GameManager gm)
    {
        audioManager.PlayClick();
        LoadLevel(currentLevel + 1, gm);
    }

    public void RestartGame(GameManager gm)
    {
        audioManager.PlayClick();
        scoreManager.ResetScore();

        LoadLevel(currentLevel, gm);
    }

    public void ExitToMainMenu()
    {
        audioManager.PlayClick();
        Time.timeScale = 1f;
        uiPanelManager.ShowMainMenuUI();

        ClearCurrentBoard();
        OnReturnToMainMenu?.Invoke();
    }
}
