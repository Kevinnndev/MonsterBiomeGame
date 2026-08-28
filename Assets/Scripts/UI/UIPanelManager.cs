using UnityEngine;
using UnityEngine.UI;

public class UIPanelManager : MonoBehaviour
{
    [Header("Panels & UI References")]
    public GameObject mainMenuUI;
    public GameObject settingsPanel;
    public GameObject settingScreen;
    public GameObject gameOverOutOfTimeUI;
    public GameObject gameOverOutOfLifeUI;
    public GameObject winScreenUI;
    public GameObject restartButton;
    public GameObject nextLevelButton;
    public GameObject topBarPanel;
    public GameObject howToPlayPanel;
    public GameObject boosterPanel;

    public void InitializeUI()
    {
        mainMenuUI.SetActive(true);
        settingsPanel.SetActive(false);
        if (settingScreen) settingScreen.SetActive(false);
        gameOverOutOfTimeUI.SetActive(false);
        gameOverOutOfLifeUI.SetActive(false);
        winScreenUI.SetActive(false);
        restartButton.SetActive(false);
        nextLevelButton.SetActive(false);
        topBarPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        boosterPanel.SetActive(false);
    }

    private void Awake()
    {
        ReportIfMissing(mainMenuUI, nameof(mainMenuUI));
        ReportIfMissing(settingsPanel, nameof(settingsPanel));
        ReportIfMissing(settingScreen, nameof(settingScreen));
        ReportIfMissing(gameOverOutOfTimeUI, nameof(gameOverOutOfTimeUI));
        ReportIfMissing(gameOverOutOfLifeUI, nameof(gameOverOutOfLifeUI));
        ReportIfMissing(winScreenUI, nameof(winScreenUI));
        ReportIfMissing(restartButton, nameof(restartButton));
        ReportIfMissing(nextLevelButton, nameof(nextLevelButton));
        ReportIfMissing(topBarPanel, nameof(topBarPanel));
        ReportIfMissing(howToPlayPanel, nameof(howToPlayPanel));
        ReportIfMissing(boosterPanel, nameof(boosterPanel));
    }

    private void ReportIfMissing(GameObject obj, string fieldName)
    {
        if (obj == null) Debug.LogError($"[UIPanelManager] '{fieldName}' is not assigned on {name}.", this);
    }

    public void ShowMainMenuUI()
    {
        settingsPanel.SetActive(false);
        if (settingScreen) settingScreen.SetActive(false);
        gameOverOutOfTimeUI.SetActive(false);
        gameOverOutOfLifeUI.SetActive(false);
        winScreenUI.SetActive(false);
        topBarPanel.SetActive(false);
        boosterPanel.SetActive(false);
        restartButton.SetActive(false);
        nextLevelButton.SetActive(false);
        mainMenuUI.SetActive(true);
    }

    public void ShowLevelUI()
    {
        mainMenuUI.SetActive(false);
        settingsPanel.SetActive(false);
        if (settingScreen) settingScreen.SetActive(false);
        gameOverOutOfTimeUI.SetActive(false);
        gameOverOutOfLifeUI.SetActive(false);
        winScreenUI.SetActive(false);
        howToPlayPanel.SetActive(false);
        restartButton.SetActive(false);
        nextLevelButton.SetActive(false);
        topBarPanel.SetActive(true);
        boosterPanel.SetActive(true);
        RestoreGameUI(0f);
    }

    public void ShowPanel(GameObject panel)
    {
        Animations.Current.Kill(panel.transform);
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg != null) Animations.Current.Kill(cg);

        panel.SetActive(true);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, 800);
        Animations.Current.MoveAnchor(rect, Vector2.zero, 0.4f, AnimationEase.OutBack, unscaled: true);
        if (cg != null)
        {
            cg.alpha = 0f;
            Animations.Current.FadeTo(cg, 1f, 0.4f, unscaled: true);
        }
    }

    public void HidePanel(GameObject panel)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        Animations.Current.Kill(rect);
        if (cg != null) Animations.Current.Kill(cg);

        Animations.Current.MoveAnchor(rect, new Vector2(0, -800), 0.3f, AnimationEase.InBack, unscaled: true,
            onComplete: () => panel.SetActive(false));
    }

    public void ShowPopupScale(GameObject panel)
    {
        Animations.Current.Kill(panel.transform);
        panel.SetActive(true);
        panel.transform.localScale = Vector3.zero;
        Animations.Current.ScaleTo(panel.transform, Vector3.one, 0.4f, AnimationEase.OutBack, unscaled: true);
    }

    public void DimGameUI(float targetAlpha = 0.3f, float duration = 0.4f)
    {
        FadeCanvasGroup(topBarPanel, targetAlpha, duration);
        FadeCanvasGroup(boosterPanel, targetAlpha, duration);
    }

    public void RestoreGameUI(float duration = 0.3f)
    {
        FadeCanvasGroup(topBarPanel, 1f, duration);
        FadeCanvasGroup(boosterPanel, 1f, duration);
    }

    private void FadeCanvasGroup(GameObject panel, float targetAlpha, float duration)
    {
        if (panel == null) return;
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        Animations.Current.Kill(cg);
        Animations.Current.FadeTo(cg, targetAlpha, duration, unscaled: true);
    }
}
