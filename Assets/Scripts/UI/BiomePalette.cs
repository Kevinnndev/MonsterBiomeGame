using UnityEngine;

public class BiomePalette : MonoBehaviour
{
    [Header("UI & Asset References")]
    public Color[] biomeColors;
    public Sprite[] monsterSprites;
    public Sprite brokenHeartSprite;

    public Color GetBiomeColor(int biomeID)
    {
        if (biomeColors == null || biomeColors.Length <= 1) return Color.white;
        if (biomeID < biomeColors.Length) return biomeColors[biomeID];
        int safeIndex = ((biomeID - 1) % (biomeColors.Length - 1)) + 1;
        return biomeColors[safeIndex];
    }

    public Sprite GetMonsterSprite(int biomeID)
    {
        if (monsterSprites == null || monsterSprites.Length <= 1) return null;
        if (biomeID < monsterSprites.Length) return monsterSprites[biomeID];
        int safeIndex = ((biomeID - 1) % (monsterSprites.Length - 1)) + 1;
        return monsterSprites[safeIndex];
    }
}
