using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [Header("Toggle References")]
    public Toggle musicToggle;
    public Toggle soundToggle;
    public Toggle vibrateToggle;

    // Keep legacy overlay references for backward compatibility (can be removed later)
    [Header("Toggle Button Slash Overlays (Legacy)")]
    public GameObject musicSlashOverlay;
    public GameObject soundSlashOverlay;
    public GameObject vibrateSlashOverlay;

    // PlayerPrefs keys
    private const string KEY_MUSIC_OFF = "MusicOff";
    private const string KEY_SFX_OFF = "SFXOff";
    private const string KEY_VIBRATION_OFF = "VibrationOff";

    private bool isVibrationOff = false;

    public bool IsVibrationOff => isVibrationOff;

    private void Awake()
    {
        // Load persisted states (0 = not muted / on, 1 = muted / off)
        bool musicOff = PlayerPrefs.GetInt(KEY_MUSIC_OFF, 0) == 1;
        bool sfxOff = PlayerPrefs.GetInt(KEY_SFX_OFF, 0) == 1;
        isVibrationOff = PlayerPrefs.GetInt(KEY_VIBRATION_OFF, 0) == 1;

        // Set toggles without triggering listeners (listeners are added in OnEnable)
        if (musicToggle) musicToggle.SetIsOnWithoutNotify(!musicOff);
        if (soundToggle) soundToggle.SetIsOnWithoutNotify(!sfxOff);
        if (vibrateToggle) vibrateToggle.SetIsOnWithoutNotify(!isVibrationOff);
    }

    public void ToggleVibration(AudioManager audioManager)
    {
        audioManager?.PlayClick();
        isVibrationOff = !isVibrationOff;
        PlayerPrefs.SetInt(KEY_VIBRATION_OFF, isVibrationOff ? 1 : 0);
        PlayerPrefs.Save();
        if (!Application.isEditor && !isVibrationOff) Handheld.Vibrate();
    }

    public void SaveMusicState(bool isMuted)
    {
        PlayerPrefs.SetInt(KEY_MUSIC_OFF, isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SaveSFXState(bool isMuted)
    {
        PlayerPrefs.SetInt(KEY_SFX_OFF, isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void UpdateToggleButtonsUI(bool isMusicMuted, bool isSFXMuted)
    {
        // Update toggle visuals (SpriteSwitch listens to isOn changes)
        if (musicToggle) musicToggle.isOn = !isMusicMuted;
        if (soundToggle) soundToggle.isOn = !isSFXMuted;
        if (vibrateToggle) vibrateToggle.isOn = !isVibrationOff;

        // Legacy overlays (kept for backward compatibility)
        if (musicSlashOverlay) musicSlashOverlay.SetActive(isMusicMuted);
        if (soundSlashOverlay) soundSlashOverlay.SetActive(isSFXMuted);
        if (vibrateSlashOverlay) vibrateSlashOverlay.SetActive(isVibrationOff);
    }
}
