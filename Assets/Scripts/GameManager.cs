using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public enum BoosterType { None, FindOne, FreezeTime, Rocket, Bow }

public class GameManager : MonoBehaviour
{
    [Header("Level Configuration")]
    public int currentLevel = 0;
    public GameObject[] levelPrefabs;
    public Transform boardContainer;
    public TextMeshProUGUI levelTitleText;

    [Header("Board Data (Dynamic)")]
    public int[,] gridData;
    public bool[,] solutionCells; 
    public int[,] placedMonsters;
    public int[,] cellMarks;
    public int[,] errorCells;

    private int currentRows = 0;
    private int currentCols = 0;

    private LevelBoardView currentBoardView;
    private GameObject currentBoardInstance;

    [Header("Game Over Effects")]
    public CanvasGroup darkOverlay; 
    public CanvasGroup gameOverCanvasGroup; 

    [Header("UI & Asset References")]
    public Color[] biomeColors;
    public Sprite[] monsterSprites;
    public Sprite brokenHeartSprite;

    public Color GetBiomeColor(int biomeID)
    {
        if (biomeColors == null || biomeColors.Length <= 1) return Color.white;
        if (biomeID < biomeColors.Length) return biomeColors[biomeID];
        int safeIndex = ((biomeID - 1) % (biomeColors.Length - 1)) + 1;
        return biomeColors[safeIndex];
    }

    public Sprite GetMonsterSprite(int biomeID)
    {
        if (monsterSprites == null || monsterSprites.Length <= 1) return null;
        if (biomeID < monsterSprites.Length) return monsterSprites[biomeID];
        int safeIndex = ((biomeID - 1) % (monsterSprites.Length - 1)) + 1;
        return monsterSprites[safeIndex];
    }

    public GameObject mainMenuUI;
    public GameObject settingsPanel;
    public GameObject gameOverUI;
    public GameObject winScreenUI;
    public GameObject restartButton;
    public GameObject nextLevelButton;
    public GameObject topBarPanel;
    public GameObject howToPlayPanel;
    public GameObject boosterPanel;

    [Header("Toggle Button Slash Overlays")]
    public GameObject musicSlashOverlay;
    public GameObject soundSlashOverlay;
    public GameObject vibrateSlashOverlay;

    [Header("Lives & Score System")]
    public int lives = 3;
    [Header("Lives Display (mới)")]
    public TMPro.TextMeshProUGUI livesCountText;
    public TextMeshProUGUI scoreText;
    private int currentScore = 0;
    private int displayedScore = 0;

    [Header("Timer System")]
    private float currentTime;
    private bool isTimerRunning = false;

    [Header("Timer Display (mới)")]
    public TMPro.TextMeshProUGUI timerText;

    [Header("Audio System")]
    public AudioSource sfxSource;
    public AudioSource bgmSource;
    public AudioClip clickSound;
    public AudioClip placeMonsterSound;
    public AudioClip errorSound;
    public AudioClip winSound;
    public AudioClip loseSound;

    [Header("Booster System")]
    public int findOneCount = 1;
    public int freezeTimeCount = 1;
    public int rocketCount = 1;
    public int bowCount = 1;


    [Header("Booster Buttons")]
    public Button findOneBtn;
    public Button freezeTimeBtn;
    public Button rocketBtn;
    public Button bowBtn;

    private BoosterType activeBooster = BoosterType.None;
    private float freezeTimeRemaining = 0f;

    private bool isGameOver = false;
    private bool isMusicMuted = false;
    private bool isSFXMuted = false;
    private bool isVibrationOff = false;

    private int placedMonstersCount = 0;
    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f;
    private int lastRow = -1;
    private int lastCol = -1;
    private Color defaultTimerColor = Color.white;

