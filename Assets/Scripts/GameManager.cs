using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using MonsterBiome.Core.Models;

[DefaultExecutionOrder(-50)]
public class GameManager : MonoBehaviour
{
    [Header("Modular Components")]
    [SerializeField] private BiomePalette biomePalette;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private UIPanelManager uiPanelManager;
    [SerializeField] private SettingsController settingsController;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private LivesManager livesManager;
    [SerializeField] private TimerController timerController;
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private BoosterController boosterController;

    [Header("Level Configuration")]
    public int currentLevel = 0;

    [Header("Game Over Effects")]
    public CanvasGroup darkOverlay;
    public CanvasGroup gameOverCanvasGroup;


    private BoardState boardState;
    private LevelBoardView currentBoardView;
    private GameObject currentBoardInstance;
    private bool isGameOver = false;
    private Tween boosterDelayedTween;

    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f;
    private int lastRow = -1;
    private int lastCol = -1;

    public int[,] gridData => boardState?.GridData;
    public bool[,] solutionCells => boardState?.SolutionCells;
    public int[,] placedMonsters => boardState?.PlacedMonsters;
    public int[,] cellMarks => boardState?.CellMarks;
    public int[,] errorCells => boardState?.ErrorCells;

    public Color GetBiomeColor(int biomeID) => biomePalette != null ? biomePalette.GetBiomeColor(biomeID) : Color.white;
    public Sprite GetMonsterSprite(int biomeID) => biomePalette != null ? biomePalette.GetMonsterSprite(biomeID) : null;
    public bool IsGameOver() => isGameOver;


    private T GetOrAddComponent<T>() where T : Component
    {
        T comp = GetComponent<T>();
        if (comp == null) comp = FindFirstObjectByType<T>(FindObjectsInactive.Include);
        if (comp == null) comp = gameObject.AddComponent<T>();
        return comp;
    }



    private void Start()
    {
        timerController.OnTimerExpired += GameOver;
        livesManager.OnLivesDepleted += GameOver;
        boosterController.OnFindOneRequested += HandleFindOne;
        boosterController.OnFreezeTimeRequested += HandleFreezeTime;
        boosterController.OnBoosterTargetClicked += ProcessBoosterTarget;

        uiPanelManager.InitializeUI();
        UpdateToggleButtonsUI();
    }

    private void OnDestroy()
    {
        boosterDelayedTween?.Kill();
        boosterDelayedTween = null;

        if (timerController != null) timerController.OnTimerExpired -= GameOver;
        if (livesManager != null) livesManager.OnLivesDepleted -= GameOver;
        if (boosterController != null)
        {
            boosterController.OnFindOneRequested -= HandleFindOne;
            boosterController.OnFreezeTimeRequested -= HandleFreezeTime;
            boosterController.OnBoosterTargetClicked -= ProcessBoosterTarget;
        }
    }

    private void PlayClick()
    {
        audioManager.PlayClick();
    }

    public void StartGame()
    {
        PlayClick();
        uiPanelManager.ShowLevelUI();

        currentLevel = 0;
        livesManager.ResetLives(3);
        scoreManager.ResetScore();
        LoadLevel(currentLevel);
    }

    public void OpenSettings()
    {
        PlayClick();
        UpdateToggleButtonsUI();
        uiPanelManager.ShowPanel(uiPanelManager.settingsPanel);
    }

    public void CloseSettings()
    {
        PlayClick();
        uiPanelManager.HidePanel(uiPanelManager.settingsPanel, false);
    }

    public void OpenHowToPlay()
    {
        PlayClick();
        uiPanelManager.ShowPanel(uiPanelManager.howToPlayPanel);
    }

    public void CloseHowToPlay()
    {
        PlayClick();
        uiPanelManager.HidePanel(uiPanelManager.howToPlayPanel, false);
    }

    public void ToggleMusic()
    {
        audioManager.ToggleMusic();
        UpdateToggleButtonsUI();
    }

    public void ToggleSFX()
    {
        audioManager.ToggleSFX();
        UpdateToggleButtonsUI();
    }

    public void ToggleVibration()
    {
        settingsController.ToggleVibration(audioManager);
        UpdateToggleButtonsUI();
    }

    public void UpdateToggleButtonsUI()
    {
        settingsController.UpdateToggleButtonsUI(audioManager.IsMusicMuted, audioManager.IsSFXMuted);
    }

    public void RestartFromSettings()
    {
        CloseSettings();
        RestartGame();
    }

    public void ExitToMainMenu()
    {
        PlayClick();
        isGameOver = true;
        uiPanelManager.ShowMainMenuUI();
        ClearCurrentBoard();
    }

