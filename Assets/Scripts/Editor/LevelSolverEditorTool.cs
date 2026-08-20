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
            Debug.LogError($"[LevelSolverEditorTool] Thư mục '{levelsFolder}' không tồn tại!");
            return;
        }

        string[] txtFiles = Directory.GetFiles(levelsFolder, "*.txt", SearchOption.AllDirectories);

        if (txtFiles.Length == 0)
        {
            Debug.LogWarning("[LevelSolverEditorTool] Không tìm thấy file .txt nào!");
            return;
        }

        Debug.Log($"[LevelSolverEditorTool] Kiểm tra {txtFiles.Length} file level...");

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
                Debug.LogError($"[LevelSolverEditorTool] LỖI PARSE: {fileName} — {ex.Message}");
                countParseError++;
                continue;
            }

            var solutions = LevelSolver.Solve(grid, rows, cols, maxSolutionsToFind: 2);

            if (solutions.Count == 0)
            {
                Debug.LogError($"[LevelSolverEditorTool] VÔ NGHIỆM: {fileName} ({rows}x{cols})");
                countNoSolution++;
            }
            else if (solutions.Count >= 2)
            {
                string sol1 = FormatSolution(solutions[0]);
                string sol2 = FormatSolution(solutions[1]);
                Debug.LogError($"[LevelSolverEditorTool] TRÙNG NGHIỆM: {fileName} ({rows}x{cols})\n" +
                               $"  [Nghiệm 1]: {sol1}  |  [Nghiệm 2]: {sol2}");
                countMultipleSolutions++;
            }
            else
            {
                string sol = FormatSolution(solutions[0]);
                Debug.Log($"[LevelSolverEditorTool] OK: {fileName} ({rows}x{cols}) — {sol}");
                countOK++;
            }
        }

        Debug.Log($"[LevelSolverEditorTool] KẾT QUẢ: OK={countOK}, Trùng nghiệm={countMultipleSolutions}, Vô nghiệm={countNoSolution}, Lỗi parse={countParseError}");

        if (countNoSolution > 0 || countMultipleSolutions > 0 || countParseError > 0)
        {
            Debug.LogError("[LevelSolverEditorTool] Có level cần kiểm tra lại.");
        }
        else
        {
            Debug.Log("[LevelSolverEditorTool] Tất cả level đều hợp lệ.");
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
