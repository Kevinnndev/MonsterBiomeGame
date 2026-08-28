using System;
using UnityEngine;
using DG.Tweening;

public class DotweenAnimationService : IAnimationService
{
    public void ScaleTo(Transform target, Vector3 to, float duration, AnimationEase ease,
        bool unscaled = false, float delay = 0f, Action onComplete = null)
    {
        Tweener tween = target.DOScale(to, duration)
            .SetEase(ToEase(ease))
            .SetLink(target.gameObject);
        if (unscaled) tween.SetUpdate(true);
        if (delay > 0f) tween.SetDelay(delay);
        if (onComplete != null) tween.OnComplete(() => onComplete());
    }

    public void PunchScale(Transform target, Vector3 amount, float duration)
    {
        target.DOPunchScale(amount, duration, 2, 0.5f).SetLink(target.gameObject);
    }

    public void ShakePosition(Transform target, Vector3 strength, float duration, int vibrato = 10, bool unscaled = false)
    {
        Tween tween = target.DOShakePosition(duration, strength, vibrato, 90, false, true)
            .SetLink(target.gameObject);
        if (unscaled) tween.SetUpdate(true);
    }

    public void ColorTo(SpriteRenderer target, Color to, float duration, bool unscaled = false)
    {
        Tween tween = target.DOColor(to, duration).SetLink(target.gameObject);
        if (unscaled) tween.SetUpdate(true);
    }

    public void FadeTo(CanvasGroup target, float alpha, float duration, bool unscaled = false)
    {
        Tween tween = target.DOFade(alpha, duration).SetLink(target.gameObject);
        if (unscaled) tween.SetUpdate(true);
    }

    public void MoveAnchor(RectTransform target, Vector2 to, float duration, AnimationEase ease,
        bool unscaled = false, Action onComplete = null)
    {
        Tween tween = target.DOAnchorPos(to, duration).SetEase(ToEase(ease)).SetLink(target.gameObject);
        if (unscaled) tween.SetUpdate(true);
        if (onComplete != null) tween.OnComplete(() => onComplete());
    }

    public void ValueTo(Component target, float from, float to, float duration, Action<float> onUpdate,
        AnimationEase ease = AnimationEase.OutQuad)
    {
        DOTween.To(() => from, v => onUpdate(v), to, duration)
            .SetEase(ToEase(ease))
            .SetTarget(target)
            .SetLink(target.gameObject);
    }

    public void Kill(Component target, bool complete = false)
    {
        target.DOKill(complete);
    }

    private static Ease ToEase(AnimationEase ease)
    {
        switch (ease)
        {
            case AnimationEase.OutBack: return Ease.OutBack;
            case AnimationEase.InBack: return Ease.InBack;
            default: return Ease.OutQuad;
        }
    }
}
