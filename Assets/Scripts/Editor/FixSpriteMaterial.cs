using UnityEngine;
using UnityEditor;

/// <summary>
/// Đổi material của tất cả SpriteRenderer trong CellPrefab và Level Prefabs
/// từ Sprite-Lit-Default sang Sprite-Unlit-Default.
/// </summary>
public class FixSpriteMaterial : EditorWindow
{
    [MenuItem("Monster Biome/Fix Material: Lit -> Unlit (All Board Prefabs)")]
    public static void FixMaterials()
    {
        // Tìm material Sprite-Unlit-Default
        Material unlitMat = AssetDatabase.LoadAssetAtPath<Material>(
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");

        if (unlitMat == null)
        {
            // Tìm bằng tên trong toàn bộ project
            string[] matGuids = AssetDatabase.FindAssets("Sprite-Unlit-Default t:Material");
            if (matGuids.Length > 0)
            {
                string matPath = AssetDatabase.GUIDToAssetPath(matGuids[0]);
                unlitMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                Debug.Log($"[FixSpriteMaterial] Tìm thấy Unlit material tại: {matPath}");
            }
        }

        if (unlitMat == null)
        {
            EditorUtility.DisplayDialog("Lỗi!", 
                "Không tìm thấy material 'Sprite-Unlit-Default'!\n" +
                "Hãy tìm thủ công và thay thế material trong CellPrefab.", 
                "OK");
            return;
        }

        string prefabFolder = "Assets/Prefab";
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabFolder });

        int fixedRenderers = 0;
        int fixedPrefabs = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            // Chỉ xử lý LevelBoardView prefabs và CellPrefab
            bool isLevelPrefab = prefab.GetComponent<LevelBoardView>() != null;
            bool isCellPrefab  = prefab.GetComponent<BoardCell>() != null;

            if (!isLevelPrefab && !isCellPrefab) continue;

            using (var editScope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                GameObject root = editScope.prefabContentsRoot;
                bool modified = false;

                SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (SpriteRenderer sr in renderers)
                {
                    if (sr.sharedMaterial != null && sr.sharedMaterial.name.Contains("Lit"))
                    {
                        Debug.Log($"[FixSpriteMaterial] {prefab.name}/{sr.name}: '{sr.sharedMaterial.name}' -> 'Sprite-Unlit-Default'");
                        sr.sharedMaterial = unlitMat;
                        modified = true;
                        fixedRenderers++;
                    }
                }

                if (modified) fixedPrefabs++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string message = fixedRenderers > 0
            ? $"✅ Đã đổi {fixedRenderers} SpriteRenderer trong {fixedPrefabs} Prefab\nTừ Lit → Unlit-Default"
            : "⚠️ Không tìm thấy SpriteRenderer nào dùng Lit material.\nHãy kiểm tra thủ công CellPrefab.";

        EditorUtility.DisplayDialog("Fix Material - Done!", message, "OK");
        Debug.Log($"FixSpriteMaterial: Done! fixedRenderers={fixedRenderers}, fixedPrefabs={fixedPrefabs}");
    }
}
