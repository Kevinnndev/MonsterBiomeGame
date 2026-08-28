using UnityEngine;

public class GameEndSequenceFx
{
    private readonly CanvasGroup darkOverlay;
    private readonly CanvasGroup gameOverCanvasGroup;

    public GameEndSequenceFx(CanvasGroup darkOverlay, CanvasGroup gameOverCanvasGroup)
    {
        this.darkOverlay = darkOverlay;
        this.gameOverCanvasGroup = gameOverCanvasGroup;
    }

    public void PlayGameOverEffects()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Animations.Current.ShakePosition(mainCamera.transform, Vector3.one * 0.15f, 0.3f, vibrato: 10, unscaled: true);
        }

        if (darkOverlay != null)
        {
            darkOverlay.gameObject.SetActive(true);
            darkOverlay.alpha = 0f;
            Animations.Current.FadeTo(darkOverlay, 0.5f, 0.6f, unscaled: true);
        }
    }

    public void KillActiveEffects()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null) Animations.Current.Kill(mainCamera.transform);
        if (darkOverlay != null) Animations.Current.Kill(darkOverlay);
    }

    public void ShowGameOverPanel(GameObject gameOverUI)
    {
        if (gameOverUI == null) return;

        Animations.Current.Kill(gameOverUI.transform);
        gameOverUI.SetActive(true);

        // Ensure scale animates to Vector3.one regardless of gameOverCanvasGroup presence
        gameOverUI.transform.localScale = Vector3.one * 0.8f;
        Animations.Current.ScaleTo(gameOverUI.transform, Vector3.one, 0.5f, AnimationEase.OutBack, unscaled: true);

        CanvasGroup cg = gameOverUI.GetComponent<CanvasGroup>();
        if (cg == null) cg = gameOverCanvasGroup;
        if (cg != null)
        {
            Animations.Current.Kill(cg);
            cg.alpha = 0f;
            Animations.Current.FadeTo(cg, 1f, 0.5f, unscaled: true);
        }
    }

    public void HideGameOverEffects()
    {
        if (darkOverlay != null)
        {
            Animations.Current.Kill(darkOverlay);
            darkOverlay.alpha = 0f;
            darkOverlay.gameObject.SetActive(false);
        }
    }

    public void HideGameOverPanel(GameObject gameOverUI)
    {
        if (gameOverUI == null) return;

        CanvasGroup cg = gameOverUI.GetComponent<CanvasGroup>();
        if (cg == null) cg = gameOverCanvasGroup;
        if (cg != null)
        {
            Animations.Current.Kill(cg);
        }

        Animations.Current.Kill(gameOverUI.transform);
        Animations.Current.ScaleTo(gameOverUI.transform, Vector3.zero, 0.3f, AnimationEase.InBack, unscaled: true,
            onComplete: () => gameOverUI.SetActive(false));
    }

}
