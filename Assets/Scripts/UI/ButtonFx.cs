using UnityEngine;

public static class ButtonFx
{
    public static void Punch(Transform target)
    {
        Animations.Current.PunchScale(target, new Vector3(0.15f, 0.15f, 0f), 0.2f);
    }
}
