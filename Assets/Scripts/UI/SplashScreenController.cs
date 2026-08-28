using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Điều phối Splash Screen: async load SampleScene trong background,
/// cập nhật progress bar, rồi chuyển sang game.
/// Dùng Additive Loading để không có khoảng trắng giữa 2 scenes.
/// </summary>
public class SplashScreenController : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private SplashScreenUI splashUI;

    [Header("Settings")]
    [Tooltip("Tên scene chính cần load")]
    [SerializeField] private string targetSceneName = "SampleScene";

    [Tooltip("Thời gian tối thiểu hiển thị splash (giây)")]
    [SerializeField] private float minimumDisplayTime = 2.5f;

    private void Start()
    {
        StartCoroutine(LoadGameAsync());
    }

    private IEnumerator LoadGameAsync()
    {
        // Load scene chính ADDITIVE (chồng lên splash, không unload splash)
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
        asyncOp.allowSceneActivation = false;

        float elapsed = 0f;
        float displayedProgress = 0f;

        while (!IsLoadComplete(asyncOp) || elapsed < minimumDisplayTime)
        {
            elapsed += Time.unscaledDeltaTime;

            float realProgress = Mathf.Clamp01(asyncOp.progress / 0.9f);

            if (IsLoadComplete(asyncOp))
            {
                float remainingRatio = Mathf.Clamp01(elapsed / minimumDisplayTime);
                realProgress = Mathf.Max(realProgress, remainingRatio);
            }

            displayedProgress = Mathf.Max(displayedProgress,
                Mathf.Lerp(displayedProgress, realProgress, Time.unscaledDeltaTime * 5f));

            if (splashUI != null)
            {
                splashUI.UpdateProgress(displayedProgress);
            }

            yield return null;
        }

        // Hiển thị 100%
        if (splashUI != null)
        {
            splashUI.UpdateProgress(1f);
        }

        yield return new WaitForSecondsRealtime(0.3f);

        // Kích hoạt scene chính (vẫn chồng lên splash)
        asyncOp.allowSceneActivation = true;

        // Chờ scene mới activate hoàn toàn
        while (!asyncOp.isDone)
        {
            yield return null;
        }

        // Set scene mới làm active scene (để lighting, physics... đúng)
        Scene gameScene = SceneManager.GetSceneByName(targetSceneName);
        if (gameScene.IsValid())
        {
            SceneManager.SetActiveScene(gameScene);
        }

        // Chờ 1 frame để scene mới render
        yield return null;

        // Bỏ splash scene — game scene đã sẵn sàng, không có khoảng trắng
        SceneManager.UnloadSceneAsync("SplashScreen");
    }

    private bool IsLoadComplete(AsyncOperation asyncOp)
    {
        return asyncOp.progress >= 0.9f;
    }
}
