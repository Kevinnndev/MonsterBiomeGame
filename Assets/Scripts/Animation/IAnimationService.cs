using System;
using UnityEngine;

public enum AnimationEase
{
    OutQuad,
    OutBack,
    InBack
}

public interface IAnimationService
{
    void ScaleTo(Transform target, Vector3 to, float duration, AnimationEase ease,
        bool unscaled = false, float delay = 0f, Action onComplete = null);

    void PunchScale(Transform target, Vector3 amount, float duration);

    void ShakePosition(Transform target, Vector3 strength, float duration, int vibrato = 10, bool unscaled = false);

    void ColorTo(SpriteRenderer target, Color to, float duration, bool unscaled = false);

    void FadeTo(CanvasGroup target, float alpha, float duration, bool unscaled = false);

    void MoveAnchor(RectTransform target, Vector2 to, float duration, AnimationEase ease,
        bool unscaled = false, Action onComplete = null);

    void ValueTo(Component target, float from, float to, float duration, Action<float> onUpdate,
        AnimationEase ease = AnimationEase.OutQuad);

    void Kill(Component target, bool complete = false);
}
