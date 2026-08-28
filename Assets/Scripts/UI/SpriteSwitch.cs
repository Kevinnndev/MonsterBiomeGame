using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class SpriteSwitch : MonoBehaviour
{
    [SerializeField] private Image switchImage;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;

    private Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        UpdateSprite(toggle.isOn);
    }

    private void OnEnable() => toggle.onValueChanged.AddListener(UpdateSprite);
    private void OnDisable() => toggle.onValueChanged.RemoveListener(UpdateSprite);

    private void UpdateSprite(bool isOn)
    {
        switchImage.sprite = isOn ? onSprite : offSprite;
    }
}
