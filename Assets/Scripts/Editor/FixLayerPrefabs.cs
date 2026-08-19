using UnityEngine;
using UnityEditor;

/// <summary>
/// Đổi Layer của tất cả Level Prefab và CellPrefab từ "UI" về "Default"
/// để camera có thể render chúng trong World Space.
/// </summary>
public class FixLayerPrefabs : EditorWindow
{
    [MenuItem("Monster Biome/Fix Layer: UI -> Default (All Board Prefabs)")]
    public static void FixLayers()
    {
        string prefabFolder = "Assets/Prefab";
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabFolder });

        int fixedPrefabs = 0;
        int fixedObjects = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            // Chỉ xử lý LevelPrefab (có LevelBoardView) và CellPrefab (có BoardCell)
            bool isLevelPrefab  = prefab.GetComponent<LevelBoardView>() != null;
            bool isCellPrefab   = prefab.GetComponent<BoardCell>() != null;

            if (!isLevelPrefab && !isCellPrefab) continue;

            using (var editScope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                GameObject root = editScope.prefabContentsRoot;
                bool modified = false;

                // Đổi layer của root và tất cả child
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (t.gameObject.layer != LayerMask.NameToLayer("Default"))
                    {
                        Debug.Log($"[FixLayer] {prefab.name}/{t.name}: layer '{LayerMask.LayerToName(t.gameObject.layer)}' -> 'Default'");
                        t.gameObject.layer = LayerMask.NameToLayer("Default");
                        modified = true;
                        fixedObjects++;
                    }
                }

                if (modified) fixedPrefabs++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Fix Layer - Done!",
            $"✅ Đã sửa {fixedObjects} object trong {fixedPrefabs} Prefab\nTất cả đã được đặt về Layer: Default",
            "OK"
        );

        Debug.Log($"FixLayerPrefabs: Done! fixedObjects={fixedObjects}, fixedPrefabs={fixedPrefabs}");
    }
}
