using System;
using UnityEngine;
using TMPro;
using MonsterBiome.Core.Models;

public class TimerController : MonoBehaviour
{
    [Header("Timer Display")]
    public TextMeshProUGUI timerText;

    [Header("Theme")]
    [SerializeField] private GameTheme theme;

    private readonly TimerCore model = new TimerCore();

    public event Action OnTimerExpired
    {
        add => model.OnTimerExpired += value;
        remove => model.OnTimerExpired -= value;
    }

    private int lastDisplayedSeconds = -1;
    private Color lastDisplayedColor = Color.clear;

    private void Awake()
    {
        if (theme == null) Debug.LogError("[TimerController] GameTheme asset is not assigned in the Inspector.", this);
        model.OnTimerTick += HandleTimerTick;
    }

    private void OnDestroy()
    {
        model.OnTimerTick -= HandleTimerTick;
    }

    public void StartTimer(float timeLimitSeconds)
    {
        lastDisplayedSeconds = -1;
        lastDisplayedColor = Color.clear;
        model.StartTimer(timeLimitSeconds);
    }

    public void StopTimer()
    {
        model.StopTimer();
    }

    public void AddFreezeTime(float seconds)
    {
        model.AddFreezeTime(seconds);
    }

    private void Update()
    {
        model.Tick(Time.deltaTime);
    }

    private void HandleTimerTick(float currentTime, bool isFrozen)
    {
        int secondsLeft = Mathf.CeilToInt(Mathf.Max(0, currentTime));
        Color timerColor = isFrozen ? theme.timerFrozen
            : (secondsLeft <= theme.timerWarningSeconds ? theme.timerWarning : theme.timerNormal);
        UpdateTimerDisplay(secondsLeft, timerColor);
    }

    private void UpdateTimerDisplay(int secondsLeft, Color textColor)
    {
        if (timerText == null) return;
        if (secondsLeft == lastDisplayedSeconds && textColor == lastDisplayedColor) return;

        lastDisplayedSeconds = secondsLeft;
        lastDisplayedColor = textColor;

        int minutes = secondsLeft / 60;
        int seconds = secondsLeft % 60;
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        timerText.color = textColor;
    }
}