    public void LoadLevel(int levelIndex)
    {
        currentLevel = levelIndex;
        isGameOver = false;

        uiPanelManager.ShowLevelUI();
        boosterController.EnsureBoosterButtons(uiPanelManager.boosterPanel);

        livesManager.ResetLives(3);
        ClearCurrentBoard();

        bool success = levelLoader.LoadLevel(currentLevel, this, out boardState, out currentBoardView, out currentBoardInstance);
        if (!success)
        {
            ClearCurrentBoard();
            ExitToMainMenu();
            return;
        }

        if (currentBoardView != null)
            timerController.StartTimer(currentBoardView.timeLimitSeconds);
        boosterController.ResetBoosters();
    }

    private void ClearCurrentBoard()
    {
        boosterDelayedTween?.Kill();
        boosterDelayedTween = null;
        levelLoader.ClearCurrentBoard(ref currentBoardInstance, ref currentBoardView);
        boardState = null;
    }

    public void HandleCellClick(int row, int col)
    {
        if (isGameOver || boardState == null) return;

        if (boosterController.ActiveBooster != BoosterType.None)
        {
            boosterController.HandleCellClickWithBooster(row, col);
            return;
        }

        if (boardState.IsErrorCell(row, col)) return;

        int biomeID = boardState.GridData[row, col];
        if (biomeID == 0) return;

        if (boardState.IsPlacedMonster(row, col))
        {
            RemoveMonster(row, col);
            PlayClick();
            return;
        }

        float timeSinceLastClick = Time.time - lastClickTime;
        if (timeSinceLastClick <= doubleClickThreshold && lastRow == row && lastCol == col)
        {
            TryPlaceMonster(row, col, biomeID);
            lastClickTime = 0f;
        }
        else
        {
            ToggleMark(row, col, biomeID);
            PlayClick();
            lastClickTime = Time.time;
            lastRow = row;
            lastCol = col;
        }
    }

    private void ToggleMark(int row, int col, int biomeID)
    {
        BoardCell targetCell = currentBoardView != null ? currentBoardView.GetCell(row, col, boardState.Cols) : null;
        bool isMarked = boardState.ToggleMark(row, col);
        if (targetCell != null)
        {
            targetCell.SetMarkState(isMarked, GetBiomeColor(biomeID));
        }
    }

    private void TryPlaceMonster(int row, int col, int biomeID)
    {
        BoardCell targetCell = currentBoardView != null ? currentBoardView.GetCell(row, col, boardState.Cols) : null;

        if (boardState.IsValidPlacement(row, col, biomeID))
        {
            PlaceMonsterAt(row, col, biomeID);
        }
        else
        {
            livesManager.DeductLife();
            audioManager.PlayError();
            if (!Application.isEditor && !settingsController.IsVibrationOff) Handheld.Vibrate();

            if (targetCell != null)
            {
                Sprite errorSprite = biomePalette != null ? biomePalette.brokenHeartSprite : null;
                targetCell.ShowErrorSprite(errorSprite);
            }
            boardState.MarkError(row, col);
        }
    }

    private void PlaceMonsterAt(int row, int col, int biomeID)
    {
        boardState.PlaceMonster(row, col);

        BoardCell targetCell = currentBoardView != null ? currentBoardView.GetCell(row, col, boardState.Cols) : null;
        if (targetCell != null)
        {
            targetCell.SetMonsterState(true, GetMonsterSprite(biomeID), GetBiomeColor(biomeID));
        }

        scoreManager.AddScore(100);
        audioManager.PlayPlaceMonster();

        if (boardState.PlacedMonstersCount >= boardState.CountTotalSolutionCells())
        {
            GameWin();
        }
    }

    private void RemoveMonster(int row, int col)
    {
        boardState.RemoveMonster(row, col);

        BoardCell targetCell = currentBoardView != null ? currentBoardView.GetCell(row, col, boardState.Cols) : null;
        if (targetCell != null)
        {
            targetCell.SetMonsterState(false, null, Color.white);
        }
    }

    private void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        timerController.StopTimer();
        audioManager.PlayLose();

        Sequence gameOverSeq = DOTween.Sequence().SetUpdate(true);

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

