using UnityEngine;

public class CameraTest : MonoBehaviour
{
    void Start()
    {
        // Tạo một cục màu đỏ to đùng ngay trước camera
        GameObject testObj = new GameObject("TestRedSquare");
        
        // Đặt ngay trước camera (Z = 0)
        testObj.transform.position = new Vector3(0, 0, 0);
        testObj.transform.localScale = new Vector3(10, 10, 1);

        // Thêm Sprite Renderer
        SpriteRenderer sr = testObj.AddComponent<SpriteRenderer>();
        
        // Tạo một texture màu đỏ
        Texture2D tex = new Texture2D(100, 100);
        for (int y = 0; y < 100; y++)
        {
            for (int x = 0; x < 100; x++)
            {
                tex.SetPixel(x, y, Color.red);
            }
        }
        tex.Apply();

        // Ép thành Sprite
        sr.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        sr.color = Color.red;

        // Thử tìm unlit material nếu có
        Material unlit = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
        if (unlit != null) sr.material = unlit;

        sr.sortingOrder = 999;
        
        Debug.Log("[CameraTest] Đã sinh ra TestRedSquare màu đỏ khổng lồ ở Z=0. Nếu bạn không thấy hình vuông ĐỎ, camera đang BỊ LỖI RENDER!");
    }
}
