using UnityEngine;

[CreateAssetMenu(fileName = "GameTheme", menuName = "Monster Biome/Game Theme")]
public class GameTheme : ScriptableObject
{
    [Header("Biome (index 0 = empty cell)")]
    public Color[] biomeColors;
    public Sprite[] monsterSprites;
    public Sprite brokenHeartSprite;

    [Header("Timer States")]
    public Color timerNormal = new Color(0.06415099f, 0.05797797f, 0.05797797f, 1f);
    public Color timerWarning = Color.red;
    public Color timerFrozen = Color.cyan;
    public int timerWarningSeconds = 5;

    [Header("Cell States")]
    [Range(0f, 1f)] public float markedCellAlpha = 0.4f;
    public Color loseGray = Color.gray;

    private void OnValidate()
    {
        if (biomeColors == null || monsterSprites == null) return;
        if (biomeColors.Length != monsterSprites.Length)
            Debug.LogWarning($"[GameTheme] biomeColors ({biomeColors.Length}) and monsterSprites ({monsterSprites.Length}) lengths differ. Biome index pairs must match.", this);
    }

    public Color GetBiomeColor(int biomeID)
    {
        if (biomeColors == null || biomeID < 0 || biomeID >= biomeColors.Length)
        {
            Debug.LogError($"[GameTheme] biomeID {biomeID} is out of range (0..{(biomeColors?.Length ?? 0) - 1}). Check the level file and the theme asset.");
            return Color.white;
        }
        return biomeColors[biomeID];
    }

    public Sprite GetMonsterSprite(int biomeID)
    {
        if (monsterSprites == null || biomeID < 0 || biomeID >= monsterSprites.Length)
        {
            Debug.LogError($"[GameTheme] biomeID {biomeID} is out of range (0..{(monsterSprites?.Length ?? 0) - 1}). Check the level file and the theme asset.");
            return null;
        }
        return monsterSprites[biomeID];
    }
}
