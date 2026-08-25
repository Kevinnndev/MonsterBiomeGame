using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIPanelManager : MonoBehaviour
{
    [Header("Panels & UI References")]
    public GameObject mainMenuUI;
    public GameObject settingsPanel;
    public GameObject gameOverUI;
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
        gameOverUI.SetActive(false);
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
        ReportIfMissing(gameOverUI, nameof(gameOverUI));
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
        gameOverUI.SetActive(false);
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
        gameOverUI.SetActive(false);
        winScreenUI.SetActive(false);
        howToPlayPanel.SetActive(false);
        restartButton.SetActive(false);
        nextLevelButton.SetActive(false);
        topBarPanel.SetActive(true);
        boosterPanel.SetActive(true);
    }

    public void ShowPanel(GameObject panel)
    {
        panel.SetActive(true);
        RectTransform rect = panel.GetComponent<RectTransform>();
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        rect.DOKill();
        rect.anchoredPosition = new Vector2(0, 800);
        rect.DOAnchorPos(Vector2.zero, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
        if (cg != null)
        {
            cg.DOKill();
            cg.alpha = 0f;
            cg.DOFade(1f, 0.4f).SetUpdate(true);
        }
    }

    public void HidePanel(GameObject panel, bool resumeTime)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();

        Sequence seq = DOTween.Sequence().SetUpdate(true);
        rect.DOKill();
        seq.Join(rect.DOAnchorPos(new Vector2(0, -800), 0.3f).SetEase(Ease.InBack));
        if (cg != null)
        {
            cg.DOKill();
            seq.Join(cg.DOFade(0f, 0.3f));
        }

        seq.OnComplete(() => {
            panel.SetActive(false);
            if (resumeTime) Time.timeScale = 1f;
        });
    }

    public void ShowPopupScale(GameObject panel)
    {
        panel.SetActive(true);
        panel.transform.DOKill();
        panel.transform.localScale = Vector3.zero;
        panel.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
    }
}
