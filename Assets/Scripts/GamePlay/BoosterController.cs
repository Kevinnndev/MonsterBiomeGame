using System;
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

    // View creates the Model in absence of a DI framework
    private readonly BoosterCore model = new BoosterCore();

    [Header("Dependencies")]
    [SerializeField] private BoardMoveExecutor moveExecutor;
    [SerializeField] private TimerController timerController;

    private Func<bool> gameOverProvider;
    private Tween delayedPlaceTween;

    public BoosterCore Model => model;
    
    // Properties passed-through for existing UI/Systems that might read them
    public BoosterType ActiveBooster => model.ActiveBooster;

    private void Awake()
    {
        model.OnBoosterCountsChanged += UpdateBoosterUI;
        
        // System to View/Dependency hooks
        model.OnAddFreezeTimeRequested += HandleAddFreezeTime;
        model.OnPlaceMonsterRequested += HandlePlaceMonster;
        model.OnToggleMarkRequested += HandleToggleMark;
        model.OnBoosterAnimationRequested += HandleBoosterAnimation;
    }

    private void OnDestroy()
    {
        model.OnBoosterCountsChanged -= UpdateBoosterUI;

        model.OnAddFreezeTimeRequested -= HandleAddFreezeTime;
        model.OnPlaceMonsterRequested -= HandlePlaceMonster;
        model.OnToggleMarkRequested -= HandleToggleMark;
        model.OnBoosterAnimationRequested -= HandleBoosterAnimation;

        delayedPlaceTween?.Kill();
        delayedPlaceTween = null;
    }

    public void Initialize(Func<BoardState> stateProvider, Func<bool> gameOverCheck, BoardMoveExecutor executor, TimerController timer)
    {
        gameOverProvider = gameOverCheck;
        moveExecutor = executor;
        timerController = timer;
        
        model.Initialize(stateProvider);
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
        model.OnClickFindOne(IsGameOver());
    }

    private void HandleFreezeTimeBtnClick()
    {
        freezeTimeBtn.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0), 0.2f, 2, 0.5f);
        model.OnClickFreezeTime(IsGameOver());
    }

    private void HandleRocketBtnClick()
    {
        rocketBtn.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0), 0.2f, 2, 0.5f);
        model.OnClickRocket(IsGameOver());
    }

    private void HandleBowBtnClick()
    {
        bowBtn.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0), 0.2f, 2, 0.5f);
        model.OnClickBow(IsGameOver());
    }

    public void HandleCellClickWithBooster(int row, int col) => model.HandleCellClickWithBooster(row, col);
    public void ClearActiveBooster() => model.ClearActiveBooster();

    public void UpdateBoosterUI()
    {
        findOneBtn.interactable = (model.FindOneCount > 0);
        freezeTimeBtn.interactable = (model.FreezeTimeCount > 0);
        rocketBtn.interactable = (model.RocketCount > 0);
        bowBtn.interactable = (model.BowCount > 0);
    }

    // --- Action Handlers from Model ---
    
    private void HandleAddFreezeTime(float amount)
    {
        timerController?.AddFreezeTime(amount);
    }

    private void HandlePlaceMonster(int row, int col, int biomeID)
    {
        moveExecutor?.PlaceMonsterAt(row, col, biomeID);
    }

    private void HandleToggleMark(int row, int col, int biomeID)
    {
        moveExecutor?.ToggleMark(row, col, biomeID);
    }

    private void HandleBoosterAnimation(int r, int c, BoosterType type, Action onComplete)
    {
        delayedPlaceTween?.Kill();
        delayedPlaceTween = DOVirtual.DelayedCall(0.4f, () =>
        {
            delayedPlaceTween = null;
            if (IsGameOver()) return;
            onComplete?.Invoke();
        });
    }
}
