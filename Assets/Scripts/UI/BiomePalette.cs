using UnityEngine;

public class BiomePalette : MonoBehaviour
{
    [Header("UI & Asset References")]
    public Color[] biomeColors;
    public Sprite[] monsterSprites;
    public Sprite brokenHeartSprite;

    private void Awake()
    {
        if (biomeColors == null || biomeColors.Length == 0)
            Debug.LogError($"[BiomePalette] biomeColors is empty on {name}. Biome colors will render as white.", this);
        if (monsterSprites == null || monsterSprites.Length == 0)
            Debug.LogError($"[BiomePalette] monsterSprites is empty on {name}. Placed monsters will be invisible.", this);
        if (brokenHeartSprite == null)
            Debug.LogError($"[BiomePalette] brokenHeartSprite is not assigned on {name}.", this);
    }

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
