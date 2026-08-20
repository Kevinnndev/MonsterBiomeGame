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
        EditorGUILayout.LabelField("Kiểm tra Level", EditorStyles.boldLabel);

        if (GUILayout.Button("Kiểm tra tính hợp lệ ", GUILayout.Height(30)))
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
                EditorUtility.DisplayDialog("Lỗi Kích Thước", $"Board không phải hình vuông ({rows}x{cols}). Level phải có số hàng bằng số cột!", "OK");
                return;
            }


            var solutions = LevelSolver.Solve(grid, rows, cols, maxSolutionsToFind: 200);

            string message;
            if (solutions.Count == 0)
            {
                message = "❌ VÔ NGHIỆM — level này không thể chơi được. Cần vẽ lại vùng biome.";
            }
            else if (solutions.Count == 1)
            {
                message = "✅ DUY NHẤT 1 NGHIỆM — level hợp lệ, sẵn sàng sử dụng.";
            }
            else
            {
                string suffix = solutions.Count >= 200 ? "+ (dừng đếm ở giới hạn 200)" : "";
                message = $"⚠️ CÓ {solutions.Count}{suffix} NGHIỆM — puzzle không rõ ràng, " +
                          "cần chỉnh lại hình dạng vùng biome để thu hẹp còn đúng 1 đáp án.";
            }

            EditorUtility.DisplayDialog($"Kết quả kiểm tra — {textFile.name}", message, "OK");
            Debug.Log($"[LevelCheck] {textFile.name}: {message}");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Lỗi Parse", $"File text lỗi định dạng:\n{e.Message}", "OK");
        }
    }
}
