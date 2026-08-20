#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;


public static class LevelSolverEditorTool
{
    [MenuItem("Tools/Monster Biome/Check Level Solvability")]
    public static void CheckAllLevels()
    {
    
        string levelsFolder = Path.Combine(Application.dataPath, "Levels Data");

        if (!Directory.Exists(levelsFolder))
        {
            Debug.LogError($"[LevelSolverEditorTool] Không tìm thấy thư mục '{levelsFolder}'!");
            return;
        }

        string[] txtFiles = Directory.GetFiles(levelsFolder, "*.txt", SearchOption.AllDirectories);

        if (txtFiles.Length == 0)
        {
            Debug.LogWarning("[LevelSolverEditorTool] Không tìm thấy file .txt nào trong thư mục Levels Data!");
            return;
        }

        Debug.Log($"[LevelSolverEditorTool] ═══ BẮT ĐẦU KIỂM TRA {txtFiles.Length} FILE LEVEL ═══");

        int countOK = 0;
        int countNoSolution = 0;
        int countMultipleSolutions = 0;
        int countParseError = 0;

        foreach (string filePath in txtFiles)
        {
            string fileName = Path.GetFileName(filePath);
            string content = File.ReadAllText(filePath);

            int rows, cols;
            int[,] grid;


            try
            {
                grid = LevelTextParser.Parse(content, out rows, out cols);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LevelSolverEditorTool] ❌ PARSE LỖI: {fileName} — {ex.Message}");
                countParseError++;
                continue;
            }

            var solutions = LevelSolver.Solve(grid, rows, cols, maxSolutionsToFind: 2);

            if (solutions.Count == 0)
            {
                Debug.LogError($"[LevelSolverEditorTool] ❌ VÔ NGHIỆM: {fileName} ({rows}x{cols}) — Level này KHÔNG THỂ chơi được!");
                countNoSolution++;
            }
            else if (solutions.Count >= 2)
            {
                string sol1 = FormatSolution(solutions[0]);
                string sol2 = FormatSolution(solutions[1]);
                Debug.LogError($"[LevelSolverEditorTool] ⚠️ NHIỀU NGHIỆM: {fileName} ({rows}x{cols}) — Puzzle không rõ ràng!\n" +
                               $"  [Nghiệm 1]: {sol1}  |  [Nghiệm 2]: {sol2}");
                countMultipleSolutions++;
            }
            else
            {
                string sol = FormatSolution(solutions[0]);
                Debug.Log($"[LevelSolverEditorTool] ✅ OK: {fileName} ({rows}x{cols}) — Duy nhất 1 nghiệm: {sol}");
                countOK++;
            }
        }

  
        Debug.Log($"[LevelSolverEditorTool] ═══ KẾT QUẢ TỔNG HỢP ═══\n" +
                  $"  ✅ OK (1 nghiệm duy nhất): {countOK}\n" +
                  $"  ⚠️ Nhiều nghiệm:           {countMultipleSolutions}\n" +
                  $"  ❌ Vô nghiệm:              {countNoSolution}\n" +
                  $"  ❌ Lỗi parse:              {countParseError}\n" +
                  $"  Tổng số file:              {txtFiles.Length}");

        if (countNoSolution > 0 || countMultipleSolutions > 0 || countParseError > 0)
        {
            Debug.LogError("[LevelSolverEditorTool] ⛔ CÓ LEVEL CẦN SỬA! Xem chi tiết từng file ở trên.");
        }
        else
        {
            Debug.Log("[LevelSolverEditorTool] 🎉 Tất cả level đều OK — sẵn sàng phát hành!");
        }
    }


    private static string FormatSolution(System.Collections.Generic.List<(int row, int col)> solution)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < solution.Count; i++)
        {
            if (i > 0) sb.Append(" ");
            sb.Append($"(r{solution[i].row},c{solution[i].col})");
        }
        return sb.ToString();
    }
}
#endif
