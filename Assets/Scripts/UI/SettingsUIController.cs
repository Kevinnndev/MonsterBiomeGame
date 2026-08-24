using UnityEngine;

public class SettingsUIController : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private SettingsController settingsController;
    [SerializeField] private UIPanelManager uiPanelManager;

    private void Awake()
    {
        if (audioManager == null) audioManager = FindFirstObjectByType<AudioManager>(FindObjectsInactive.Include);
        if (settingsController == null) settingsController = FindFirstObjectByType<SettingsController>(FindObjectsInactive.Include);
        if (uiPanelManager == null) uiPanelManager = FindFirstObjectByType<UIPanelManager>(FindObjectsInactive.Include);

        if (audioManager == null || settingsController == null || uiPanelManager == null)
        {
            Debug.LogError($"[SettingsUIController] Missing dependency on {name}: audio={audioManager}, settings={settingsController}, panels={uiPanelManager}.", this);
        }
    }

    private void Start()
    {
        UpdateToggleButtonsUI();
    }

    public void OpenSettings()
    {
        PlayClick();
        UpdateToggleButtonsUI();
        uiPanelManager.ShowPanel(uiPanelManager.settingsPanel);
    }

    public void CloseSettings()
    {
        PlayClick();
        uiPanelManager.HidePanel(uiPanelManager.settingsPanel, false);
    }

    public void OpenHowToPlay()
    {
        PlayClick();
        uiPanelManager.ShowPanel(uiPanelManager.howToPlayPanel);
    }

    public void CloseHowToPlay()
    {
        PlayClick();
        uiPanelManager.HidePanel(uiPanelManager.howToPlayPanel, false);
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
