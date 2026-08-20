using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelBoardView))]
public class LevelBoardViewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelBoardView boardView = (LevelBoardView)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Kiểm Tra Level", EditorStyles.boldLabel);

        if (GUILayout.Button("Kiểm Tra Tính Hợp Lệ", GUILayout.Height(30)))
        {
            CheckLevelValidity(boardView);
        }
    }

    private void CheckLevelValidity(LevelBoardView boardView)
    {
        TextAsset textFile = boardView.levelTextFile;

        if (textFile == null)
        {
            EditorUtility.DisplayDialog("Lỗi", "Chưa gán file Level Text cho prefab này.", "OK");
            return;
        }

        try
        {
            int[,] grid = LevelTextParser.Parse(textFile.text, out int rows, out int cols);

            if (rows != cols)
            {
                EditorUtility.DisplayDialog("Lỗi Kích Thước", $"Board không phải hình vuông ({rows}x{cols}).", "OK");
                return;
            }

            var solutions = LevelSolver.Solve(grid, rows, cols, maxSolutionsToFind: 200);

            string message;
            if (solutions.Count == 0)
            {
                message = "Vô nghiệm — Level này không thể giải được. Cần chỉnh lại vùng Biome.";
            }
            else if (solutions.Count == 1)
            {
                message = "Duy nhất 1 nghiệm — Level hợp lệ.";
            }
            else
            {
                string suffix = solutions.Count >= 200 ? "+" : "";
                message = $"Có {solutions.Count}{suffix} nghiệm — Level chưa tối ưu (trùng nghiệm).";
            }

            EditorUtility.DisplayDialog($"Kết Quả — {textFile.name}", message, "OK");
            Debug.Log($"[LevelCheck] {textFile.name}: {message}");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Lỗi Parse", $"Không thể đọc file text:\n{e.Message}", "OK");
        }
    }
}