    void Update()
    {
        if (isGameOver) return;

        if (freezeTimeRemaining > 0f)
        {
            freezeTimeRemaining -= Time.deltaTime;
            UpdateTimerDisplay(currentTime, Color.cyan);
            return;
        }

        if (!isTimerRunning) return;

        currentTime -= Time.deltaTime;

        int secondsLeft = Mathf.CeilToInt(Mathf.Max(0, currentTime));
        Color timerColor = (secondsLeft <= 5) ? Color.red : defaultTimerColor;
        UpdateTimerDisplay(currentTime, timerColor);

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isTimerRunning = false;
            Debug.Log("[Timer] Hết giờ! Game Over.");
            GameOver();
        }
    }

    private void UpdateTimerDisplay(float timeInSeconds, Color textColor)
    {
        int secondsLeft = Mathf.CeilToInt(Mathf.Max(0, timeInSeconds));
        int minutes = secondsLeft / 60;
        int seconds = secondsLeft % 60;
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        timerText.color = textColor;
    }

    void Start()
    {
        defaultTimerColor = timerText.color;

        mainMenuUI.SetActive(true);
        EnsureToggleSlashOverlays();
        UpdateToggleButtonsUI();
        settingsPanel.SetActive(false);
        gameOverUI.SetActive(false);
        winScreenUI.SetActive(false);
        restartButton.SetActive(false);
        nextLevelButton.SetActive(false);
        topBarPanel.SetActive(false);
        EnsureHowToPlayCloseButton();
        howToPlayPanel.SetActive(false);
        boosterPanel.SetActive(false);

        foreach (var graphic in boosterPanel.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
        {
            if (graphic.GetComponent<Button>() == null)
            {
                graphic.raycastTarget = false;
            }
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void StartGame()
    {
        PlaySFX(clickSound);
        mainMenuUI.SetActive(false);
        topBarPanel.SetActive(true);

        currentLevel = 0;
        lives = 3;
        currentScore = 0;
        UpdateScoreUI();
        LoadLevel(currentLevel);
    }

    public void OpenSettings()
    {
        PlaySFX(clickSound);
        UpdateToggleButtonsUI();
        settingsPanel.SetActive(true);
        ShowPanel(settingsPanel);
        Time.timeScale = 0f;
    }

    public void CloseSettings()
    {
        PlaySFX(clickSound);
        HidePanel(settingsPanel, true);
    }

    public void OpenHowToPlay()
    {
        PlaySFX(clickSound);
        howToPlayPanel.SetActive(true);
        ShowPanel(howToPlayPanel);
    }

    public void CloseHowToPlay()
    {
        PlaySFX(clickSound);
        HidePanel(howToPlayPanel, false);
    }

    private void EnsureHowToPlayCloseButton()
    {
        Transform closeTrans = howToPlayPanel.transform.Find("PopupBackground/CloseBtn");
        if (closeTrans == null) closeTrans = howToPlayPanel.transform.Find("CloseBtn");
        if (closeTrans == null) closeTrans = howToPlayPanel.transform.Find("PopupBackground/CloseHowToPlayBtn");
        if (closeTrans == null) closeTrans = howToPlayPanel.transform.Find("CloseHowToPlayBtn");

        Button btn = null;
        if (closeTrans != null)
        {
            btn = closeTrans.GetComponent<Button>();
        }
        else
        {
            Transform parentTrans = howToPlayPanel.transform.Find("PopupBackground");
            if (parentTrans == null) parentTrans = howToPlayPanel.transform;

            GameObject closeObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            closeObj.transform.SetParent(parentTrans, false);
            closeObj.transform.SetAsLastSibling();
            closeObj.layer = 5;

            RectTransform rt = closeObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(-45f, -45f);
            rt.sizeDelta = new Vector2(70f, 70f);

            Image img = closeObj.GetComponent<Image>();
            img.raycastTarget = true;
            img.preserveAspect = true;
            img.color = Color.white;


            btn = closeObj.GetComponent<Button>();
        }

        if (btn != null)
        {
            btn.onClick.RemoveListener(CloseHowToPlay);
            btn.onClick.AddListener(CloseHowToPlay);
        }
    }

    public void ToggleMusic()
    {
        PlaySFX(clickSound);
        isMusicMuted = !isMusicMuted;
        bgmSource.mute = isMusicMuted;
        UpdateToggleButtonsUI();
    }

    public void ToggleSFX()
    {
        isSFXMuted = !isSFXMuted;
        sfxSource.mute = isSFXMuted;
        if (!isSFXMuted) PlaySFX(clickSound);
        UpdateToggleButtonsUI();
    }

    public void ToggleVibration()
    {
        PlaySFX(clickSound);
        isVibrationOff = !isVibrationOff;
        if (!isVibrationOff) Handheld.Vibrate();
        UpdateToggleButtonsUI();
    }

    public void UpdateToggleButtonsUI()
    {
        EnsureToggleSlashOverlays();

        musicSlashOverlay.SetActive(isMusicMuted);
        soundSlashOverlay.SetActive(isSFXMuted);
        vibrateSlashOverlay.SetActive(isVibrationOff);
    }

    private void EnsureToggleSlashOverlays()
    {
        Transform group = settingsPanel.transform.Find("PopupBackground/ToggleButtonsGroup");
        if (group == null) group = settingsPanel.transform.Find("ToggleButtonsGroup");
        if (group == null) return;

        string[] btnNames = { "MusicBtn", "SoundBtn", "VibrateBtn" };
        for (int i = 0; i < btnNames.Length; i++)
        {
            Transform btnTrans = group.Find(btnNames[i]);
            if (btnTrans == null) continue;

            Transform slashTrans = btnTrans.Find("SlashOverlay");
            GameObject slashObj;
            if (slashTrans == null)
            {
                slashObj = new GameObject("SlashOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                slashObj.transform.SetParent(btnTrans, false);
                slashObj.transform.SetAsLastSibling();
                slashObj.layer = 5;

                RectTransform rt = slashObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(10f, 75f);
                rt.localEulerAngles = new Vector3(0f, 0f, -45f);

                Image img = slashObj.GetComponent<Image>();
                img.raycastTarget = false;
                img.color = new Color(0.95f, 0.2f, 0.2f, 1f);
            }
            else
            {
                slashObj = slashTrans.gameObject;
            }

            if (i == 0 && musicSlashOverlay == null) musicSlashOverlay = slashObj;
            else if (i == 1 && soundSlashOverlay == null) soundSlashOverlay = slashObj;
            else if (i == 2 && vibrateSlashOverlay == null) vibrateSlashOverlay = slashObj;
        }
    }
    public void RestartFromSettings()
    {
        CloseSettings();
        RestartGame();
    }

    private void ClearCurrentBoard()
    {
        if (currentBoardInstance != null)
        {
            Destroy(currentBoardInstance);
            currentBoardInstance = null;
            currentBoardView = null;
        }
    }

    public void ExitToMainMenu()
    {
        PlaySFX(clickSound);
        Time.timeScale = 1f;

        CloseSettings();
        gameOverUI.SetActive(false);
        winScreenUI.SetActive(false);
        topBarPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        boosterPanel.SetActive(false);
        restartButton.SetActive(false);
        nextLevelButton.SetActive(false);

        ClearCurrentBoard();

        mainMenuUI.SetActive(true);
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex >= levelPrefabs.Length)
        {
            Debug.LogError($"[GameManager] Hết màn! levelIndex={levelIndex} >= {levelPrefabs.Length}");
            return;
        }

        currentLevel = levelIndex;
        levelTitleText.text = "MÀN " + (currentLevel + 1);

        lives = 3;
        placedMonstersCount = 0;
        isGameOver = false;

        gameOverUI.SetActive(false);
        winScreenUI.SetActive(false);
        restartButton.SetActive(false);
        nextLevelButton.SetActive(false);
        boosterPanel.SetActive(true);

        livesCountText.text = "x" + lives;

        ClearCurrentBoard();

        currentBoardInstance = Instantiate(levelPrefabs[currentLevel], boardContainer);

        currentBoardInstance.transform.localPosition = Vector3.zero;
        currentBoardInstance.transform.localScale = Vector3.one;

        currentBoardView = currentBoardInstance.GetComponent<LevelBoardView>();

        if (currentBoardView == null)
        {
            Debug.LogError("[GameManager] Prefab màn chơi thiếu script LevelBoardView!");
            return;
        }

        TextAsset textFile = currentBoardView.levelTextFile;
        if (textFile == null)
        {
            Debug.LogError("[GameManager] Level Text File chưa được gán vào LevelBoardView!");
            return;
        }

        int[,] parsedGrid;
        try
        {
            parsedGrid = LevelTextParser.Parse(textFile.text, out currentRows, out currentCols);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GameManager] Lỗi load level: {ex.Message}");
            return;
        }

        var solutions = LevelSolver.Solve(parsedGrid, currentRows, currentCols, maxSolutionsToFind: 2);

        if (solutions.Count == 0)
        {
            Debug.LogError($"[GameManager] Level {currentLevel} VÔ NGHIỆM — không thể chơi được! " +
                            "Cần sửa lại file text level này.");
  
            ClearCurrentBoard();
            ExitToMainMenu();
            return;
        }

        if (solutions.Count >= 2)
        {
            Debug.LogError($"[GameManager] ⚠️ CẢNH BÁO THIẾT KẾ: Level {currentLevel} có NHIỀU HƠN 1 NGHIỆM! " +
                            "Puzzle không rõ ràng — người chơi có thể bị báo sai dù đặt đúng luật theo nghiệm khác. " +
                            "Cần kiểm tra lại vùng biome của level này TRƯỚC KHI phát hành.");
        }

        solutionCells = new bool[currentRows, currentCols];
        foreach (var (row, col) in solutions[0])
            solutionCells[row, col] = true;

        bool isBoardValid = currentBoardView.InitializeBoard(this, parsedGrid, currentRows, currentCols);
        if (!isBoardValid) return;

        currentTime = currentBoardView.timeLimitSeconds;
        isTimerRunning = true;

        gridData = parsedGrid;
        placedMonsters = new int[currentRows, currentCols];
        cellMarks = new int[currentRows, currentCols];
        errorCells = new int[currentRows, currentCols];

  
        activeBooster = BoosterType.None;
        freezeTimeRemaining = 0f;
        UpdateBoosterUI();
    }

    public void HandleCellClick(int row, int col)
    {
        if (isGameOver) return;

 
        if (activeBooster != BoosterType.None)
        {
            HandleBoosterTargetClick(row, col);
            return;
        }

        if (errorCells != null && errorCells[row, col] == 1) return; 
        int biomeID = gridData[row, col];
        if (biomeID == 0) return;

        if (placedMonsters[row, col] == 1)
        {
            RemoveMonster(row, col);
            PlaySFX(clickSound);
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
            PlaySFX(clickSound);
            lastClickTime = Time.time;
            lastRow = row;
            lastCol = col;
        }
    }

    void ToggleMark(int row, int col, int biomeID)
    {
        BoardCell targetCell = currentBoardView.GetCell(row, col, currentCols);

        if (cellMarks[row, col] == 0)
        {
            cellMarks[row, col] = 1;
            targetCell.SetMarkState(true, GetBiomeColor(biomeID));
        }
        else
        {
            cellMarks[row, col] = 0;
            targetCell.SetMarkState(false, GetBiomeColor(biomeID));
        }
    }

    void TryPlaceMonster(int row, int col, int biomeID)
    {
        BoardCell targetCell = currentBoardView.GetCell(row, col, currentCols);

        if (IsValidPlacement(row, col, biomeID))
        {
            PlaceMonsterAt(row, col, biomeID);
        }
        else
        {
            lives--;

            livesCountText.text = "x" + lives;

            PlaySFX(errorSound);
            if (!isVibrationOff) Handheld.Vibrate();

            targetCell.ShowErrorSprite(brokenHeartSprite);
            if (errorCells != null) errorCells[row, col] = 1;

            if (lives <= 0) GameOver();
        }
    }

    private void PlaceMonsterAt(int row, int col, int biomeID)
    {
        placedMonsters[row, col] = 1;
        cellMarks[row, col] = 0;

        BoardCell targetCell = currentBoardView.GetCell(row, col, currentCols);
        targetCell.SetMonsterState(true, GetMonsterSprite(biomeID), GetBiomeColor(biomeID));

        placedMonstersCount++;
        AddScore(100);
        PlaySFX(placeMonsterSound);

        if (placedMonstersCount >= CountTotalSolutionCells()) GameWin();
    }

    void RemoveMonster(int row, int col)
    {
        placedMonsters[row, col] = 0;
        placedMonstersCount--;

        BoardCell targetCell = currentBoardView.GetCell(row, col, currentCols);
        targetCell.SetMonsterState(false, null, GetBiomeColor(gridData[row, col]));

        AddScore(-100);
    }

    void GameOver()
    {
        isGameOver = true;
        isTimerRunning = false;
        PlaySFX(loseSound);

        Sequence gameOverSeq = DOTween.Sequence().SetUpdate(true);

        
        if (Camera.main != null)
        {
            gameOverSeq.Insert(0, Camera.main.transform.DOShakePosition(0.3f, strength: 0.15f, vibrato: 10).SetUpdate(true));
        }

       
        darkOverlay.gameObject.SetActive(true);
        darkOverlay.alpha = 0f;
        gameOverSeq.Insert(0, darkOverlay.DOFade(0.5f, 0.6f).SetEase(Ease.OutQuad).SetUpdate(true));

        
        gameOverSeq.Insert(0, DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0.3f, 0.4f).SetUpdate(true));

     
        for (int r = 0; r < currentRows; r++)
        {
            for (int c = 0; c < currentCols; c++)
            {
                if (placedMonsters[r, c] == 1)
                {
                    BoardCell cell = currentBoardView.GetCell(r, c, currentCols);
                    gameOverSeq.Insert(0, cell.monsterSprite.DOColor(Color.gray, 0.4f).SetUpdate(true));
                }
            }
        }

        gameOverSeq.InsertCallback(0.8f, () => {
            Time.timeScale = 1f; 
            ShowGameOverPanel();
        });
    }

    void ShowGameOverPanel()
    {
        gameOverUI.SetActive(true);
        
        gameOverCanvasGroup.alpha = 0f;
        gameOverUI.transform.localScale = Vector3.one * 1.1f;

        gameOverCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.OutQuad).SetUpdate(true);
        gameOverUI.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutQuad).SetUpdate(true);

        restartButton.SetActive(true);
    }

    void GameWin()
    {
        isGameOver = true;
        isTimerRunning = false;
        int bonusScore = 500 + (lives * 100);
        AddScore(bonusScore);
        winScreenUI.SetActive(true);
        ShowPopupScale(winScreenUI);
        restartButton.SetActive(false);
        nextLevelButton.SetActive(true);
        PlaySFX(winSound);
    }

    public void NextLevel()
    {
        PlaySFX(clickSound);
        LoadLevel(currentLevel + 1);
    }

    public void RestartGame()
    {
        PlaySFX(clickSound);
        Time.timeScale = 1f; 
        currentScore = 0;
        UpdateScoreUI();

        gameOverUI.SetActive(false);
        winScreenUI.SetActive(false);
        restartButton.SetActive(false);
        nextLevelButton.SetActive(false);
        mainMenuUI.SetActive(false);

        topBarPanel.SetActive(true);

        LoadLevel(currentLevel);
    }

    void AddScore(int amount)
    {
        currentScore += amount;
        if (currentScore < 0) currentScore = 0;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        scoreText.transform.DOKill(true);
        DOTween.To(() => displayedScore, x => { 
            displayedScore = x; 
            scoreText.text = "ĐIỂM: " + x.ToString(); 
        }, currentScore, 0.4f).SetEase(Ease.OutQuad);
        scoreText.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.3f, 2, 0.5f);
    }

    bool IsValidPlacement(int targetRow, int targetCol, int targetBiomeID)
    {

        if (targetBiomeID == 0) return false;
        return solutionCells[targetRow, targetCol];
    }

    int CountTotalSolutionCells()
    {
        int count = 0;
        for (int r = 0; r < currentRows; r++)
            for (int c = 0; c < currentCols; c++)
                if (solutionCells[r, c]) count++;
        return count;
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }


    private bool TryAutoPlaceInScope(IEnumerable<(int row, int col)> candidateCells)
    {
        foreach (var (row, col) in candidateCells)
        {
            if (solutionCells[row, col] && placedMonsters[row, col] == 0)
            {
                int biomeID = gridData[row, col];
                PlaceMonsterAt(row, col, biomeID);
                return true;
            }
        }
        return false;
    }

    public void OnClickFindOne()
    {
        if (findOneCount <= 0 || isGameOver) return;

        var allCells = new List<(int, int)>();
        for (int r = 0; r < currentRows; r++)
            for (int c = 0; c < currentCols; c++)
                allCells.Add((r, c));

        if (TryAutoPlaceInScope(allCells))
        {
            findOneCount--;
            UpdateBoosterUI();
        }
    }


    public void OnClickFreezeTime()
    {
        if (freezeTimeCount <= 0 || isGameOver) return;

        freezeTimeRemaining += 15f;
        UpdateBoosterUI();
    }

 
    public void OnClickRocket()
    {
        if (rocketCount <= 0 || isGameOver) return;
        activeBooster = BoosterType.Rocket;
    }

 
    public void OnClickBow()
    {
        if (bowCount <= 0 || isGameOver) return;
        activeBooster = BoosterType.Bow;
    }


    private void HandleBoosterTargetClick(int targetRow, int targetCol)
    {
        var scope = new List<(int, int)>();

        if (activeBooster == BoosterType.Rocket)
        {
  
            for (int r = 0; r < currentRows; r++)
                scope.Add((r, targetCol));
        }
        else if (activeBooster == BoosterType.Bow)
        {
   
            for (int c = 0; c < currentCols; c++)
                scope.Add((targetRow, c));
        }


        (int row, int col)? correctCell = null;
        foreach (var (row, col) in scope)
        {
            if (solutionCells[row, col] && placedMonsters[row, col] == 0)
            {
                correctCell = (row, col);
                break;
            }
        }


        foreach (var (row, col) in scope)
        {
            bool isCorrect = correctCell != null && row == correctCell.Value.row && col == correctCell.Value.col;
            bool isEmpty = gridData[row, col] == 0;
            bool alreadyPlaced = placedMonsters[row, col] == 1;

            if (!isCorrect && !isEmpty && !alreadyPlaced)
            {
                cellMarks[row, col] = 1;
                BoardCell cell = currentBoardView.GetCell(row, col, currentCols);
                cell.SetMarkState(true, GetBiomeColor(gridData[row, col]));
            }
        }

        if (activeBooster == BoosterType.Rocket) rocketCount--;
        else if (activeBooster == BoosterType.Bow) bowCount--;
        UpdateBoosterUI();

        activeBooster = BoosterType.None; 

        if (correctCell == null)
        {
            return; 
        }

       
        DG.Tweening.DOVirtual.DelayedCall(0.4f, () =>
        {
            var (r, c) = correctCell.Value;
            cellMarks[r, c] = 0; 
            TryAutoPlaceInScope(new List<(int, int)> { (r, c) });
        });
    }

    private void UpdateBoosterUI()
    {
        findOneBtn.interactable = (findOneCount > 0);
        freezeTimeBtn.interactable = (freezeTimeCount > 0);
        rocketBtn.interactable = (rocketCount > 0);
        bowBtn.interactable = (bowCount > 0);
    }

    private void ShowPopupScale(GameObject panel)
    {
        panel.transform.DOKill();
        panel.transform.localScale = Vector3.zero;
        panel.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    private void ShowPanel(GameObject panel)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (rect != null)
        {
            rect.DOKill();
            rect.anchoredPosition = new Vector2(0, 800);
            rect.DOAnchorPos(Vector2.zero, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
        }
        if (cg != null)
        {
            cg.DOKill();
            cg.alpha = 0f;
            cg.DOFade(1f, 0.4f).SetUpdate(true);
        }
    }

    private void HidePanel(GameObject panel, bool resumeTime)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        
        Sequence seq = DOTween.Sequence().SetUpdate(true);
        if (rect != null)
        {
            rect.DOKill();
            seq.Join(rect.DOAnchorPos(new Vector2(0, -800), 0.3f).SetEase(Ease.InBack));
        }
        if (cg != null)
        {
            cg.DOKill();
            seq.Join(cg.DOFade(0f, 0.3f));
        }
        
        seq.OnComplete(() => {
            panel.SetActive(false);
            if (resumeTime) Time.timeScale = 1f;
        });
    }
}