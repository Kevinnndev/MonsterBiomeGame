using UnityEngine;

/// <summary>
/// Settings controller for the GAMEPLAY overlay panel (settingsPanel).
/// Pauses/resumes timer. Gắn vào nút Settings trong TopBar khi đang chơi.
/// </summary>
public class SettingsUIController : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private SettingsController settingsController;
    [SerializeField] private UIPanelManager uiPanelManager;
    [SerializeField] private TimerController timerController;

    private void Awake()
    {
        if (audioManager == null) audioManager = FindFirstObjectByType<AudioManager>(FindObjectsInactive.Include);
        if (settingsController == null) settingsController = FindFirstObjectByType<SettingsController>(FindObjectsInactive.Include);
        if (uiPanelManager == null) uiPanelManager = FindFirstObjectByType<UIPanelManager>(FindObjectsInactive.Include);
        if (timerController == null) timerController = FindFirstObjectByType<TimerController>(FindObjectsInactive.Include);

        if (audioManager == null || settingsController == null || uiPanelManager == null || timerController == null)
        {
            Debug.LogError($"[SettingsUIController] Missing dependency on {name}: audio={audioManager}, settings={settingsController}, panels={uiPanelManager}, timer={timerController}.", this);
        }
    }

    private void Start()
    {
        UpdateToggleButtonsUI();
    }

    /// <summary>Mở settings panel trong gameplay — pause timer.</summary>
    public void OpenSettings()
    {
        PlayClick();
        timerController.PauseTimer();
        UpdateToggleButtonsUI();
        uiPanelManager.ShowPanel(uiPanelManager.settingsPanel);
    }

    /// <summary>Đóng settings panel trong gameplay — resume timer.</summary>
    public void CloseSettings()
    {
        PlayClick();
        timerController.ResumeTimer();
        uiPanelManager.HidePanel(uiPanelManager.settingsPanel);
    }

    public void OpenHowToPlay()
    {
        PlayClick();
        uiPanelManager.ShowPanel(uiPanelManager.howToPlayPanel);
    }

    public void CloseHowToPlay()
    {
        PlayClick();
        uiPanelManager.HidePanel(uiPanelManager.howToPlayPanel);
    }

    public void ToggleMusic()
    {
        audioManager.ToggleMusic();
        UpdateToggleButtonsUI();
    }

    public void ToggleSFX()
    {
        audioManager.ToggleSFX();
        UpdateToggleButtonsUI();
    }

    public void ToggleVibration()
    {
        settingsController.ToggleVibration(audioManager);
        UpdateToggleButtonsUI();
    }

    public void UpdateToggleButtonsUI()
    {
        settingsController.UpdateToggleButtonsUI(audioManager.IsMusicMuted, audioManager.IsSFXMuted);
    }

    private void PlayClick()
    {
        audioManager.PlayClick();
    }
}
