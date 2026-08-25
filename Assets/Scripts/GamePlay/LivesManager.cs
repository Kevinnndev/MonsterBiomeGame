using System;
using UnityEngine;
using MonsterBiome.Core.Models;

public class LivesManager : MonoBehaviour
{
    [Header("Lives Display")]
    public TMPro.TextMeshProUGUI livesCountText;

    private readonly LivesCore model = new LivesCore();

    public event Action OnLivesDepleted
    {
        add => model.OnLivesDepleted += value;
        remove => model.OnLivesDepleted -= value;
    }

    public int Lives => model.Lives;

    private void Awake()
    {
        model.OnLivesChanged += UpdateLivesUI;
    }

    private void OnDestroy()
    {
        model.OnLivesChanged -= UpdateLivesUI;
    }

    public void ResetLives(int initialLives = 3)
    {
        model.ResetLives(initialLives);
    }

    public void DeductLife()
    {
        model.DeductLife();
    }

    private void UpdateLivesUI(int currentLives)
    {
        if (livesCountText)
        {
            livesCountText.text = "x" + currentLives;
        }
    }
}
