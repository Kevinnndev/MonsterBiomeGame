using UnityEngine;

/// <summary>
/// Settings controller for the MAIN MENU full setting screen (settingScreen).
/// Không liên quan đến timer. Gắn vào SettingButton trên MainMenu.
/// </summary>
public class MainMenuSettingsController : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private SettingsController settingsController;
    [SerializeField] private UIPanelManager uiPanelManager;

    private void EnsureDependencies()
    {
        if (audioManager == null) audioManager = FindFirstObjectByType<AudioManager>(FindObjectsInactive.Include);
        if (settingsController == null) settingsController = FindFirstObjectByType<SettingsController>(FindObjectsInactive.Include);
        if (uiPanelManager == null) uiPanelManager = FindFirstObjectByType<UIPanelManager>(FindObjectsInactive.Include);
    }

    /// <summary>Mở SettingScreen từ main menu — ẩn main menu, hiện setting screen.</summary>
    public void OpenSettingScreen()
    {
        EnsureDependencies();
        PlayClick();
        UpdateToggleButtonsUI();
        uiPanelManager.mainMenuUI.SetActive(false);
        uiPanelManager.ShowPanel(uiPanelManager.settingScreen);
    }

    /// <summary>Đóng SettingScreen — quay về main menu.</summary>
    public void CloseSettingScreen()
    {
        EnsureDependencies();
        PlayClick();
        uiPanelManager.HidePanel(uiPanelManager.settingScreen);
        uiPanelManager.mainMenuUI.SetActive(true);
    }

    public void ToggleMusic()
    {
        EnsureDependencies();
        audioManager.ToggleMusic();
        UpdateToggleButtonsUI();
    }

    public void ToggleSFX()
    {
        EnsureDependencies();
        audioManager.ToggleSFX();
        UpdateToggleButtonsUI();
    }

    public void ToggleVibration()
    {
        EnsureDependencies();
        settingsController.ToggleVibration(audioManager);
        UpdateToggleButtonsUI();
    }

    public void UpdateToggleButtonsUI()
    {
        EnsureDependencies();
        settingsController.UpdateToggleButtonsUI(audioManager.IsMusicMuted, audioManager.IsSFXMuted);
    }

    private void PlayClick()
    {
        if (audioManager != null) audioManager.PlayClick();
    }
}
