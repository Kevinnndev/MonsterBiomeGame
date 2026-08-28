using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class BoardCell : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public SpriteRenderer cellSprite;
    public SpriteRenderer monsterSprite;

    [Header("Mark Visual (Sprite)")]
    [SerializeField] private SpriteRenderer markIcon;

    private BoardCellFx fx;
    private int row, col;
    private System.Action<int, int> onClickCallback;
    private System.Func<bool> canInteractCallback;
    private Vector3 originalScale;

    private int lastHandledFrame = -1;

    private void Awake()
    {
        if (cellSprite == null) cellSprite = GetComponent<SpriteRenderer>();
        if (monsterSprite == null || markIcon == null)
            Debug.LogError($"[BoardCell] monsterSprite or markIcon is not assigned on {name}.", this);

        fx = new BoardCellFx(cellSprite, monsterSprite, markIcon, transform);
        markIcon.enabled = false;
    }

    public void InitCell(int r, int c, System.Action<int, int> clickHandler, System.Func<bool> interactableCheck, Vector3 initialScale)
    {
        row = r;
        col = c;
        onClickCallback = clickHandler;
        canInteractCallback = interactableCheck;
        originalScale = initialScale;
    }

    private void HandleClick()
    {
        if (canInteractCallback != null && !canInteractCallback.Invoke()) return;
        if (Time.frameCount == lastHandledFrame) return;
        lastHandledFrame = Time.frameCount;
        onClickCallback?.Invoke(row, col);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        HandleClick();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (canInteractCallback == null || canInteractCallback.Invoke())
        {
            fx.Hover(originalScale);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        fx.Unhover(originalScale);
    }

    public void SetupCell(int biomeID, Color biomeColor)
    {
        Color color = new Color(biomeColor.r, biomeColor.g, biomeColor.b, 1f);
        BoxCollider2D col2D = GetComponent<BoxCollider2D>();

        if (biomeID == 0)
        {
            cellSprite.enabled = false;
            col2D.enabled = false;
        }
        else
        {
            cellSprite.enabled = true;
            cellSprite.color = color;
            col2D.enabled = true;
            if (cellSprite.sprite != null)
            {
                col2D.size = cellSprite.sprite.rect.size / cellSprite.sprite.pixelsPerUnit;
            }
        }
        monsterSprite.enabled = false;
        markIcon.enabled = false;
    }

    public void GrayOutMonster(Color grayColor)
    {
        fx.GrayMonster(grayColor);
    }

    public void RestoreMonsterColor()
    {
        fx.RestoreMonsterColor();
    }

    public void SetMarkState(bool isMarked, Color biomeColor, float markedAlpha)
    {
        if (isMarked) fx.Mark(biomeColor, markedAlpha);
        else fx.Unmark(biomeColor);
    }

    public void SetMonsterState(bool hasMonster, Sprite sprite, Color biomeColor)
    {
        if (hasMonster)
        {
            monsterSprite.enabled = true;
            monsterSprite.color = Color.white;
            monsterSprite.sortingOrder = cellSprite.sortingOrder + 1;
            if (sprite != null) monsterSprite.sprite = sprite;
            ScaleSpriteToFit();
            fx.ShowMonster(biomeColor, 0.35f);
        }
        else
        {
            fx.HideMonster(biomeColor);
        }
    }

    public void ShowErrorSprite(Sprite errorSprite, Color biomeColor)
    {
        if (errorSprite != null) monsterSprite.sprite = errorSprite;
        monsterSprite.enabled = true;
        monsterSprite.color = Color.white;
        monsterSprite.sortingOrder = cellSprite.sortingOrder + 1;
        ScaleSpriteToFit();
        fx.ShowError(biomeColor, 0.2f);
    }

    private void ScaleSpriteToFit()
    {
        if (monsterSprite.sprite == null) return;

        monsterSprite.transform.localScale = Vector3.one;

        if (cellSprite.sprite != null)
        {
            Vector3 cellBounds = cellSprite.bounds.size;
            Vector3 monsterBounds = monsterSprite.bounds.size;

            float targetSize = Mathf.Min(cellBounds.x, cellBounds.y) * 0.8f;
            float currentMaxDimension = Mathf.Max(monsterBounds.x, monsterBounds.y);

            if (currentMaxDimension > 0)
            {
                float scale = targetSize / currentMaxDimension;
                monsterSprite.transform.localScale = new Vector3(scale, scale, 1f);
            }
        }
        else
        {
            monsterSprite.transform.localScale = Vector3.one * 0.8f;
        }
    }
}
