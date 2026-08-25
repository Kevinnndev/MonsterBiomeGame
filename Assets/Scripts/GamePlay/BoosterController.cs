using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] private GameTheme theme;

    private Func<bool> gameOverProvider;
    private Coroutine delayedPlaceRoutine;

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

        if (delayedPlaceRoutine != null)
        {
            StopCoroutine(delayedPlaceRoutine);
            delayedPlaceRoutine = null;
        }
    }

    public void Initialize(Func<BoardState> stateProvider, Func<bool> gameOverCheck, BoardMoveExecutor executor, TimerController timer, GameTheme gameTheme)
    {
        gameOverProvider = gameOverCheck;
        moveExecutor = executor;
        timerController = timer;
        theme = gameTheme;

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
        ButtonFx.Punch(findOneBtn.transform);
        model.OnClickFindOne(IsGameOver());
    }

    private void HandleFreezeTimeBtnClick()
    {
        ButtonFx.Punch(freezeTimeBtn.transform);
        model.OnClickFreezeTime(IsGameOver());
    }

    private void HandleRocketBtnClick()
    {
        ButtonFx.Punch(rocketBtn.transform);
        model.OnClickRocket(IsGameOver());
    }

    private void HandleBowBtnClick()
    {
        ButtonFx.Punch(bowBtn.transform);
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
    
    private void HandleAddFreezeTime()
    {
        timerController.AddFreezeTime(theme.freezeTimeSeconds);
    }

    private void HandlePlaceMonster(int row, int col, int biomeID)
    {
        moveExecutor?.PlaceMonsterAt(row, col, biomeID);
    }

    private void HandleToggleMark(int row, int col, int biomeID)
    {
        moveExecutor?.ToggleMark(row, col, biomeID);
    }

    private void HandleBoosterAnimation(Action onComplete)
    {
        if (delayedPlaceRoutine != null) StopCoroutine(delayedPlaceRoutine);
        delayedPlaceRoutine = StartCoroutine(DelayedPlaceTimeline(onComplete));
    }

    private IEnumerator DelayedPlaceTimeline(Action onComplete)
    {
        yield return new WaitForSeconds(0.4f);
        delayedPlaceRoutine = null;
        if (IsGameOver()) yield break;
        onComplete?.Invoke();
    }
}
