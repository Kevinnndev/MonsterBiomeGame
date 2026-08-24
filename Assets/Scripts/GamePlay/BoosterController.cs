using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using MonsterBiome.Core.Models;

public class BoosterController : MonoBehaviour
{
    [Header("Booster Buttons")]
    public Button findOneBtn;
    public Button freezeTimeBtn;
    public Button rocketBtn;
    public Button bowBtn;

    private readonly BoosterCore model = new BoosterCore();

    [Header("Dependencies")]
    [SerializeField] private BoardMoveExecutor moveExecutor;
    [SerializeField] private TimerController timerController;

    private Func<BoardState> boardStateProvider;
    private Func<bool> gameOverProvider;
    private Tween delayedPlaceTween;

    public BoosterCore Model => model;
    public BoosterType ActiveBooster => model.ActiveBooster;
    public int findOneCount => model.FindOneCount;
    public int freezeTimeCount => model.FreezeTimeCount;
    public int rocketCount => model.RocketCount;
    public int bowCount => model.BowCount;

    public event Action OnFindOneRequested
    {
        add => model.OnFindOneRequested += value;
        remove => model.OnFindOneRequested -= value;
    }

    public event Action OnFreezeTimeRequested
    {
        add => model.OnFreezeTimeRequested += value;
        remove => model.OnFreezeTimeRequested -= value;
    }

    public event Action<int, int, BoosterType> OnBoosterTargetClicked
    {
        add => model.OnBoosterTargetClicked += value;
        remove => model.OnBoosterTargetClicked -= value;
    }

    private void Awake()
    {
        model.OnBoosterCountsChanged += UpdateBoosterUI;
        model.OnFindOneRequested += HandleFindOne;
        model.OnFreezeTimeRequested += HandleFreezeTime;
        model.OnBoosterTargetClicked += ProcessBoosterTarget;
    }

    private void OnDestroy()
    {
        model.OnBoosterCountsChanged -= UpdateBoosterUI;
        model.OnFindOneRequested -= HandleFindOne;
        model.OnFreezeTimeRequested -= HandleFreezeTime;
        model.OnBoosterTargetClicked -= ProcessBoosterTarget;

        delayedPlaceTween?.Kill();
        delayedPlaceTween = null;
    }

    public void Initialize(Func<BoardState> stateProvider, Func<bool> gameOverCheck, BoardMoveExecutor executor, TimerController timer)
    {
        boardStateProvider = stateProvider;
        gameOverProvider = gameOverCheck;
        moveExecutor = executor;
        timerController = timer;
    }

    public void ResetBoosters(int findOne = 1, int freezeTime = 1, int rocket = 1, int bow = 1)
    {
        model.ResetBoosters(findOne, freezeTime, rocket, bow);
    }

    private void Start()
    {
        EnsureBoosterButtons();
    }

    public void EnsureBoosterButtons()
    {
        if (findOneBtn == null || freezeTimeBtn == null || rocketBtn == null || bowBtn == null)
        {
            Debug.LogError($"[BoosterController] Booster buttons are not fully assigned on {name}. Assign them in the Inspector.", this);
        }

        BindButton(findOneBtn, HandleFindOneBtnClick);
        BindButton(freezeTimeBtn, HandleFreezeTimeBtnClick);
        BindButton(rocketBtn, HandleRocketBtnClick);
        BindButton(bowBtn, HandleBowBtnClick);

        UpdateBoosterUI();
    }

