// File: Assets/Scripts/Editor/LivesUISetupTool.cs
// Đặt trong thư mục Editor — Unity sẽ KHÔNG build file này vào game thật.
// Mục đích: Tự động hoá việc setup 3 icon tim (heartIcons) trong LivesPanel,
//           kiểm tra/wire nút OpenSettingsBtn → GameManager.OpenSettings(),
//           và sửa vị trí các nút UI đang nằm ngoài màn hình (y=750).
//
// === KẾT QUẢ KHÁM PHÁ (Bước 1) ===
// • heartIcons = public GameObject[] ✅ (GameManager.cs dòng 65)
// • LivesPanel đã có 3 children: Heart_1, Heart_2, Heart_3 + HorizontalLayoutGroup
// • Sprite tim: Assets/Sprites/Icon-heart.png (GUID: c4082a09f2a035b4d9aa747770e7e7c8)
// • Bug vị trí: OpenSettingsBtn y=750, HowToPlayBtn y=750 → nằm ngoài TopBarPanel
// • ScoreText: m_IsActive=0 → bị tắt trong scene

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.Events;
using System.Reflection;

public static class LivesUISetupTool
{
    // ─────────────────────────────────────────────────────────────
    // MENU ITEM 1: Setup 3 icon tim trong LivesPanel
    // ─────────────────────────────────────────────────────────────
    [MenuItem("Tools/Monster Biome/Setup Lives UI (3 Hearts)")]
    public static void SetupLivesUI()
    {
        Debug.Log("=== [LivesUISetupTool] Bắt đầu Setup Lives UI ===");

        // ── BƯỚC 1: Tìm LivesPanel trong scene đang mở ──────────────
        // Dùng GameObject.Find vì tên LivesPanel là duy nhất trong Hierarchy.
        // Nếu không tìm thấy → báo lỗi rõ ràng, không crash im lặng.
        GameObject livesPanel = GameObject.Find("LivesPanel");
        if (livesPanel == null)
        {
            Debug.LogError("[LivesUISetupTool] ❌ Không tìm thấy GameObject tên 'LivesPanel' trong scene đang mở. " +
                           "Hãy kiểm tra Hierarchy xem tên có đúng không (phân biệt HOA/thường).");
            return;
        }
        Debug.Log($"[LivesUISetupTool] ✅ Tìm thấy LivesPanel: {GetFullPath(livesPanel)}");

        // ── BƯỚC 2: Đảm bảo LivesPanel có HorizontalLayoutGroup ────
        // Tool đã phát hiện LivesPanel có sẵn HorizontalLayoutGroup (spacing=-30).
        // Chúng ta chỉ thêm nếu chưa có để tool idempotent.
        HorizontalLayoutGroup hlg = livesPanel.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null)
        {
            hlg = livesPanel.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = -10f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            Debug.Log("[LivesUISetupTool] ✅ Đã thêm HorizontalLayoutGroup mới vào LivesPanel.");
        }
        else
        {
            Debug.Log($"[LivesUISetupTool] ℹ️ LivesPanel đã có HorizontalLayoutGroup (spacing={hlg.spacing}). Giữ nguyên.");
        }

        // ── BƯỚC 3: Load sprite tim ─────────────────────────────────
        // Ưu tiên dùng "Icon-heart.png" (đây là sprite tim nhỏ gọn dùng cho UI),
        // fallback sang "heart.png" nếu không tìm thấy.
        Sprite heartSprite = LoadHeartSprite();
        if (heartSprite == null)
        {
            Debug.LogError("[LivesUISetupTool] ❌ Không tìm thấy sprite tim trong Assets/Sprites/. " +
                           "Cần có file 'Icon-heart.png' hoặc 'heart.png' được import đúng kiểu Sprite.");
            return;
        }
        Debug.Log($"[LivesUISetupTool] ✅ Sprite tim đã sẵn sàng: {heartSprite.name}");

