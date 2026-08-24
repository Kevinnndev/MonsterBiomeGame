using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class BoardCell : MonoBehaviour, IPointerDownHandler
{
    public SpriteRenderer cellSprite;
    public SpriteRenderer monsterSprite;

    [Header("Mark Visual (Sprite)")]
    [SerializeField] private SpriteRenderer markIcon;

    private int row, col;
    private System.Action<int, int> onClickCallback;
    private System.Func<bool> canInteractCallback;
    private Vector3 originalScale;
    private Vector3 markIconOriginalScale;

    private void Awake()
    {
        if (cellSprite == null) cellSprite = GetComponent<SpriteRenderer>();
        if (monsterSprite == null || markIcon == null)
            Debug.LogError($"[BoardCell] monsterSprite or markIcon is not assigned on {name}.", this);

        markIconOriginalScale = markIcon.transform.localScale;
        markIcon.enabled = false;
    }

    public void InitCell(int r, int c, System.Action<int, int> clickHandler, System.Func<bool> interactableCheck = null)
    {
        row = r;
        col = c;
        onClickCallback = clickHandler;
        canInteractCallback = interactableCheck;
        originalScale = Vector3.one;
    }

    public void InitCell(int r, int c, GameManager gm)
    {
        InitCell(r, c, gm.HandleCellClick, () => !gm.IsGameOver());
    }

    private int lastHandledFrame = -1;

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

    private void OnMouseDown()
    {
        HandleClick();
    }

    private void OnMouseEnter()
    {
        if (canInteractCallback == null || canInteractCallback.Invoke())
        {
            transform.DOKill();
            transform.DOScale(originalScale * 1.05f, 0.15f).SetEase(Ease.OutQuad).SetLink(gameObject);
        }
    }

    private void OnMouseExit()
    {
        transform.DOKill();
        transform.DOScale(originalScale, 0.15f).SetEase(Ease.OutQuad).SetLink(gameObject);
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

    public void SetMarkState(bool isMarked, Color biomeColor)
    {
        markIcon.transform.DOKill();

        if (isMarked)
        {
            markIcon.enabled = true;
            markIcon.transform.localScale = Vector3.zero;
            markIcon.transform.DOScale(markIconOriginalScale, 0.3f).SetEase(Ease.OutBack).SetLink(gameObject);
        }
        else
        {
            markIcon.enabled = false;
        }

        cellSprite.DOKill();
        transform.DOKill(complete: true);

        Color targetColor = isMarked ? new Color(biomeColor.r, biomeColor.g, biomeColor.b, 0.4f) : new Color(biomeColor.r, biomeColor.g, biomeColor.b, 1f);
        cellSprite.DOColor(targetColor, 0.15f).SetEase(Ease.OutQuad).SetLink(gameObject);
        transform.DOPunchScale(Vector3.one * 0.08f, 0.15f, 2, 0.5f).SetLink(gameObject);
    }

    public void SetMonsterState(bool hasMonster, Sprite sprite, Color biomeColor)
    {
        if (hasMonster)
        {
            monsterSprite.enabled = true;
            monsterSprite.color = Color.white;
            monsterSprite.sortingOrder = cellSprite.sortingOrder + 1;
            if (sprite != null) monsterSprite.sprite = sprite;
            markIcon.enabled = false;

            ScaleSpriteToFit();

            Vector3 targetScale = monsterSprite.transform.localScale;
            if (targetScale.x <= 0.001f || targetScale.y <= 0.001f)
            {
                targetScale = Vector3.one * 0.8f;
            }

            monsterSprite.transform.localScale = Vector3.zero;
            monsterSprite.transform.DOKill();
            monsterSprite.transform.DOScale(targetScale, 0.35f).SetEase(Ease.OutBack).SetLink(gameObject);
        }
        else
        {
            if (monsterSprite.enabled)
            {
                monsterSprite.transform.DOKill();
                monsterSprite.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).SetLink(gameObject).OnComplete(() => {
                    monsterSprite.enabled = false;
                });
            }
        }
    }

    public void ShowErrorSprite(Sprite errorSprite)
    {
        transform.DOKill();
        transform.DOShakePosition(0.4f, strength: new Vector3(0.1f, 0, 0), vibrato: 20, randomness: 90, snapping: false, fadeOut: true)
            .SetLink(gameObject)
            .OnComplete(() => {
                markIcon.enabled = false;

                monsterSprite.enabled = true;
                if (errorSprite != null) monsterSprite.sprite = errorSprite;
                ScaleSpriteToFit();

                Vector3 targetScale = monsterSprite.transform.localScale;
                if (targetScale.x <= 0.001f || targetScale.y <= 0.001f)
                {
                    targetScale = Vector3.one * 0.8f;
                }

                monsterSprite.transform.localScale = Vector3.zero;
                monsterSprite.transform.DOKill();
                monsterSprite.transform.DOScale(targetScale, 0.2f).SetEase(Ease.OutBack).SetLink(gameObject);
            });
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
