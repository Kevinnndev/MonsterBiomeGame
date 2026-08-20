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
    private GameManager gameManager;
    private Vector3 originalScale;
    private Vector3 markIconOriginalScale;

    private void Awake()
    {
        markIconOriginalScale = markIcon.transform.localScale;
        markIcon.enabled = false;
    }

    public void InitCell(int r, int c, GameManager gm)
    {
        row = r;
        col = c;
        gameManager = gm;
        originalScale = Vector3.one;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        gameManager.HandleCellClick(row, col);
    }

    private void OnMouseEnter()
    {
        if (!gameManager.IsGameOver())
        {
            transform.DOKill();
            transform.DOScale(originalScale * 1.05f, 0.15f).SetEase(Ease.OutQuad);
        }
    }

    private void OnMouseExit()
    {
        transform.DOKill();
        transform.DOScale(originalScale, 0.15f).SetEase(Ease.OutQuad);
    }

    public void SetupCell(int biomeID, Color biomeColor)
    {
        Color color = new Color(biomeColor.r, biomeColor.g, biomeColor.b, 1f);

        if (biomeID == 0)
        {
            cellSprite.enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;
        }
        else
        {
            cellSprite.enabled = true;
            cellSprite.color = color;
            GetComponent<BoxCollider2D>().enabled = true;
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
            markIcon.transform.DOScale(markIconOriginalScale, 0.3f).SetEase(Ease.OutBack);
        }
        else
        {
            markIcon.enabled = false;
        }

        cellSprite.DOKill();
        transform.DOKill(complete: true);

        Color targetColor = isMarked ? new Color(biomeColor.r, biomeColor.g, biomeColor.b, 0.4f) : new Color(biomeColor.r, biomeColor.g, biomeColor.b, 1f);
        cellSprite.DOColor(targetColor, 0.15f).SetEase(Ease.OutQuad);
        transform.DOPunchScale(Vector3.one * 0.08f, 0.15f, 2, 0.5f);
    }

    public void SetMonsterState(bool hasMonster, Sprite sprite, Color biomeColor)
    {
        if (hasMonster)
        {
            monsterSprite.enabled = true;
            monsterSprite.sprite = sprite;
            markIcon.enabled = false;

            ScaleSpriteToFit();

            Vector3 targetScale = monsterSprite.transform.localScale;
            monsterSprite.transform.localScale = Vector3.zero;
            monsterSprite.transform.DOKill();
            monsterSprite.transform.DOScale(targetScale, 0.35f).SetEase(Ease.OutBack);
        }
        else
        {
            if (monsterSprite.enabled)
            {
                monsterSprite.transform.DOKill();
                monsterSprite.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() => {
                    monsterSprite.enabled = false;
                });
            }
        }
    }

    public void ShowErrorSprite(Sprite errorSprite)
    {
        transform.DOKill();
        transform.DOShakePosition(0.4f, strength: new Vector3(0.1f, 0, 0), vibrato: 20, randomness: 90, snapping: false, fadeOut: true)
            .OnComplete(() => {
                markIcon.enabled = false;

                monsterSprite.enabled = true;
                monsterSprite.sprite = errorSprite;
                ScaleSpriteToFit();

                Vector3 targetScale = monsterSprite.transform.localScale;
                monsterSprite.transform.localScale = Vector3.zero;
                monsterSprite.transform.DOKill();
                monsterSprite.transform.DOScale(targetScale, 0.2f).SetEase(Ease.OutBack);
            });
    }

    private void ScaleSpriteToFit()
    {
        if (monsterSprite.sprite == null || cellSprite.sprite == null) return;

        monsterSprite.transform.localScale = Vector3.one;

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
}