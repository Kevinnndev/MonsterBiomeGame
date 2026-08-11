using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int currentLevel = 0;

    // --- DANH SÁCH CÁC FILE MÀN CHƠI SẼ ĐƯỢC KÉO THẢ TỪ NGOÀI VÀO ---
    [Header("Level Files")]
    public LevelData[] levelFiles;

    public int[,] gridData = new int[5, 5];
    public int[,] placedMonsters = new int[5, 5];

    [Header("UI References")]
    public Transform gameBoard;
    public Color[] biomeColors;
    public Sprite[] monsterSprites;

    [Header("Lives System")]
    public int lives = 3;
    public GameObject[] heartIcons;
    private bool isGameOver = false;

    [Header("Game Over & Win UI")]
    public GameObject restartButton;
    public GameObject winScreenUI;
    public GameObject nextLevelButton;
    private int placedMonstersCount = 0;

    void Start()
    {
        LoadLevel(currentLevel);
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex >= levelFiles.Length)
        {
            Debug.Log("<color=cyan>CHÚC MỪNG! BẠN ĐÃ PHÁ ĐẢO!</color>");
            return;
        }

        currentLevel = levelIndex;
        lives = 3;
        placedMonstersCount = 0;
        isGameOver = false;

        winScreenUI.SetActive(false);
        restartButton.SetActive(false);
        if (nextLevelButton != null) nextLevelButton.SetActive(false);

        for (int i = 0; i < heartIcons.Length; i++)
        {
            heartIcons[i].SetActive(true);
        }

        // --- ĐỔ DỮ LIỆU TỪ FILE SCRIPTABLE OBJECT VÀO GAME ---
        LevelData currentData = levelFiles[currentLevel];

        for (int r = 0; r < 5; r++)
        {
            for (int c = 0; c < 5; c++)
            {
                // Lấy ID từ hàng r, cột c của file dữ liệu
                gridData[r, c] = currentData.rows[r].columns[c];
                placedMonsters[r, c] = 0;
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

                // Xử lý màu sắc cho ô đất đá (ID = 0) hoặc ô sinh thái thông thường
                Button btn = cell.GetComponent<Button>();
                Image cellImage = cell.GetComponent<Image>();

                if (biomeID == 0)
                {
                    // Ô đá: Tô màu xám tối và VÔ HƯỚNG NÚT BẤM (không cho click vào ô đá)
                    cellImage.color = new Color(0.4f, 0.4f, 0.4f, 1f);
                    btn.interactable = false;
                }
                else
                {
                    // Ô sinh thái bình thường
                    Color fixedColor = biomeColors[biomeID];
                    fixedColor.a = 1f;
                    cellImage.color = fixedColor;
                    btn.interactable = true;
                }

                Transform monsterIcon = cell.GetChild(0);
                monsterIcon.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnCellClicked(row, col, biomeID));
            }
        }
    }

    public void OnCellClicked(int row, int col, int biomeID)
    {
        if (isGameOver || placedMonsters[row, col] == 1) return;

        if (IsValidPlacement(row, col, biomeID))
        {
            placedMonsters[row, col] = 1;
            int cellIndex = (row * 5) + col;
            Transform monsterIcon = gameBoard.GetChild(cellIndex).GetChild(0);

            Image iconImage = monsterIcon.GetComponent<Image>();
            iconImage.sprite = monsterSprites[biomeID];
            iconImage.color = new Color(1f, 1f, 1f, 1f);

            placedMonstersCount++;
            if (placedMonstersCount >= 5) GameWin();
        }
        else
        {
            lives--;
            heartIcons[lives].SetActive(false);
            if (lives <= 0) GameOver();
        }
    }

    void GameOver()
    {
        isGameOver = true;
        restartButton.SetActive(true);
    }

    void GameWin()
    {
        isGameOver = true;
        winScreenUI.SetActive(true);
        restartButton.SetActive(false); // Giấu nút chơi lại
        if (nextLevelButton != null) nextLevelButton.SetActive(true);
    }

    public void NextLevel()
    {
        LoadLevel(currentLevel + 1);
    }

    public void RestartGame()
    {
        LoadLevel(currentLevel);
    }

    bool IsValidPlacement(int targetRow, int targetCol, int targetBiomeID)
    {
        // Chặn tuyệt đối không cho đặt vào ô số 0
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