    private void BindButton(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null || action == null) return;
        if (btn.targetGraphic != null)
        {
            btn.targetGraphic.raycastTarget = true;
        }
        btn.onClick.RemoveListener(action);
        btn.onClick.AddListener(action);
    }

    private bool IsGameOver() => gameOverProvider != null && gameOverProvider.Invoke();

    private void HandleFindOneBtnClick()
    {
        findOneBtn.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0), 0.2f, 2, 0.5f);
        OnClickFindOne(IsGameOver());
    }

    private void HandleFreezeTimeBtnClick()
    {
        freezeTimeBtn.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0), 0.2f, 2, 0.5f);
        OnClickFreezeTime(IsGameOver());
    }

    private void HandleRocketBtnClick()
    {
        rocketBtn.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0), 0.2f, 2, 0.5f);
        OnClickRocket(IsGameOver());
    }

    private void HandleBowBtnClick()
    {
        bowBtn.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0), 0.2f, 2, 0.5f);
        OnClickBow(IsGameOver());
    }

    public void OnClickFindOne(bool isGameOver = false) => model.OnClickFindOne(isGameOver);
    public void ConsumeFindOne() => model.ConsumeFindOne();
    public void OnClickFreezeTime(bool isGameOver = false) => model.OnClickFreezeTime(isGameOver);
    public void OnClickRocket(bool isGameOver = false) => model.OnClickRocket(isGameOver);
    public void OnClickBow(bool isGameOver = false) => model.OnClickBow(isGameOver);
    public void HandleCellClickWithBooster(int row, int col) => model.HandleCellClickWithBooster(row, col);
    public void ClearActiveBooster() => model.ClearActiveBooster();

    private void HandleFindOne()
    {
        BoardState state = boardStateProvider?.Invoke();
        if (state == null) return;

        var allCells = new List<(int, int)>();
        for (int r = 0; r < state.Rows; r++)
            for (int c = 0; c < state.Cols; c++)
                allCells.Add((r, c));

        if (TryAutoPlaceInScope(allCells))
        {
            model.ConsumeFindOne();
        }
    }

    private void HandleFreezeTime()
    {
        timerController.AddFreezeTime(15f);
    }

    private void ProcessBoosterTarget(int targetRow, int targetCol, BoosterType boosterType)
    {
        BoardState state = boardStateProvider?.Invoke();
        if (state == null) return;

        var scope = new List<(int, int)>();

        if (boosterType == BoosterType.Rocket)
        {
            for (int r = 0; r < state.Rows; r++)
                scope.Add((r, targetCol));
        }
        else if (boosterType == BoosterType.Bow)
        {
            for (int c = 0; c < state.Cols; c++)
                scope.Add((targetRow, c));
        }

        (int row, int col)? correctCell = null;
        foreach (var (row, col) in scope)
        {
            if (state.SolutionCells[row, col] && state.PlacedMonsters[row, col] == 0)
            {
                correctCell = (row, col);
                break;
            }
        }

        foreach (var (row, col) in scope)
        {
            bool isCorrect = correctCell != null && row == correctCell.Value.row && col == correctCell.Value.col;
            bool isEmpty = state.GridData[row, col] == 0;
            bool alreadyPlaced = state.PlacedMonsters[row, col] == 1;

            if (!isCorrect && !isEmpty && !alreadyPlaced)
            {
                moveExecutor.ToggleMark(row, col, state.GridData[row, col]);
            }
        }

        if (correctCell != null)
        {
            delayedPlaceTween?.Kill();
            BoardState targetBoardState = state;

            delayedPlaceTween = DOVirtual.DelayedCall(0.4f, () =>
            {
                delayedPlaceTween = null;
                bool over = gameOverProvider.Invoke();
                BoardState current = boardStateProvider.Invoke();
                if (over || current == null || current != targetBoardState) return;
                var (r, c) = correctCell.Value;
                TryAutoPlaceInScope(new List<(int, int)> { (r, c) });
            });
        }
    }

    private bool TryAutoPlaceInScope(IEnumerable<(int row, int col)> candidateCells)
    {
        BoardState state = boardStateProvider.Invoke();

        foreach (var (row, col) in candidateCells)
        {
            if (state.SolutionCells[row, col] && state.PlacedMonsters[row, col] == 0)
            {
                int biomeID = state.GridData[row, col];
                if (biomeID != 0)
                {
                    moveExecutor.PlaceMonsterAt(row, col, biomeID);
                    return true;
                }
            }
        }
        return false;
    }

    public void UpdateBoosterUI()
    {
        findOneBtn.interactable = (model.FindOneCount > 0);
        freezeTimeBtn.interactable = (model.FreezeTimeCount > 0);
        rocketBtn.interactable = (model.RocketCount > 0);
        bowBtn.interactable = (model.BowCount > 0);
    }
}
