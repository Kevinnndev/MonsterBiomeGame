using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Level Configuration")]
    public int currentLevel = 0;
    public LevelData[] levelFiles;
    public TextMeshProUGUI levelTitleText;

    [Header("Board Data")]
    public int[,] gridData = new int[5, 5];
    public int[,] placedMonsters = new int[5, 5];
    public int[,] cellMarks = new int[5, 5];

    [Header("UI References")]
    public Transform gameBoard;
    public Color[] biomeColors;
    public Sprite[] monsterSprites;
    public GameObject mainMenuUI;
    public GameObject settingsPanel;
    public GameObject gameOverUI;
    public GameObject winScreenUI;
    public GameObject restartButton;
    public GameObject nextLevelButton;

    [Header("Lives & Score System")]
    public int lives = 3;
    public GameObject[] heartIcons;
    public TextMeshProUGUI scoreText;
    private int currentScore = 0;

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
    private int placedMonstersCount = 0;
    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f;
    private int lastRow = -1;
    private int lastCol = -1;

    void Start()
    {
        if (mainMenuUI != null) mainMenuUI.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (winScreenUI != null) winScreenUI.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);
        if (nextLevelButton != null) nextLevelButton.SetActive(false);
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
        currentLevel = 0;
        currentScore = 0;
        UpdateScoreUI();
        LoadLevel(currentLevel);
    }

    public void OpenSettings()
    {
        PlaySFX(clickSound);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        PlaySFX(clickSound);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void ToggleMusic()
    {
        PlaySFX(clickSound);
        isMusicMuted = !isMusicMuted;
        if (bgmSource != null)
        {
            bgmSource.mute = isMusicMuted;
        }
    }

    public void RestartFromSettings()
    {
        CloseSettings();
        RestartGame();
    }

    public void ExitToMainMenu()
    {
        PlaySFX(clickSound);
        CloseSettings();
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (winScreenUI != null) winScreenUI.SetActive(false);
        if (mainMenuUI != null) mainMenuUI.SetActive(true);
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex >= levelFiles.Length) return;

        currentLevel = levelIndex;
        if (levelTitleText != null) levelTitleText.text = "MÀN " + (currentLevel + 1);

        lives = 3;
        placedMonstersCount = 0;
        isGameOver = false;

        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (winScreenUI != null) winScreenUI.SetActive(false);
        if (restartButton != null) restartButton.SetActive(false);
        if (nextLevelButton != null) nextLevelButton.SetActive(false);

        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] != null) heartIcons[i].SetActive(true);
        }

        LevelData currentData = levelFiles[currentLevel];

        for (int r = 0; r < 5; r++)
        {
            for (int c = 0; c < 5; c++)
            {
                gridData[r, c] = currentData.rows[r].columns[c];
                placedMonsters[r, c] = 0;
                cellMarks[r, c] = 0;
            }
        }
        DrawBoard();
    }

    void DrawBoard()
    {
        for (int r = 0; r < 5; r++)
        {
            for (int c = 0; c < 5; c++)
            {
                int row = r;
                int col = c;
                int biomeID = gridData[row, col];

                int cellIndex = (row * 5) + col;
                Transform cell = gameBoard.GetChild(cellIndex);

                Button btn = cell.GetComponent<Button>();
                Image cellImage = cell.GetComponent<Image>();
                TextMeshProUGUI markText = cell.Find("NoteText")?.GetComponent<TextMeshProUGUI>();

                if (biomeID == 0)
                {
                    cellImage.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                    btn.interactable = false;
                    if (markText) markText.text = "";
                }
                else
                {
                    Color fixedColor = biomeColors[biomeID];
                    fixedColor.a = 1f;
                    cellImage.color = fixedColor;
                    btn.interactable = true;
                    if (markText) markText.text = "";
                }

                Transform monsterIcon = cell.GetChild(0);
                monsterIcon.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnCellClickAction(row, col, biomeID));
            }
        }
    }

    void OnCellClickAction(int row, int col, int biomeID)
    {
        if (isGameOver) return;

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
        int cellIndex = (row * 5) + col;
        Transform cell = gameBoard.GetChild(cellIndex);
        TextMeshProUGUI markText = cell.Find("NoteText")?.GetComponent<TextMeshProUGUI>();
        Image cellImage = cell.GetComponent<Image>();

        if (markText != null)
        {
            if (cellMarks[row, col] == 0)
            {
                cellMarks[row, col] = 1;
                markText.text = "X";
                markText.color = new Color(0.8f, 0.2f, 0.2f, 1f);
                Color dimmedColor = biomeColors[biomeID];
                dimmedColor.a = 0.4f;
                cellImage.color = dimmedColor;
            }
            else
            {
                cellMarks[row, col] = 0;
                markText.text = "";
                Color normalColor = biomeColors[biomeID];
                normalColor.a = 1f;
                cellImage.color = normalColor;
            }
        }
    }

    void TryPlaceMonster(int row, int col, int biomeID)
    {
        if (IsValidPlacement(row, col, biomeID))
        {
            placedMonsters[row, col] = 1;
            cellMarks[row, col] = 0;

            int cellIndex = (row * 5) + col;
            Transform cell = gameBoard.GetChild(cellIndex);
            TextMeshProUGUI markText = cell.Find("NoteText")?.GetComponent<TextMeshProUGUI>();
            if (markText) markText.text = "";

            Image cellImage = cell.GetComponent<Image>();
            Color normalColor = biomeColors[biomeID];
            normalColor.a = 1f;
            cellImage.color = normalColor;

            Transform monsterIcon = cell.GetChild(0);
            Image iconImage = monsterIcon.GetComponent<Image>();
            iconImage.sprite = monsterSprites[biomeID];
            iconImage.color = new Color(1f, 1f, 1f, 1f);

            placedMonstersCount++;
            AddScore(100);
            PlaySFX(placeMonsterSound);

            if (placedMonstersCount >= 5) GameWin();
        }
        else
        {
            lives--;
            if (lives >= 0 && heartIcons[lives] != null)
            {
                heartIcons[lives].SetActive(false);
            }
            PlaySFX(errorSound);

            if (lives <= 0) GameOver();
        }
    }

    void RemoveMonster(int row, int col)
    {
        placedMonsters[row, col] = 0;
        placedMonstersCount--;
        int cellIndex = (row * 5) + col;
        Transform cell = gameBoard.GetChild(cellIndex);
        Transform monsterIcon = cell.GetChild(0);
        monsterIcon.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
        AddScore(-100);
    }

    void GameOver()
    {
        isGameOver = true;
        if (gameOverUI != null) gameOverUI.SetActive(true);
        if (restartButton != null) restartButton.SetActive(true);
        PlaySFX(loseSound);
    }

    void GameWin()
    {
        isGameOver = true;
        int bonusScore = 500 + (lives * 100);
        AddScore(bonusScore);
        if (winScreenUI != null) winScreenUI.SetActive(true);
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
        currentScore = 0;
        UpdateScoreUI();
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
            scoreText.text = "ĐIỂM: " + currentScore.ToString();
        }
    }

    bool IsValidPlacement(int targetRow, int targetCol, int targetBiomeID)
    {
        if (targetBiomeID == 0) return false;
        for (int r = 0; r < 5; r++)
        {
            for (int c = 0; c < 5; c++)
            {
                if (placedMonsters[r, c] == 1)
                {
                    if (gridData[r, c] == targetBiomeID || r == targetRow || c == targetCol ||
                       (Mathf.Abs(r - targetRow) <= 1 && Mathf.Abs(c - targetCol) <= 1))
                        return false;
                }
            }
        }
        return true;
    }
}