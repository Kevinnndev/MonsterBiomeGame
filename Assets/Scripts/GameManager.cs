using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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

    [Header("Lives & Score System")]
    public int lives = 3;
    [Tooltip("Không dùng nữa, chờ xoá khi UI mới hoàn thiện")]
    public GameObject[] heartIcons; 
    [Header("Lives Display (mới)")]
    public TMPro.TextMeshProUGUI livesCountText;
    public TextMeshProUGUI scoreText;
    private int currentScore = 0;
    private int displayedScore = 0;

    [Header("Timer System")]
    private float currentTime;
    private bool isTimerRunning = false;
    private int timeLimitSeconds = 0;
    
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
        if (!isTimerRunning || isGameOver) return;
        
        currentTime -= Time.deltaTime;

   
        if (timerText != null)
        {
            int secondsLeft = Mathf.CeilToInt(Mathf.Max(0, currentTime));
            
     
            int minutes = secondsLeft / 60;
            int seconds = secondsLeft % 60;
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            
            if (secondsLeft <= 5)
                timerText.color = Color.red;
            else
                timerText.color = defaultTimerColor;
        }

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isTimerRunning = false;
            Debug.Log("[Timer] Hết giờ! Game Over.");
            GameOver(); // hết giờ = thua NGAY, không trừ mạng
        }
    }

    void Start()
    {
        if (timerText != null)
        {
            defaultTimerColor = timerText.color;
        }

        if (Camera.main != null && Camera.main.GetComponent<UnityEngine.EventSystems.Physics2DRaycaster>() == null)
        {
            Camera.main.gameObject.AddComponent<UnityEngine.EventSystems.Physics2DRaycaster>();
            Debug.Log("[GameManager] Đã tự động thêm Physics2DRaycaster vào Main Camera.");
        }

        if (mainMenuUI != null) mainMenuUI.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (winScreenUI != null) winScreenUI.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);
        if (nextLevelButton != null) nextLevelButton.SetActive(false);
        if (topBarPanel != null) topBarPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void StartGame()
    {
        PlaySFX(clickSound);
        if (mainMenuUI != null) mainMenuUI.SetActive(false);
        if (topBarPanel != null) topBarPanel.SetActive(true);

        currentLevel = 0;
        lives = 3;
        currentScore = 0;
        UpdateScoreUI();
        LoadLevel(currentLevel);
    }

    public void OpenSettings()
    {
        PlaySFX(clickSound);
        if (settingsPanel != null) 
        {
            settingsPanel.SetActive(true);
            ShowPanel(settingsPanel);
        }
        Time.timeScale = 0f;
    }

    public void CloseSettings()
    {
        PlaySFX(clickSound);
        if (settingsPanel != null) HidePanel(settingsPanel, true);
        else Time.timeScale = 1f;
    }

    public void OpenHowToPlay()
    {
        PlaySFX(clickSound);
        if (howToPlayPanel != null) 
        {
            howToPlayPanel.SetActive(true);
            ShowPanel(howToPlayPanel);
        }
    }

    public void CloseHowToPlay()
    {
        PlaySFX(clickSound);
        if (howToPlayPanel != null) HidePanel(howToPlayPanel, false);
    }

    public void ToggleMusic()
    {
        PlaySFX(clickSound);
        isMusicMuted = !isMusicMuted;
        if (bgmSource != null) bgmSource.mute = isMusicMuted;
    }

    public void ToggleSFX()
    {
        isSFXMuted = !isSFXMuted;
        if (sfxSource != null) sfxSource.mute = isSFXMuted;
        if (!isSFXMuted) PlaySFX(clickSound);
    }

    public void ToggleVibration()
    {
        PlaySFX(clickSound);
        isVibrationOff = !isVibrationOff;
        if (!isVibrationOff) Handheld.Vibrate();
    }

    public void RestartFromSettings()
    {
        CloseSettings();
        RestartGame();
    }

    public void ExitToMainMenu()
    {
        PlaySFX(clickSound);
        Time.timeScale = 1f;

        CloseSettings();
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (winScreenUI != null) winScreenUI.SetActive(false);
        if (topBarPanel != null) topBarPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);
        if (nextLevelButton != null) nextLevelButton.SetActive(false);

        if (currentBoardInstance != null)
        {
            Destroy(currentBoardInstance);
            currentBoardInstance = null;
            currentBoardView = null;
        }

        if (mainMenuUI != null) mainMenuUI.SetActive(true);
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex >= levelPrefabs.Length)
        {
            Debug.LogError($"[GameManager] Hết màn! levelIndex={levelIndex} >= {levelPrefabs.Length}");
            return;
        }

        currentLevel = levelIndex;
        if (levelTitleText != null) levelTitleText.text = "MÀN " + (currentLevel + 1);

        lives = 3;
        placedMonstersCount = 0;
        isGameOver = false;

        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (winScreenUI != null) winScreenUI.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);
        if (nextLevelButton != null) nextLevelButton.SetActive(false);

        // heartIcons không dùng nữa
        // for (int i = 0; i < heartIcons.Length; i++)
        // {
        //     if (heartIcons[i] != null) heartIcons[i].SetActive(true);
        // }
        // PlayHeartEntranceAnimation();

        if (livesCountText != null) livesCountText.text = "x" + lives;

        if (currentBoardInstance != null)
        {
            Destroy(currentBoardInstance);
        }

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
        bool[,] parsedSolution;
        try
        {
            parsedGrid = LevelTextParser.Parse(textFile.text, out currentRows, out currentCols, out parsedSolution, out timeLimitSeconds);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GameManager] Lỗi load level: {ex.Message}");
            return; // hoặc xử lý fallback phù hợp — không để game crash/treo
        }

        bool isBoardValid = currentBoardView.InitializeBoard(this, parsedGrid, currentRows, currentCols);
        if (!isBoardValid) return;

        currentTime = timeLimitSeconds;
        isTimerRunning = true;

        gridData = parsedGrid;
        solutionCells = parsedSolution;
        placedMonsters = new int[currentRows, currentCols];
        cellMarks = new int[currentRows, currentCols];
        errorCells = new int[currentRows, currentCols];

        for (int r = 0; r < currentRows; r++)
        {
            for (int c = 0; c < currentCols; c++)
            {
                placedMonsters[r, c] = 0;
                cellMarks[r, c] = 0;
                errorCells[r, c] = 0;
            }
        }
    }

    public void HandleCellClick(int row, int col)
    {
        if (isGameOver) return;
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
        if (targetCell == null) return;

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
        if (targetCell == null) return;

        if (IsValidPlacement(row, col, biomeID))
        {
            placedMonsters[row, col] = 1;
            cellMarks[row, col] = 0;

            targetCell.SetMonsterState(true, GetMonsterSprite(biomeID), GetBiomeColor(biomeID));

            placedMonstersCount++;
            AddScore(100);
            PlaySFX(placeMonsterSound);

            if (placedMonstersCount >= CountTotalSolutionCells()) GameWin();
        }
        else
        {
            lives--;

            if (livesCountText != null) livesCountText.text = "x" + lives;

            PlaySFX(errorSound);
            if (!isVibrationOff) Handheld.Vibrate();

            targetCell.ShowErrorSprite(brokenHeartSprite);
            if (errorCells != null) errorCells[row, col] = 1;

            if (lives <= 0) GameOver();
        }
    }

    void RemoveMonster(int row, int col)
    {
        placedMonsters[row, col] = 0;
        placedMonstersCount--;

        BoardCell targetCell = currentBoardView.GetCell(row, col, currentCols);
        if (targetCell != null)
        {
            targetCell.SetMonsterState(false, null, GetBiomeColor(gridData[row, col]));
        }

        AddScore(-100);
    }

    void GameOver()
    {
        isGameOver = true;
        isTimerRunning = false;
        PlaySFX(loseSound);

        Sequence gameOverSeq = DOTween.Sequence().SetUpdate(true); 

        
        if (heartIcons != null && heartIcons.Length > 0 && heartIcons[0] != null)
        {
            GameObject lastHeart = heartIcons[0];
            lastHeart.transform.DOKill();
            
            gameOverSeq.Append(lastHeart.transform.DOScale(1.3f, 0.15f))
                       .Append(lastHeart.transform.DOShakeRotation(0.4f, new Vector3(0, 0, 25), 15, 90, false))
                       .Append(lastHeart.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack))
                       .AppendCallback(() => lastHeart.SetActive(false));
        }

        
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

     
        for (int r = 0; r < currentRows; r++)
        {
            for (int c = 0; c < currentCols; c++)
            {
                if (placedMonsters != null && placedMonsters[r, c] == 1)
                {
                    BoardCell cell = currentBoardView.GetCell(r, c, currentCols);
                    if (cell != null && cell.monsterSprite != null)
                    {
                        gameOverSeq.Insert(0, cell.monsterSprite.DOColor(Color.gray, 0.4f).SetUpdate(true));
                    }
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
        if (gameOverUI != null) 
        {
            gameOverUI.SetActive(true);
            
            if (gameOverCanvasGroup != null)
            {
             
                gameOverCanvasGroup.alpha = 0f;
                gameOverUI.transform.localScale = Vector3.one * 1.1f;

                gameOverCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.OutQuad).SetUpdate(true);
                gameOverUI.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutQuad).SetUpdate(true);
            }
            else
            {
                
                ShowPopupScale(gameOverUI);
            }
        }
        
        if (restartButton != null) restartButton.SetActive(true);
    }

    void GameWin()
    {
        isGameOver = true;
        isTimerRunning = false;
        int bonusScore = 500 + (lives * 100);
        AddScore(bonusScore);
        if (winScreenUI != null) 
        {
            winScreenUI.SetActive(true);
            ShowPopupScale(winScreenUI);
        }
        if (restartButton != null) restartButton.SetActive(false);
        if (nextLevelButton != null) nextLevelButton.SetActive(true);
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
        Time.timeScale = 1f; // Bắt buộc reset TimeScale khi Restart
        currentScore = 0;
        UpdateScoreUI();

        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (winScreenUI != null) winScreenUI.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);
        if (nextLevelButton != null) nextLevelButton.SetActive(false);
        if (mainMenuUI != null) mainMenuUI.SetActive(false);

        if (topBarPanel != null) topBarPanel.SetActive(true);

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
        if (scoreText != null)
        {
            scoreText.transform.DOKill(true);
            DOTween.To(() => displayedScore, x => { 
                displayedScore = x; 
                scoreText.text = "ĐIỂM: " + x.ToString(); 
            }, currentScore, 0.4f).SetEase(Ease.OutQuad);
            scoreText.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0), 0.3f, 2, 0.5f);
        }
    }

    bool IsValidPlacement(int targetRow, int targetCol, int targetBiomeID)
    {
        // Đã chuyển sang kiểm tra theo đáp án cố định (solutionCells) thay vì
        // check rule hàng/cột/vùng/kề cạnh như trước — người thiết kế level
        // phải tự đảm bảo đáp án tuân đúng luật khi viết file text.
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


    // ─────────────────────────────────────────────────────────────────────
    // HEART ANIMATIONS
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Pop-in staggered → rồi chạy heartbeat idle liên tục.
    /// Gọi sau mỗi lần LoadLevel (hearts đã SetActive(true) trước đó).
    /// </summary>
    private void PlayHeartEntranceAnimation()
    {
        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] == null || !heartIcons[i].activeSelf) continue;

            Transform t = heartIcons[i].transform;
            int idx = i; // capture cho lambda

            // Kill tween cũ (idle cũ từ màn trước nếu có)
            t.DOKill();
            t.localScale = Vector3.zero;

            // Pop-in: scale 0 → 1.3 (overshoot) → 1.0, delay theo thứ tự
            t.DOScale(1f, 0.45f)
             .SetEase(Ease.OutBack)
             .SetDelay(idx * 0.12f)
             .OnComplete(() => PlayHeartIdleLoop(heartIcons[idx].transform));
        }
    }

    /// <summary>
    /// Heartbeat idle: 2 nhịp đập nhanh rồi nghỉ, lặp vô tận.
    /// Pattern: thu nhỏ → phình to nhanh (nhịp 1) → to nhanh (nhịp 2) → về bình thường → chờ.
    /// </summary>
    private void PlayHeartIdleLoop(Transform heartTrans)
    {
        if (heartTrans == null) return;

        heartTrans.DOKill();

        Sequence beat = DOTween.Sequence();

        // Nhịp 1 — đập mạnh
        beat.Append(heartTrans.DOScale(1.20f, 0.13f).SetEase(Ease.OutQuad));
        beat.Append(heartTrans.DOScale(1.00f, 0.12f).SetEase(Ease.InQuad));

        // Nhịp 2 — đập nhẹ hơn (như tim thật)
        beat.Append(heartTrans.DOScale(1.10f, 0.10f).SetEase(Ease.OutQuad));
        beat.Append(heartTrans.DOScale(1.00f, 0.11f).SetEase(Ease.InQuad));

        // Nghỉ giữa các chu kỳ
        beat.AppendInterval(0.85f);

        // Lặp vô tận
        beat.SetLoops(-1, LoopType.Restart);
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