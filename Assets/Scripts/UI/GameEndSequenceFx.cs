using UnityEngine;
using DG.Tweening;

public class GameEndSequenceFx
{
    private readonly CanvasGroup darkOverlay;
    private readonly CanvasGroup gameOverCanvasGroup;
    private readonly GameObject linkTarget;
    private Sequence activeSequence;

    public GameEndSequenceFx(CanvasGroup darkOverlay, CanvasGroup gameOverCanvasGroup, GameObject linkTarget)
    {
        this.darkOverlay = darkOverlay;
        this.gameOverCanvasGroup = gameOverCanvasGroup;
        this.linkTarget = linkTarget;
    }

    public void PlayGameOverEffects()
    {
        KillActiveEffects();
        activeSequence = DOTween.Sequence().SetUpdate(true).SetLink(linkTarget);

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            activeSequence.Insert(0, mainCamera.transform.DOShakePosition(0.3f, strength: 0.15f, vibrato: 10).SetUpdate(true));
        }

        if (darkOverlay != null)
        {
            darkOverlay.gameObject.SetActive(true);
            darkOverlay.alpha = 0f;
            activeSequence.Insert(0, darkOverlay.DOFade(0.5f, 0.6f).SetEase(Ease.OutQuad).SetUpdate(true));
        }
    }

    public void ShowGameOverPanel(GameObject gameOverUI)
    {
        gameOverUI.SetActive(true);
        if (gameOverCanvasGroup == null) return;

        gameOverCanvasGroup.alpha = 0f;
        gameOverUI.transform.localScale = Vector3.one * 1.1f;
        gameOverCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.OutQuad).SetUpdate(true).SetLink(linkTarget);
        gameOverUI.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutQuad).SetUpdate(true).SetLink(linkTarget);
    }

    public void KillActiveEffects()
    {
        activeSequence?.Kill();
        activeSequence = null;
    }
}
