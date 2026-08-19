using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Editor tool: Xóa các UI component thừa (Image, Canvas Renderer, Grid Layout Group)
/// khỏi tất cả Level Prefab để chúng hoạt động đúng trong World Space.
/// </summary>
public class CleanLevelPrefabs : EditorWindow
{
    [MenuItem("Monster Biome/Clean Level Prefabs (Remove UI Components)")]
    public static void CleanPrefabs()
    {
        string prefabFolder = "Assets/Prefab";
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabFolder });

        int cleaned = 0;
        int skipped = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            // Chỉ xử lý các prefab có LevelBoardView (tức là Level Prefab)
            LevelBoardView boardView = prefab.GetComponent<LevelBoardView>();
            if (boardView == null)
            {
                skipped++;
                continue;
            }

            // Dùng PrefabUtility để chỉnh sửa prefab asset trực tiếp
            using (var editScope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                GameObject root = editScope.prefabContentsRoot;
                bool modified = false;

                // Xóa Grid Layout Group
                GridLayoutGroup grid = root.GetComponent<GridLayoutGroup>();
                if (grid != null)
                {
                    Object.DestroyImmediate(grid);
                    modified = true;
                    Debug.Log($"[{prefab.name}] Removed: GridLayoutGroup");
                }

                // Xóa Image
                Image img = root.GetComponent<Image>();
                if (img != null)
                {
                    Object.DestroyImmediate(img);
                    modified = true;
                    Debug.Log($"[{prefab.name}] Removed: Image");
                }

                // Xóa Canvas Renderer (sau khi Image bị xóa)
                CanvasRenderer cr = root.GetComponent<CanvasRenderer>();
                if (cr != null)
                {
                    Object.DestroyImmediate(cr);
                    modified = true;
                    Debug.Log($"[{prefab.name}] Removed: CanvasRenderer");
                }

                if (modified) cleaned++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Clean Level Prefabs - Done!",
            $"✅ Đã dọn dẹp: {cleaned} Level Prefab\n⏭️ Bỏ qua: {skipped} Prefab khác",
            "OK"
        );

        Debug.Log($"CleanLevelPrefabs: Done! Cleaned={cleaned}, Skipped={skipped}");
    }
}
