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

    private readonly BoosterCore model = new BoosterCore();

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
    }

    private void OnDestroy()
    {
        model.OnBoosterCountsChanged -= UpdateBoosterUI;
    }

    public void ResetBoosters(int findOne = 1, int freezeTime = 1, int rocket = 1, int bow = 1)
    {
        model.ResetBoosters(findOne, freezeTime, rocket, bow);
    }

    private void Start()
    {
        EnsureBoosterButtons();
    }

    public void EnsureBoosterButtons(GameObject panel = null)
    {
        Transform root = panel != null ? panel.transform : transform;
        Button[] sceneButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (findOneBtn == null) findOneBtn = FindButton(root, "FindOneBtn", "Find One", "FindOne", "BtnFindOne", "FindOneButton") ?? MatchButton(sceneButtons, "FindOneBtn", "Find One", "FindOne");
        if (freezeTimeBtn == null) freezeTimeBtn = FindButton(root, "FreezeTimeBtn", "Freeze Time", "FreezeTime", "BtnFreezeTime", "FreezeTimeButton") ?? MatchButton(sceneButtons, "FreezeTimeBtn", "Freeze Time", "FreezeTime");
        if (rocketBtn == null) rocketBtn = FindButton(root, "RocketBtn", "Rocket", "RocketButton", "BtnRocket") ?? MatchButton(sceneButtons, "RocketBtn", "Rocket");
        if (bowBtn == null) bowBtn = FindButton(root, "BowBtn", "Bow", "BowButton", "BtnBow") ?? MatchButton(sceneButtons, "BowBtn", "Bow");

        BindButton(findOneBtn, HandleFindOneBtnClick);
        BindButton(freezeTimeBtn, HandleFreezeTimeBtnClick);
        BindButton(rocketBtn, HandleRocketBtnClick);
        BindButton(bowBtn, HandleBowBtnClick);

        UpdateBoosterUI();
    }

    private Button FindButton(Transform root, params string[] possibleNames)
    {
        if (root == null) return null;
        foreach (string name in possibleNames)
        {
            Transform t = root.Find(name);
            if (t == null)
            {
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        t = child;
                        break;
                    }
                }
            }
            if (t != null)
            {
                Button btn = t.GetComponent<Button>();
                if (btn != null) return btn;
            }
        }
        return null;
    }

    private Button MatchButton(Button[] sceneButtons, params string[] possibleNames)
    {
        if (sceneButtons == null) return null;
        foreach (var btn in sceneButtons)
        {
            if (btn == null) continue;
            foreach (string name in possibleNames)
            {
                if (btn.gameObject.name.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                    btn.gameObject.name.Replace(" ", "").Equals(name.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
                {
                    return btn;
                }
            }
        }
        foreach (var btn in sceneButtons)
        {
            if (btn == null) continue;
            var tmp = btn.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
            if (tmp != null)
            {
                foreach (string name in possibleNames)
                {
                    if (tmp.text.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                        tmp.text.Replace(" ", "").Equals(name.Replace(" ", ""), StringComparison.OrdinalIgnoreCase))
                    {
                        return btn;
                    }
                }
            }
        }
        return null;
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

    private void HandleFindOneBtnClick()
    {
        if (findOneBtn) findOneBtn.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0), 0.2f, 2, 0.5f);
        OnClickFindOne(false);
    }

    private void HandleFreezeTimeBtnClick()
    {
        if (freezeTimeBtn) freezeTimeBtn.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0), 0.2f, 2, 0.5f);
        OnClickFreezeTime(false);
    }

    private void HandleRocketBtnClick()
    {
        if (rocketBtn) rocketBtn.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0), 0.2f, 2, 0.5f);
        OnClickRocket(false);
    }

    private void HandleBowBtnClick()
    {
        if (bowBtn) bowBtn.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0), 0.2f, 2, 0.5f);
        OnClickBow(false);
    }

    public void OnClickFindOne(bool isGameOver = false) => model.OnClickFindOne(isGameOver);
    public void ConsumeFindOne() => model.ConsumeFindOne();
    public void OnClickFreezeTime(bool isGameOver = false) => model.OnClickFreezeTime(isGameOver);
    public void OnClickRocket(bool isGameOver = false) => model.OnClickRocket(isGameOver);
    public void OnClickBow(bool isGameOver = false) => model.OnClickBow(isGameOver);
    public void HandleCellClickWithBooster(int row, int col) => model.HandleCellClickWithBooster(row, col);
    public void ClearActiveBooster() => model.ClearActiveBooster();

    public void UpdateBoosterUI()
    {
        if (findOneBtn) findOneBtn.interactable = (model.FindOneCount > 0);
        if (freezeTimeBtn) freezeTimeBtn.interactable = (model.FreezeTimeCount > 0);
        if (rocketBtn) rocketBtn.interactable = (model.RocketCount > 0);
        if (bowBtn) bowBtn.interactable = (model.BowCount > 0);
    }
}
