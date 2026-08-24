using System;
using UnityEngine;
using MonsterBiome.Core.Models;

public class BoardInputController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BoosterController boosterController;

    [Header("Gesture")]
    [SerializeField] private float doubleClickThreshold = 0.3f;

    private Func<BoardState> boardStateProvider;
    private Func<bool> gameOverProvider;

    private float lastClickTime = 0f;
    private int lastRow = -1;
    private int lastCol = -1;

    public event Action<int, int> RemoveRequested;
    public event Action<int, int, int> PlaceRequested;
    public event Action<int, int, int> MarkRequested;
    public event Action ClickSoundRequested;

    public void Initialize(Func<BoardState> stateProvider, Func<bool> gameOverCheck, BoosterController booster)
    {
        boardStateProvider = stateProvider;
        gameOverProvider = gameOverCheck;
        boosterController = booster;
    }

    public void HandleCellClick(int row, int col)
    {
        BoardState state = boardStateProvider?.Invoke();
        if ((gameOverProvider != null && gameOverProvider.Invoke()) || state == null) return;

        if (boosterController.ActiveBooster != BoosterType.None)
        {
            boosterController.HandleCellClickWithBooster(row, col);
            return;
        }

        if (state.IsErrorCell(row, col)) return;

        int biomeID = state.GridData[row, col];
        if (biomeID == 0) return;

        if (state.IsPlacedMonster(row, col))
        {
            RemoveRequested?.Invoke(row, col);
            ClickSoundRequested?.Invoke();
            return;
        }

        float timeSinceLastClick = Time.time - lastClickTime;
        if (timeSinceLastClick <= doubleClickThreshold && lastRow == row && lastCol == col)
        {
            PlaceRequested?.Invoke(row, col, biomeID);
            lastClickTime = 0f;
        }
        else
        {
            MarkRequested?.Invoke(row, col, biomeID);
            ClickSoundRequested?.Invoke();
            lastClickTime = Time.time;
            lastRow = row;
            lastCol = col;
        }
    }
}
