using System;

public static class Animations
{
    public static IAnimationService Current { get; private set; } = new DotweenAnimationService();

    public static void Register(IAnimationService service)
    {
        Current = service ?? throw new ArgumentNullException(nameof(service));
    }
}