        // ── BƯỚC 4: Tạo hoặc cập nhật 3 GameObject con Heart_1/2/3 ──
        // Tên theo convention hiện có trong scene: Heart_1, Heart_2, Heart_3
        // (scene dùng _1-based index, khác với prompt đề xuất _0-based).
        // Tool idempotent: nếu đã tồn tại → không tạo trùng, chỉ update sprite.
        string[] heartNames = { "Heart_1", "Heart_2", "Heart_3" };
        GameObject[] heartObjects = new GameObject[3];

        for (int i = 0; i < heartNames.Length; i++)
        {
            // Tìm child có tên đúng trong LivesPanel (không dùng Find toàn scene
            // để tránh nhầm với object trùng tên ở nơi khác)
            Transform existing = livesPanel.transform.Find(heartNames[i]);

            if (existing != null)
            {
                // ── Idempotent: đã tồn tại, chỉ cập nhật sprite nếu cần ──
                heartObjects[i] = existing.gameObject;
                Image img = existing.GetComponent<Image>();
                if (img != null && img.sprite != heartSprite)
                {
                    img.sprite = heartSprite;
                    img.preserveAspect = true;
                    Debug.Log($"[LivesUISetupTool] 🔄 Cập nhật sprite cho {heartNames[i]}.");
                }
                else
                {
                    Debug.Log($"[LivesUISetupTool] ℹ️ {heartNames[i]} đã tồn tại và đúng sprite, giữ nguyên.");
                }
            }
            else
            {
                // ── Tạo mới GameObject con ──────────────────────────────
                heartObjects[i] = new GameObject(heartNames[i]);

                // Đặt parent là LivesPanel, giữ local transform
                heartObjects[i].transform.SetParent(livesPanel.transform, false);

                // Đặt trên layer UI (Layer 5)
                heartObjects[i].layer = 5;

                // Thêm RectTransform (bắt buộc cho UI)
                RectTransform rt = heartObjects[i].AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(100f, 100f); // 100x100 pixels

                // Thêm Image component và gán sprite tim
                Image img = heartObjects[i].AddComponent<Image>();
                img.sprite = heartSprite;
                img.preserveAspect = true;

                Debug.Log($"[LivesUISetupTool] ✅ Đã tạo mới '{heartNames[i]}'.");
            }
        }

