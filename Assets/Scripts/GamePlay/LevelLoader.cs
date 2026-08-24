using System;
using UnityEngine;
using TMPro;
using MonsterBiome.Core.Parsers;
using MonsterBiome.Core.Algorithms;
using MonsterBiome.Core.Models;

public class LevelLoader : MonoBehaviour
{
    [Header("Level Configuration")]
    public GameObject[] levelPrefabs;
    public Transform boardContainer;
    public TextMeshProUGUI levelTitleText;

    public void ClearCurrentBoard(ref GameObject boardInstance, ref LevelBoardView boardView)
    {
        if (boardInstance != null)
        {
            Destroy(boardInstance);
            boardInstance = null;
            boardView = null;
        }
    }

    public bool LoadLevel(int levelIndex, GameManager gm, out BoardState boardState, out LevelBoardView boardView, out GameObject boardInstance)
    {
        boardState = null;
        boardView = null;
        boardInstance = null;

        if (levelPrefabs == null || levelIndex < 0 || levelIndex >= levelPrefabs.Length)
        {
            Debug.LogError($"[LevelLoader] Hết màn hoặc levelPrefabs chưa gán! levelIndex={levelIndex}");
            return false;
        }

        if (levelTitleText)
        {
            levelTitleText.text = "MÀN " + (levelIndex + 1);
        }

        boardInstance = Instantiate(levelPrefabs[levelIndex], boardContainer);
        boardInstance.transform.localPosition = Vector3.zero;
        boardInstance.transform.localScale = Vector3.one;

        boardView = boardInstance.GetComponent<LevelBoardView>();
        if (boardView == null)
        {
            Debug.LogError("[LevelLoader] Prefab màn chơi thiếu script LevelBoardView!");
            return false;
        }

        TextAsset textFile = boardView.levelTextFile;
        if (textFile == null)
        {
            Debug.LogError("[LevelLoader] Level Text File chưa được gán vào LevelBoardView!");
            return false;
        }

        int rows, cols;
        int[,] parsedGrid;
        try
        {
            parsedGrid = LevelTextParser.Parse(textFile.text, out rows, out cols);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LevelLoader] Lỗi load level: {ex.Message}");
            return false;
        }

        var solutions = LevelSolver.Solve(parsedGrid, rows, cols, maxSolutionsToFind: 2);

        if (solutions.Count == 0)
        {
            Debug.LogError($"[LevelLoader] Level {levelIndex} VÔ NGHIỆM — không thể chơi được! Cần sửa lại file text level này.");
            return false;
        }

        if (solutions.Count > 1)
        {
            Debug.LogError($"[LevelLoader] Level {levelIndex} CÓ NHIỀU NGHIỆM — đáp án không duy nhất, người chơi sẽ bị tính sai. Sửa file level (kiểm tra bằng Tools/Monster Biome/Check Level Solvability).");
            return false;
        }

        bool isBoardValid = boardView.InitializeBoard(gm, parsedGrid, rows, cols);
        if (!isBoardValid) return false;

        boardState = new BoardState();
        boardState.Initialize(parsedGrid, solutions[0], rows, cols);

        return true;
    }
}
