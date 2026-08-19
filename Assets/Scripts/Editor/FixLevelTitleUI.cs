using UnityEngine;
using UnityEditor;
using TMPro;

public class FixLevelTitleUI : EditorWindow
{
    [MenuItem("Monster Biome/Fix Level Title Text")]
    public static void FixText()
    {
        // Tìm LevelTitleText trong scene
        GameObject textObj = GameObject.Find("LevelTitleText");
        
        if (textObj == null)
        {
            Debug.LogError("Không tìm thấy LevelTitleText! Bạn hãy chắc chắn tên của nó ghi đúng chữ hoa chữ thường.");
            return;
        }

        // Tìm TopBarPanel để nhét nó vào
        GameObject topBar = GameObject.Find("TopBarPanel");
        if (topBar != null)
        {
            textObj.transform.SetParent(topBar.transform, false);
            Debug.Log("Đã chuyển LevelTitleText vào trong TopBarPanel.");
        }

        // Lấy component TextMeshProUGUI
        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            // Đổi màu thành ĐEN để dễ nhìn trên nền trắng
            tmp.color = Color.black;
            
            // Chỉnh kích thước và căn giữa
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 60;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 20;
            tmp.fontSizeMax = 60;
            Debug.Log("Đã đổi màu chữ thành Đen và tự động chỉnh cỡ chữ.");
        }

        // Chỉnh lại RectTransform cho nó nằm giữa thanh TopBar
        RectTransform rect = textObj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, 0); // Nằm chính giữa TopBar
            rect.sizeDelta = new Vector2(400, 100);    // Khung rộng rãi để không bị mất chữ
            Debug.Log("Đã căn giữa chữ vào TopBarPanel thành công.");
        }

        EditorUtility.DisplayDialog(
            "Sửa lỗi chữ Level", 
            "Đã tự động chỉnh sửa: Màu chữ đen, kích thước chuẩn, và nhét gọn vào TopBar!\n\nHãy bấm Play để xem thành quả.", 
            "OK"
        );
    }
}
