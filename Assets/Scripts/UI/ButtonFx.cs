using UnityEngine;
using DG.Tweening;

public static class ButtonFx
{
    public static void Punch(Transform target)
    {
        target.DOPunchScale(new Vector3(0.15f, 0.15f, 0f), 0.2f, 2, 0.5f);
    }
}