    private void ShowGameOverPanel()
    {
        if (uiPanelManager.gameOverUI != null)
        {
            uiPanelManager.gameOverUI.SetActive(true);
            if (gameOverCanvasGroup != null)
            {
                gameOverCanvasGroup.alpha = 0f;
                uiPanelManager.gameOverUI.transform.localScale = Vector3.one * 1.1f;
                gameOverCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.OutQuad).SetUpdate(true);
                uiPanelManager.gameOverUI.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutQuad).SetUpdate(true);
            }
        }

        if (uiPanelManager.restartButton != null)
        {
            uiPanelManager.restartButton.SetActive(true);
        }
    }

    private void GameWin()
    {
        if (isGameOver) return;
        isGameOver = true;
        timerController.StopTimer();

        int livesLeft = livesManager.Lives;
        scoreManager.AddScore(500 + (livesLeft * 100));

        if (uiPanelManager.winScreenUI != null)
        {
            uiPanelManager.winScreenUI.SetActive(true);
            uiPanelManager.ShowPopupScale(uiPanelManager.winScreenUI);
        }

        if (uiPanelManager.restartButton != null) uiPanelManager.restartButton.SetActive(false);
        if (uiPanelManager.nextLevelButton != null) uiPanelManager.nextLevelButton.SetActive(true);

        audioManager.PlayWin();
    }

    public void NextLevel()
    {
        PlayClick();
        LoadLevel(currentLevel + 1);
    }

    public void RestartGame()
    {
        PlayClick();
        Time.timeScale = 1f;
        scoreManager.ResetScore();

        uiPanelManager.ShowLevelUI();
        LoadLevel(currentLevel);
    }

    public void OnClickFindOne()
    {
        if (isGameOver || boardState == null) return;
        if (boosterController.findOneCount <= 0) return;
        HandleFindOne();
    }
    public void OnClickFreezeTime()
    {
        boosterController.OnClickFreezeTime(isGameOver);
    }
    public void OnClickRocket()
    {
        boosterController.OnClickRocket(isGameOver);
    }
    public void OnClickBow()
    {
        boosterController.OnClickBow(isGameOver);
    }

    private void HandleFindOne()
    {
        if (boardState == null) return;
        var allCells = new List<(int, int)>();
        for (int r = 0; r < boardState.Rows; r++)
            for (int c = 0; c < boardState.Cols; c++)
                allCells.Add((r, c));

        if (TryAutoPlaceInScope(allCells))
        {
            boosterController.ConsumeFindOne();
        }
    }

    private void HandleFreezeTime()
    {
        timerController.AddFreezeTime(15f);
    }

    private void ProcessBoosterTarget(int targetRow, int targetCol, BoosterType boosterType)
    {
        if (boardState == null) return;

        var scope = new List<(int, int)>();

        if (boosterType == BoosterType.Rocket)
        {
            for (int r = 0; r < boardState.Rows; r++)
                scope.Add((r, targetCol));
        }
        else if (boosterType == BoosterType.Bow)
        {
            for (int c = 0; c < boardState.Cols; c++)
                scope.Add((targetRow, c));
        }

        (int row, int col)? correctCell = null;
        foreach (var (row, col) in scope)
        {
            if (boardState.SolutionCells[row, col] && boardState.PlacedMonsters[row, col] == 0)
            {
                correctCell = (row, col);
                break;
            }
        }

        foreach (var (row, col) in scope)
        {
            bool isCorrect = correctCell != null && row == correctCell.Value.row && col == correctCell.Value.col;
            bool isEmpty = boardState.GridData[row, col] == 0;
            bool alreadyPlaced = boardState.PlacedMonsters[row, col] == 1;

            if (!isCorrect && !isEmpty && !alreadyPlaced)
            {
                bool isMarked = boardState.ToggleMark(row, col);
                BoardCell cell = currentBoardView != null ? currentBoardView.GetCell(row, col, boardState.Cols) : null;
                if (cell != null)
                {
                    cell.SetMarkState(isMarked, GetBiomeColor(boardState.GridData[row, col]));
                }
            }
        }

        if (correctCell != null)
        {
            boosterDelayedTween?.Kill();
            BoardState targetBoardState = boardState;

            boosterDelayedTween = DOVirtual.DelayedCall(0.4f, () =>
            {
                boosterDelayedTween = null;
                if (isGameOver || boardState == null || boardState != targetBoardState) return;
                var (r, c) = correctCell.Value;
                TryAutoPlaceInScope(new List<(int, int)> { (r, c) });
            });
        }
    }

    private bool TryAutoPlaceInScope(IEnumerable<(int row, int col)> candidateCells)
    {
        if (boardState == null) return false;
        foreach (var (row, col) in candidateCells)
        {
            if (boardState.SolutionCells[row, col] && boardState.PlacedMonsters[row, col] == 0)
            {
                int biomeID = boardState.GridData[row, col];
                if (biomeID != 0)
                {
                    PlaceMonsterAt(row, col, biomeID);
                    return true;
                }
            }
        }
        return false;
    }
}