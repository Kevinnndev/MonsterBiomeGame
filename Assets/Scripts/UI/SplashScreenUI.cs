using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Quản lý visual/animation cho Splash Screen:
/// - Progress bar fill + percentage text
/// - Loading dots animation
///
/// Setup trong Unity Editor:
/// 1. Canvas (Screen Space - Overlay, CanvasScaler Scale With Screen Size 1080×1920)
///    ├── Background (Image — drag Textures/background.png, set to Preserve Aspect / Envelope Parent)
///    ├── ProgressBarBG (Image — rounded rect, dark semi-transparent)
///    │   └── ProgressBarFill (Image — Type: Filled, Fill Method: Horizontal, gradient sprite hoặc solid)
///    ├── LoadingText (TextMeshProUGUI — "Loading", bottom center phía trên progress bar)
///    └── PercentText (TextMeshProUGUI — "0%", bên phải hoặc trên progress bar)
/// </summary>
public class SplashScreenUI : MonoBehaviour
{
    [Header("Progress Bar")]
    [SerializeField] private Image progressBarFill;
    [SerializeField] private TextMeshProUGUI percentText;

    [Header("Loading Text")]
    [SerializeField] private TextMeshProUGUI loadingText;
    [Tooltip("Tốc độ thay đổi dots (giây)")]
    [SerializeField] private float dotSpeed = 0.4f;

    private Coroutine dotsCoroutine;

    private void OnEnable()
    {
        // Khởi tạo trạng thái ban đầu
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = 0f;
        }

        if (percentText != null)
        {
            percentText.text = "0%";
        }

        // Bắt đầu animation loading dots
        dotsCoroutine = StartCoroutine(AnimateLoadingDots());
    }

    private void OnDisable()
    {
        if (dotsCoroutine != null)
        {
            StopCoroutine(dotsCoroutine);
            dotsCoroutine = null;
        }
    }

    /// <summary>
    /// Cập nhật progress bar và percentage text.
    /// </summary>
    /// <param name="progress">Giá trị từ 0 đến 1</param>
    public void UpdateProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);

        if (progressBarFill != null)
        {
            // Smooth fill bằng DOTween
            progressBarFill.DOKill();
            progressBarFill.DOFillAmount(progress, 0.25f).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        if (percentText != null)
        {
            percentText.text = Mathf.RoundToInt(progress * 100f) + "%";
        }
    }

    /// <summary>
    /// Animation "Loading." → "Loading.." → "Loading..."
    /// </summary>
    private IEnumerator AnimateLoadingDots()
    {
        string baseText = "Loading";
        int dotCount = 0;

        while (true)
        {
            dotCount = (dotCount % 3) + 1;
            if (loadingText != null)
            {
                loadingText.text = baseText + new string('.', dotCount);
            }
            yield return new WaitForSecondsRealtime(dotSpeed);
        }
    }
}

