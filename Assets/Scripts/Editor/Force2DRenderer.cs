using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Force2DRenderer : EditorWindow
{
    [MenuItem("Monster Biome/Fix Rendering (Force 2D Renderer)")]
    public static void Force2D()
    {
        // 1. Tìm asset UniversalRP.asset
        string[] guids = AssetDatabase.FindAssets("UniversalRP t:UniversalRenderPipelineAsset");
        if (guids.Length == 0)
        {
            Debug.LogError("Không tìm thấy UniversalRP.asset!");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        UniversalRenderPipelineAsset urpAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);

        // 2. Gán vào Graphics Settings
        GraphicsSettings.defaultRenderPipeline = urpAsset;
        
        // 3. Gán vào Quality Settings (để chắc chắn không bị ghi đè)
        QualitySettings.renderPipeline = urpAsset;

        Debug.Log($"[Fix Rendering] Đã chuyển thành công Render Pipeline sang: {urpAsset.name} (Hỗ trợ 2D)");
        
        EditorUtility.DisplayDialog(
            "Fix Rendering", 
            "Đã ép Unity dùng 2D Renderer thành công!\nHãy Play game, Board chắc chắn sẽ hiện.", 
            "OK"
        );
    }
}