        // ── BƯỚC 5: Gán heartIcons[] vào GameManager dùng SerializedObject ──
        // LÝ DO dùng SerializedObject thay vì gán trực tiếp field:
        //   • Thay đổi qua SerializedObject được Unity ghi nhận vào Undo history (Ctrl+Z)
        //   • Unity đánh dấu scene là "dirty" → nhắc save → không mất dữ liệu khi đóng Editor
        //   • Gán trực tiếp qua field public ở edit-time (không phải runtime) có thể
        //     không được serialize đúng, đặc biệt với mảng (array)
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm == null)
        {
            Debug.LogError("[LivesUISetupTool] ❌ Không tìm thấy GameManager trong scene. " +
                           "Hãy đảm bảo GameObject có script GameManager đang active.");
            return;
        }

        // Tạo SerializedObject bọc quanh GameManager component
        SerializedObject serializedGM = new SerializedObject(gm);

        // Tìm property "heartIcons" (tên field chính xác trong GameManager.cs là "heartIcons")
        SerializedProperty heartIconsProp = serializedGM.FindProperty("heartIcons");
        if (heartIconsProp == null)
        {
            Debug.LogError("[LivesUISetupTool] ❌ Không tìm thấy field 'heartIcons' trong GameManager. " +
                           "Kiểm tra lại tên field trong GameManager.cs (phân biệt HOA/thường).");
            return;
        }

        // Cập nhật từ Unity trước khi sửa (quan trọng để không ghi đè thay đổi khác)
        serializedGM.Update();

        // Resize mảng về đúng 3 phần tử
        heartIconsProp.arraySize = 3;

        // Gán từng element: Heart_1 → index 0, Heart_2 → index 1, Heart_3 → index 2
        // (GameManager dùng heartIcons[0] cho tim cuối cùng bị mất → giữ đúng thứ tự gốc)
        for (int i = 0; i < 3; i++)
        {
            SerializedProperty element = heartIconsProp.GetArrayElementAtIndex(i);
            element.objectReferenceValue = heartObjects[i];
        }

        // Áp dụng thay đổi — đây là lúc Unity ghi nhận vào Undo + đánh dấu scene dirty
        serializedGM.ApplyModifiedProperties();

        Debug.Log("[LivesUISetupTool] ✅ Đã gán heartIcons[0]=Heart_1, [1]=Heart_2, [2]=Heart_3 vào GameManager.");

        // ── BƯỚC 6: Đánh dấu scene dirty để Unity nhắc save ────────
        // Không cần gọi thủ công nếu đã dùng SerializedObject.ApplyModifiedProperties(),
        // nhưng gọi thêm EditorUtility.SetDirty để chắc chắn với các thay đổi trực tiếp.
        EditorUtility.SetDirty(gm);

        // Đánh dấu scene cần lưu
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        Debug.Log("=== [LivesUISetupTool] ✅ Setup Lives UI hoàn tất! Nhớ Ctrl+S để lưu Scene ===");

        EditorUtility.DisplayDialog(
            "Setup Lives UI",
            "✅ Hoàn tất!\n\n" +
            $"• Sprite tim: {heartSprite.name}\n" +
            "• Đã tạo/cập nhật: Heart_1, Heart_2, Heart_3 trong LivesPanel\n" +
            "• Đã gán heartIcons[0..2] vào GameManager\n\n" +
            "⚠️ Hãy nhấn Ctrl+S để lưu Scene!",
            "OK"
        );
    }

    // ─────────────────────────────────────────────────────────────
    // MENU ITEM 2: Kiểm tra và wire nút OpenSettingsBtn
    // ─────────────────────────────────────────────────────────────
    [MenuItem("Tools/Monster Biome/Verify Settings Button Wiring")]
    public static void VerifySettingsButtonWiring()
    {
        Debug.Log("=== [LivesUISetupTool] Bắt đầu Verify Settings Button Wiring ===");

        bool anyFixed = false;

        // ── Bước A: Tìm OpenSettingsBtn ─────────────────────────────
        GameObject settingsBtnObj = GameObject.Find("OpenSettingsBtn");
        if (settingsBtnObj == null)
        {
            Debug.LogError("[LivesUISetupTool] ❌ Không tìm thấy 'OpenSettingsBtn' trong scene. " +
                           "Kiểm tra tên GameObject (phân biệt HOA/thường).");
            return;
        }
        Debug.Log($"[LivesUISetupTool] ✅ Tìm thấy OpenSettingsBtn: {GetFullPath(settingsBtnObj)}");

        // ── Bước B: Kiểm tra Button component ───────────────────────
        Button btn = settingsBtnObj.GetComponent<Button>();
        if (btn == null)
        {
            Debug.LogError("[LivesUISetupTool] ❌ 'OpenSettingsBtn' không có component Button. " +
                           "Hãy thêm Button component thủ công trong Inspector.");
            return;
        }
        Debug.Log("[LivesUISetupTool] ✅ Button component tồn tại.");

        // ── Bước C: Tìm GameManager ──────────────────────────────────
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm == null)
        {
            Debug.LogError("[LivesUISetupTool] ❌ Không tìm thấy GameManager trong scene.");
            return;
        }

        // ── Bước D: Kiểm tra xem đã có listener OpenSettings() chưa ──
        // Duyệt qua tất cả persistent call để tìm listener trỏ đúng method.
        bool alreadyWired = false;
        int callCount = btn.onClick.GetPersistentEventCount();

        for (int i = 0; i < callCount; i++)
        {
            Object target = btn.onClick.GetPersistentTarget(i);
            string methodName = btn.onClick.GetPersistentMethodName(i);

            if (target == gm && methodName == "OpenSettings")
            {
                alreadyWired = true;
                Debug.Log($"[LivesUISetupTool] ✅ Listener 'GameManager.OpenSettings()' đã được wired sẵn ở index {i}.");
                break;
            }
        }

        if (!alreadyWired)
        {
            // ── Thêm persistent listener đúng cách ──────────────────
            // LÝ DO dùng UnityEventTools.AddPersistentListener thay vì btn.onClick.AddListener:
            //   • AddListener() chỉ là runtime listener, KHÔNG được lưu vào scene
            //   • UnityEventTools.AddPersistentListener() tạo persistent call được serialize
            //     vào scene file, hoạt động đúng cả khi Edit mode lẫn Play mode
            SerializedObject serializedBtn = new SerializedObject(btn);
            serializedBtn.Update();

            // Lấy MethodInfo cho OpenSettings (public, void, không tham số)
            MethodInfo openSettingsMethod = typeof(GameManager).GetMethod(
                "OpenSettings",
                BindingFlags.Public | BindingFlags.Instance
            );

            if (openSettingsMethod == null)
            {
                Debug.LogError("[LivesUISetupTool] ❌ Không tìm thấy method 'OpenSettings()' trong GameManager. " +
                               "Kiểm tra lại GameManager.cs.");
                return;
            }

            // Tạo UnityAction delegate trỏ tới GameManager.OpenSettings
            UnityAction openSettingsAction = System.Delegate.CreateDelegate(
                typeof(UnityAction), gm, openSettingsMethod
            ) as UnityAction;

            // Thêm persistent listener — đây là API chính thức của Unity Editor để
            // thêm listener được lưu vào scene (tương đương kéo-thả trong Inspector)
            UnityEventTools.AddPersistentListener(btn.onClick, openSettingsAction);

            serializedBtn.ApplyModifiedProperties();
            EditorUtility.SetDirty(btn);

            Debug.Log("[LivesUISetupTool] ✅ Đã tự động thêm listener 'GameManager.OpenSettings()' vào OpenSettingsBtn.onClick.");
            anyFixed = true;
        }

        // ── Bước E: Kiểm tra field settingsPanel trên GameManager ───
        // Dùng SerializedObject để đọc giá trị field một cách an toàn
        SerializedObject serializedGM = new SerializedObject(gm);
        serializedGM.Update();
        SerializedProperty settingsPanelProp = serializedGM.FindProperty("settingsPanel");

        if (settingsPanelProp == null)
        {
            Debug.LogWarning("[LivesUISetupTool] ⚠️ Không tìm thấy field 'settingsPanel' trong GameManager. " +
                             "Kiểm tra tên field trong GameManager.cs.");
        }
        else if (settingsPanelProp.objectReferenceValue == null)
        {
            // settingsPanel chưa được gán → tự động tìm và gán
            GameObject settingsPanelObj = GameObject.Find("SettingsPanel");
            if (settingsPanelObj != null)
            {
                serializedGM.Update();
                settingsPanelProp.objectReferenceValue = settingsPanelObj;
                serializedGM.ApplyModifiedProperties();
                EditorUtility.SetDirty(gm);
                Debug.Log($"[LivesUISetupTool] ✅ Đã tự động gán settingsPanel = {GetFullPath(settingsPanelObj)}");
                anyFixed = true;
            }
            else
            {
                Debug.LogWarning("[LivesUISetupTool] ⚠️ field 'settingsPanel' đang NULL và không tìm thấy " +
                                 "GameObject tên 'SettingsPanel' trong scene. Bạn cần gán thủ công trong Inspector.");
            }
        }
        else
        {
            Debug.Log($"[LivesUISetupTool] ✅ settingsPanel đã trỏ tới: " +
                      $"{settingsPanelProp.objectReferenceValue.name} — OK.");
        }

        // ── Bước F: Kiểm tra vị trí OpenSettingsBtn (y=750 là bug) ─
        RectTransform btnRect = settingsBtnObj.GetComponent<RectTransform>();
        if (btnRect != null)
        {
            Vector2 pos = btnRect.anchoredPosition;
            if (Mathf.Abs(pos.y) > 200f)
            {
                Debug.LogWarning($"[LivesUISetupTool] ⚠️ OpenSettingsBtn có anchoredPosition.y = {pos.y} — " +
                                 "có thể nằm ngoài màn hình! Giá trị đề xuất: y = -40. " +
                                 "Hãy sửa thủ công trong Inspector → RectTransform.");
            }
            else
            {
                Debug.Log($"[LivesUISetupTool] ✅ OpenSettingsBtn position OK: ({pos.x}, {pos.y})");
            }
        }

        // ── Đánh dấu scene dirty để nhắc save ───────────────────────
        if (anyFixed)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );
        }

        // Tổng hợp kết quả
        string result = anyFixed
            ? "Đã tự động sửa một số vấn đề.\nXem Console để biết chi tiết.\n\n⚠️ Nhấn Ctrl+S để lưu Scene!"
            : "Tất cả đã đúng sẵn, không cần sửa gì. Xem Console để xác nhận.";

        Debug.Log($"=== [LivesUISetupTool] ✅ Verify xong. anyFixed={anyFixed} ===");

        EditorUtility.DisplayDialog("Verify Settings Button Wiring", result, "OK");
    }

    // ─────────────────────────────────────────────────────────────
    // MENU ITEM 3: Làm nút Settings hiện ra (màu sắc + icon text)
    // ─────────────────────────────────────────────────────────────
    // Root cause: OpenSettingsBtn dùng Shape-BG.png (trắng) trên nền trắng → VÔ HÌNH.
    // Fix: tô màu Image thành crimson (màu chủ đạo của game) + set TMP text = "⚙"
    //      để nút luôn thấy được dù chưa gán icon tùy chỉnh.
    [MenuItem("Tools/Monster Biome/Fix Settings Button Visibility")]
    public static void FixSettingsButtonVisibility()
    {
        Debug.Log("=== [LivesUISetupTool] Fix Settings Button Visibility ===");

        // ── Tìm OpenSettingsBtn ──────────────────────────────────
        GameObject btnObj = GameObject.Find("OpenSettingsBtn");
        if (btnObj == null)
        {
            Debug.LogError("[LivesUISetupTool] ❌ Không tìm thấy 'OpenSettingsBtn' trong scene.");
            return;
        }

        bool anyFixed = false;

        // ── Fix 1: Tô màu Image của nút (Shape-BG trắng → màu crimson) ──
        // Lý do: Shape-BG.png là hình vuông tròn trắng → trùng với nền → vô hình.
        // Giải pháp: Đổi Color tint thành màu đậm để nút có thể nhìn thấy.
        Image btnImage = btnObj.GetComponent<Image>();
        if (btnImage != null)
        {
            // Màu crimson nhẹ phù hợp với theme game (tông hồng/đỏ)
            // Dùng SerializedObject để thay đổi được Undo/dirty scene
            SerializedObject soBtnImage = new SerializedObject(btnImage);
            soBtnImage.Update();

            SerializedProperty colorProp = soBtnImage.FindProperty("m_Color");
            // Kiểm tra nếu đang là màu trắng thuần (vô hình trên nền sáng)
            Color currentColor = btnImage.color;
            if (currentColor.r > 0.9f && currentColor.g > 0.9f && currentColor.b > 0.9f)
            {
                // Đổi sang màu hồng đậm chủ đạo của game
                colorProp.colorValue = new Color(0.82f, 0.18f, 0.33f, 1f); // crimson
                soBtnImage.ApplyModifiedProperties();
                EditorUtility.SetDirty(btnImage);
                Debug.Log("[LivesUISetupTool] ✅ Đã tô màu OpenSettingsBtn → crimson (trước đó: trắng/vô hình).");
                anyFixed = true;
            }
            else
            {
                Debug.Log($"[LivesUISetupTool] ℹ️ OpenSettingsBtn Image color = {currentColor} — OK.");
            }
        }
        else
        {
            Debug.LogWarning("[LivesUISetupTool] ⚠️ OpenSettingsBtn không có Image component.");
        }

        // ── Fix 2: Set TMP text của child thành "=" ──
        // OpenSettingsBtn có 1 child TMP với text rỗng → không thấy nội dung.
        // Gán "=" (biểu tượng menu hamburger đơn giản) để không bị lỗi thiếu font SDF.
        TMPro.TextMeshProUGUI tmpChild = btnObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (tmpChild != null)
        {
            if (string.IsNullOrEmpty(tmpChild.text) || tmpChild.text == "⚙")
            {
                SerializedObject soTmp = new SerializedObject(tmpChild);
                soTmp.Update();

                SerializedProperty textProp = soTmp.FindProperty("m_text");
                if (textProp != null && (string.IsNullOrEmpty(textProp.stringValue) || textProp.stringValue == "⚙"))
                {
                    textProp.stringValue = "=";
                    soTmp.ApplyModifiedProperties();

                    // Đặt màu chữ trắng để thấy rõ trên nền crimson
                    SerializedProperty colorProp2 = soTmp.FindProperty("m_fontColor");
                    if (colorProp2 != null) colorProp2.colorValue = Color.white;

                    // Tăng font size cho icon thấy rõ hơn
                    SerializedProperty sizeProp = soTmp.FindProperty("m_fontSize");
                    if (sizeProp != null && sizeProp.floatValue < 30f) sizeProp.floatValue = 45f;

                    // Cho chữ ra giữa
                    SerializedProperty alignmentProp = soTmp.FindProperty("m_textAlignment");
                    if (alignmentProp != null) alignmentProp.intValue = 514; // Center/Middle

                    soTmp.ApplyModifiedProperties();
                    EditorUtility.SetDirty(tmpChild);
                    Debug.Log("[LivesUISetupTool] ✅ Đã set TMP text = '=' (trắng, 45pt) cho OpenSettingsBtn.");
                    anyFixed = true;
                }
                else
                {
                    Debug.Log($"[LivesUISetupTool] ℹ️ TMP text = '{tmpChild.text}' — giữ nguyên.");
                }
            }
        }
        else
        {
            Debug.LogWarning("[LivesUISetupTool] ⚠️ Không tìm thấy TMP child trong OpenSettingsBtn. " +
                             "Bạn có thể thêm Text (TMP) con vào nút thủ công.");
        }

        // ── Đánh dấu scene dirty ──
        if (anyFixed)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );
        }

        Debug.Log($"=== [LivesUISetupTool] Fix visibility xong. anyFixed={anyFixed} ===");
        EditorUtility.DisplayDialog(
            "Fix Settings Button Visibility",
            anyFixed
                ? "✅ Đã sửa!\n\n• Image: trắng → crimson\n• Text: → '='\n\n" +
                  "Sau này khi có icon riêng, kéo vào Image.Sprite rồi đổi Color về trắng.\n\n" +
                  "⚠️ Ctrl+S để lưu Scene!"
                : "ℹ️ Nút đã có màu sắc đúng, không cần sửa.",
            "OK"
        );
    }

    // ─────────────────────────────────────────────────────────────
    // MENU ITEM 4: Sửa vị trí các nút UI đang nằm ngoài TopBarPanel
    // ─────────────────────────────────────────────────────────────
    // Bug đã xác nhận qua đọc scene: OpenSettingsBtn.anchoredPosition.y = 750
    //                                 HowToPlayBtn.anchoredPosition.y   = 750
    // TopBarPanel chỉ cao 200px → y=750 hoàn toàn nằm ngoài vùng hiển thị.
    // Tool này sửa về giá trị hợp lý trong vùng TopBar (-40 từ góc top-right/center).
    [MenuItem("Tools/Monster Biome/Fix TopBar Button Positions")]
    public static void FixTopBarButtonPositions()
    {
        Debug.Log("=== [LivesUISetupTool] Bắt đầu Fix TopBar Button Positions ===");
        bool anyFixed = false;

        // Danh sách element cần fix: (tên GameObject, anchoredPosition đúng)
        // TopBarPanel: anchor=top, height=200px → các child phải có y ∈ [-200, 0]
        //              (anchor top + pivot top → y âm = di chuyển xuống, vào trong panel)
        //
        // Bug đã xác nhận từ đọc scene file:
        //   LivesPanel   → anchoredPosition = (400,  830)  ← 830px TRÊN đỉnh TopBar!
        //   ScoreText    → anchoredPosition = (-70,  819.5) ← tương tự!
        //   OpenSettings → anchoredPosition = (-40,  750)  ← đã fix trước đó
        //   HowToPlayBtn → anchoredPosition = (350,  750)  ← đã fix trước đó
        var fixes = new System.Collections.Generic.Dictionary<string, Vector2>
        {
            // Hearts panel — anchor top-left, pivot top-left → y=-30 nghĩa là 30px dưới đỉnh TopBar
            { "LivesPanel",      new Vector2(20f,   -30f) },
            // Score text — anchor top-right, pivot top-right → y=-30 nghĩa là 30px dưới đỉnh TopBar
            { "ScoreText",       new Vector2(-20f,  -30f) },
            // Settings button — anchor top-right
            { "OpenSettingsBtn", new Vector2(-40f,  -40f) },
            // HowToPlay button — anchor top-center-right
            { "HowToPlayBtn",    new Vector2(350f,  -40f) },
        };

        foreach (var kvp in fixes)
        {
            GameObject btnObj = GameObject.Find(kvp.Key);
            if (btnObj == null)
            {
                Debug.LogWarning($"[LivesUISetupTool] ⚠️ Không tìm thấy '{kvp.Key}' — bỏ qua.");
                continue;
            }

            RectTransform rect = btnObj.GetComponent<RectTransform>();
            if (rect == null) continue;

            Vector2 oldPos = rect.anchoredPosition;
            if (Mathf.Abs(oldPos.y) > 200f || Mathf.Abs(oldPos.x) > 800f)
            {
                // Dùng SerializedObject để thay đổi được Undo/dirty scene
                SerializedObject so = new SerializedObject(rect);
                so.Update();
                SerializedProperty m_AnchoredPosition = so.FindProperty("m_AnchoredPosition");
                if (m_AnchoredPosition != null)
                {
                    m_AnchoredPosition.vector2Value = kvp.Value;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(rect);
                    Debug.Log($"[LivesUISetupTool] ✅ Đã sửa {kvp.Key}: ({oldPos.x:F0}, {oldPos.y:F0}) → ({kvp.Value.x:F0}, {kvp.Value.y:F0})");
                    anyFixed = true;
                }
            }
            else
            {
                Debug.Log($"[LivesUISetupTool] ℹ️ {kvp.Key} vị trí OK: ({oldPos.x:F0}, {oldPos.y:F0}) — không cần sửa.");
            }
        }

        // Bonus: Bật lại ScoreText nếu đang bị tắt
        // (phát hiện m_IsActive=0 khi đọc scene file)
        GameObject scoreText = GameObject.Find("ScoreText");
        if (scoreText != null && !scoreText.activeSelf)
        {
            scoreText.SetActive(true);
            EditorUtility.SetDirty(scoreText);
            Debug.Log("[LivesUISetupTool] ✅ Đã bật lại ScoreText (trước đó đang bị SetActive=false).");
            anyFixed = true;
        }

        if (anyFixed)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );
        }

        Debug.Log($"=== [LivesUISetupTool] Fix TopBar xong. anyFixed={anyFixed} ===");

        EditorUtility.DisplayDialog(
            "Fix TopBar Button Positions",
            anyFixed
                ? "✅ Đã sửa vị trí các nút và/hoặc bật ScoreText.\nXem Console để biết chi tiết.\n\n⚠️ Nhấn Ctrl+S để lưu Scene!"
                : "ℹ️ Tất cả vị trí đã đúng, không cần sửa.",
            "OK"
        );
    }

    // ─────────────────────────────────────────────────────────────
    // MENU ITEM 5: Sửa kích thước tim và Layout của LivesPanel
    // ─────────────────────────────────────────────────────────────
    // Khắc phục lỗi "trái tym lớn quá làm 3 cái bị che":
    // 1. LivesPanel đang bị scale 1.5, 1.5 → đưa về 1, 1
    // 2. Kích thước (sizeDelta) của Heart_1, Heart_2, Heart_3 đang to → đưa về 60x60
    // 3. Spacing của HorizontalLayoutGroup đang là số âm (-30) → đưa về 10
    [MenuItem("Tools/Monster Biome/Fix Heart Size And Layout")]
    public static void FixHeartSizeAndLayout()
    {
        Debug.Log("=== [LivesUISetupTool] Bắt đầu Fix Heart Size And Layout ===");
        GameObject livesPanel = GameObject.Find("LivesPanel");
        if (livesPanel == null)
        {
            Debug.LogError("[LivesUISetupTool] ❌ Không tìm thấy LivesPanel!");
            return;
        }

        bool anyFixed = false;

        // 1. Reset scale LivesPanel về 1, 1, 1
        if (livesPanel.transform.localScale != Vector3.one)
        {
            SerializedObject soPanel = new SerializedObject(livesPanel.transform);
            soPanel.Update();
            soPanel.FindProperty("m_LocalScale").vector3Value = Vector3.one;
            soPanel.ApplyModifiedProperties();
            anyFixed = true;
            Debug.Log("[LivesUISetupTool] ✅ Đã đưa Scale của LivesPanel về 1, 1, 1");
        }

        // 2. Chỉnh spacing của HorizontalLayoutGroup
        HorizontalLayoutGroup hlg = livesPanel.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null && hlg.spacing < 10f)
        {
            SerializedObject soHlg = new SerializedObject(hlg);
            soHlg.Update();
            soHlg.FindProperty("m_Spacing").floatValue = 10f; // Khoảng cách giãn ra
            soHlg.ApplyModifiedProperties();
            anyFixed = true;
            Debug.Log("[LivesUISetupTool] ✅ Đã sửa spacing của HorizontalLayoutGroup thành 10");
        }

        // 3. Chỉnh kích thước từng tim về 60x60
        string[] heartNames = { "Heart_1", "Heart_2", "Heart_3" };
        foreach (string hName in heartNames)
        {
            Transform heartTrans = livesPanel.transform.Find(hName);
            if (heartTrans != null)
            {
                RectTransform rt = heartTrans.GetComponent<RectTransform>();
                if (rt != null && rt.sizeDelta.x > 60f)
                {
                    SerializedObject soRt = new SerializedObject(rt);
                    soRt.Update();
                    soRt.FindProperty("m_SizeDelta").vector2Value = new Vector2(60f, 60f);
                    soRt.ApplyModifiedProperties();
                    anyFixed = true;
                    Debug.Log($"[LivesUISetupTool] ✅ Đã thu nhỏ {hName} về 60x60");
                }
            }
        }

        if (anyFixed)
        {
            EditorUtility.SetDirty(livesPanel);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );
        }

        Debug.Log($"=== [LivesUISetupTool] Fix Heart Size xong. anyFixed={anyFixed} ===");
        EditorUtility.DisplayDialog(
            "Fix Heart Size And Layout",
            anyFixed
                ? "✅ Đã sửa xong trái tim!\n\n• LivesPanel Scale → 1\n• Spacing → 10\n• Kích thước tim → 60x60\n\n⚠️ Nhấn Ctrl+S để lưu Scene!"
                : "ℹ️ Trái tim đã ở kích thước đúng, không cần sửa.",
            "OK"
        );
    }

    // ─────────────────────────────────────────────────────────────
    // Helper: Load sprite tim theo thứ tự ưu tiên
    // ─────────────────────────────────────────────────────────────
    private static Sprite LoadHeartSprite()
    {
        // Ưu tiên 1: Icon-heart.png (sprite tim nhỏ gọn dùng cho UI icon)
        // GUID = c4082a09f2a035b4d9aa747770e7e7c8 (đã xác nhận từ .meta file)
        Sprite[] candidates = {
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Icon-heart.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/heart.png"),
        };

        foreach (Sprite s in candidates)
        {
            if (s != null) return s;
        }

        // Nếu cả hai đường dẫn thẳng không load được (sprite atlas/multi-sprite),
        // thử load tất cả asset trong file và lấy cái đầu tiên có type Sprite
        string[] guids = {
            "c4082a09f2a035b4d9aa747770e7e7c8", // Icon-heart.png
            "22123470b49c8dd4bacef401a11a59f5"   // heart.png
        };

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) continue;

            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object asset in allAssets)
            {
                if (asset is Sprite sprite) return sprite;
            }
        }

        return null;
    }

    // ─────────────────────────────────────────────────────────────
    // Helper: Trả về đường dẫn đầy đủ của GameObject trong Hierarchy
    // (hữu ích cho Debug.Log để biết chính xác object nào đang được xử lý)
    // ─────────────────────────────────────────────────────────────
    private static string GetFullPath(GameObject go)
    {
        string path = go.name;
        Transform current = go.transform.parent;
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        return path;
    }
}
