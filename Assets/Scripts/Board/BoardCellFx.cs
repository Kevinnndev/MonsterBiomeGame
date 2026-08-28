using UnityEngine;

public class BoardCellFx
{
    private readonly SpriteRenderer cellSprite;
    private readonly SpriteRenderer monsterSprite;
    private readonly SpriteRenderer markIcon;
    private readonly Transform root;
    private readonly Vector3 markIconOriginalScale;

    public BoardCellFx(SpriteRenderer cellSprite, SpriteRenderer monsterSprite, SpriteRenderer markIcon, Transform root)
    {
        this.cellSprite = cellSprite;
        this.monsterSprite = monsterSprite;
        this.markIcon = markIcon;
        this.root = root;
        markIconOriginalScale = markIcon.transform.localScale;
    }

    public void Hover(Vector3 baseScale)
    {
        Animations.Current.Kill(root);
        Animations.Current.ScaleTo(root, baseScale * 1.05f, 0.15f, AnimationEase.OutQuad);
    }

    public void Unhover(Vector3 baseScale)
    {
        Animations.Current.Kill(root);
        Animations.Current.ScaleTo(root, baseScale, 0.15f, AnimationEase.OutQuad);
    }

    public void GrayMonster(Color grayColor)
    {
        if (!monsterSprite.enabled) return;
        Animations.Current.ColorTo(monsterSprite, grayColor, 0.4f, unscaled: true);
    }

    public void RestoreMonsterColor()
    {
        if (!monsterSprite.enabled) return;
        Animations.Current.ColorTo(monsterSprite, Color.white, 0.4f, unscaled: true);
    }

    public void Mark(Color biomeColor, float markedAlpha)
    {
        markIcon.enabled = true;
        Animations.Current.Kill(markIcon.transform);
        markIcon.transform.localScale = Vector3.zero;
        Animations.Current.ScaleTo(markIcon.transform, markIconOriginalScale, 0.3f, AnimationEase.OutBack);

        Animations.Current.ColorTo(cellSprite, FullColor(biomeColor, markedAlpha), 0.15f);
        Animations.Current.Kill(root, complete: true);
        Animations.Current.PunchScale(root, Vector3.one * 0.08f, 0.15f);
    }

    public void Unmark(Color biomeColor)
    {
        markIcon.enabled = false;
        Animations.Current.Kill(markIcon.transform);

        Animations.Current.ColorTo(cellSprite, FullColor(biomeColor, 1f), 0.15f);
        Animations.Current.Kill(root, complete: true);
        Animations.Current.PunchScale(root, Vector3.one * 0.08f, 0.15f);
    }

    public void ShowMonster(Color biomeColor, float duration)
    {
        RestoreCellColor(biomeColor);
        markIcon.enabled = false;
        MonsterIn(duration);
    }

    public void HideMonster(Color biomeColor)
    {
        RestoreCellColor(biomeColor);
        markIcon.enabled = false;

        if (monsterSprite.enabled)
        {
            Animations.Current.Kill(monsterSprite.transform);
            Animations.Current.ScaleTo(monsterSprite.transform, Vector3.zero, 0.3f, AnimationEase.InBack,
                onComplete: () => monsterSprite.enabled = false);
        }
    }

    public void ShowError(Color biomeColor, float duration)
    {
        RestoreCellColor(biomeColor);
        markIcon.enabled = false;
        MonsterIn(duration);

        Animations.Current.Kill(root);
        Animations.Current.ShakePosition(root, new Vector3(0.1f, 0f, 0f), 0.4f, vibrato: 20);
    }

    private void MonsterIn(float duration)
    {
        Vector3 targetScale = monsterSprite.transform.localScale;
        if (targetScale.x <= 0.001f || targetScale.y <= 0.001f)
        {
            targetScale = Vector3.one * 0.8f;
        }

        Animations.Current.Kill(monsterSprite.transform);
        monsterSprite.transform.localScale = Vector3.zero;
        Animations.Current.ScaleTo(monsterSprite.transform, targetScale, duration, AnimationEase.OutBack);
    }

    private void RestoreCellColor(Color biomeColor)
    {
        Animations.Current.Kill(cellSprite);
        cellSprite.color = FullColor(biomeColor, 1f);
    }

    private static Color FullColor(Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }
}
