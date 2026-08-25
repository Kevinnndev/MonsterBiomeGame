using UnityEngine;
using MonsterBiome.Core.Models;

[DefaultExecutionOrder(-50)]
[RequireComponent(typeof(BoardMoveExecutor))]
[RequireComponent(typeof(BoardInputController))]
public class GameManager : MonoBehaviour
{
    [Header("Modular Controllers")]
    [SerializeField] private LevelFlowController levelFlowController;
    [SerializeField] private GameEndSequenceController gameEndSequenceController;
    [SerializeField] private GameTheme theme;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private UIPanelManager uiPanelManager;
    [SerializeField] private SettingsController settingsController;
    [SerializeField] private SettingsUIController settingsUIController;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private LivesManager livesManager;
    [SerializeField] private TimerController timerController;
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private BoosterController boosterController;
    [SerializeField] private BoardMoveExecutor moveExecutor;
    [SerializeField] private BoardInputController inputController;

    private bool isGameOver = false;

    public int currentLevel => levelFlowController.currentLevel;
    public BoardState boardState => levelFlowController.CurrentBoardState;
    public LevelBoardView currentBoardView => levelFlowController.CurrentBoardView;

    public Color GetBiomeColor(int biomeID) => theme.GetBiomeColor(biomeID);
    public Sprite GetMonsterSprite(int biomeID) => theme.GetMonsterSprite(biomeID);
    public bool IsGameOver() => isGameOver;

    private T GetOrAddComponent<T>() where T : Component
    {
        T comp = GetComponent<T>();
        if (comp == null) comp = FindFirstObjectByType<T>(FindObjectsInactive.Include);
        if (comp == null)
        {
            Debug.LogWarning($"[GameManager] {typeof(T).Name} not found in scene, auto-created on {name}. Assign it in the Inspector to keep scene setup explicit.", this);
            comp = gameObject.AddComponent<T>();
        }
        return comp;
    }

    private void Awake()
    {
        EnsureModularComponents();
    }

    private void EnsureModularComponents()
    {
        if (levelFlowController == null) levelFlowController = GetOrAddComponent<LevelFlowController>();
        if (gameEndSequenceController == null) gameEndSequenceController = GetOrAddComponent<GameEndSequenceController>();
        if (theme == null) Debug.LogError("[GameManager] GameTheme asset is not assigned in the Inspector.", this);
        if (audioManager == null) audioManager = GetOrAddComponent<AudioManager>();
        if (uiPanelManager == null) uiPanelManager = GetOrAddComponent<UIPanelManager>();
        if (settingsController == null) settingsController = GetOrAddComponent<SettingsController>();
        if (settingsUIController == null) settingsUIController = GetOrAddComponent<SettingsUIController>();
        if (scoreManager == null) scoreManager = GetOrAddComponent<ScoreManager>();
        if (livesManager == null) livesManager = GetOrAddComponent<LivesManager>();
        if (timerController == null) timerController = GetOrAddComponent<TimerController>();
        if (levelLoader == null) levelLoader = GetOrAddComponent<LevelLoader>();
        if (boosterController == null) boosterController = GetOrAddComponent<BoosterController>();
        if (moveExecutor == null) moveExecutor = GetComponent<BoardMoveExecutor>();
        if (inputController == null) inputController = GetComponent<BoardInputController>();
    }

    private void Start()
    {
        levelFlowController.Initialize(levelLoader, livesManager, scoreManager, uiPanelManager,
            boosterController, timerController, audioManager, theme);
        gameEndSequenceController.Initialize(timerController, audioManager, uiPanelManager, livesManager, scoreManager, theme);
        boosterController.Initialize(() => boardState, () => isGameOver, moveExecutor, timerController, theme);

        timerController.OnTimerExpired += GameOver;
        livesManager.OnLivesDepleted += GameOver;
        gameEndSequenceController.OnGameOverTriggered += () => isGameOver = true;
        gameEndSequenceController.OnGameWinTriggered += () => isGameOver = true;
        levelFlowController.OnLevelLoadedSuccessfully += HandleLevelLoaded;

        moveExecutor.Initialize(() => boardState, () => currentBoardView,
            theme, audioManager, scoreManager, livesManager, settingsController);
        moveExecutor.OnBoardCompleted += GameWin;

        inputController.Initialize(() => boardState, () => isGameOver, boosterController);
        inputController.PlaceRequested += moveExecutor.TryPlaceMonster;
        inputController.MarkRequested += moveExecutor.ToggleMark;
        inputController.RemoveRequested += moveExecutor.RemoveMonster;
        inputController.ClickSoundRequested += audioManager.PlayClick;

        uiPanelManager.InitializeUI();
    }

    private void OnDestroy()
    {
        timerController.OnTimerExpired -= GameOver;
        livesManager.OnLivesDepleted -= GameOver;

        moveExecutor.OnBoardCompleted -= GameWin;
        inputController.PlaceRequested -= moveExecutor.TryPlaceMonster;
        inputController.MarkRequested -= moveExecutor.ToggleMark;
        inputController.RemoveRequested -= moveExecutor.RemoveMonster;
        inputController.ClickSoundRequested -= audioManager.PlayClick;
    }

    // --- Level Flow Delegates ---
    public void StartGame()
    {
        levelFlowController.StartGame(this);
    }

    public void LoadLevel(int levelIndex)
    {
        levelFlowController.LoadLevel(levelIndex, this);
    }

    public void NextLevel()
    {
        levelFlowController.NextLevel(this);
    }

    public void RestartGame()
    {
        levelFlowController.RestartGame(this);
    }

    public void ExitToMainMenu()
    {
        isGameOver = true;
        levelFlowController.ExitToMainMenu();
    }

    // --- End Sequence Delegates ---
    private void HandleLevelLoaded()
    {
        isGameOver = false;
        gameEndSequenceController.CancelEndSequence();
    }

    private void GameOver()
    {
        if (isGameOver) return;
        gameEndSequenceController.PlayGameOverSequence(levelFlowController.CurrentBoardState, levelFlowController.CurrentBoardView);
    }

    private void GameWin()
    {
        if (isGameOver) return;
        gameEndSequenceController.PlayGameWinSequence();
    }

    // --- Settings ---
    public void RestartFromSettings()
    {
        settingsUIController.CloseSettings();
        RestartGame();
    }

    // --- Board Interaction & Booster Processing ---
    public void HandleCellClick(int row, int col)
    {
        inputController.HandleCellClick(row, col);
    }
}
