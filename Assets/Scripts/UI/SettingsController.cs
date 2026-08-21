using UnityEngine;

public class SettingsController : MonoBehaviour
{
    [Header("Toggle Button Slash Overlays")]
    public GameObject musicSlashOverlay;
    public GameObject soundSlashOverlay;
    public GameObject vibrateSlashOverlay;

    private bool isVibrationOff = false;

    public bool IsVibrationOff => isVibrationOff;

    public void ToggleVibration(AudioManager audioManager)
    {
        audioManager?.PlayClick();
        isVibrationOff = !isVibrationOff;
        if (!Application.isEditor && !isVibrationOff) Handheld.Vibrate();
    }

    public void UpdateToggleButtonsUI(bool isMusicMuted, bool isSFXMuted)
    {
        if (musicSlashOverlay) musicSlashOverlay.SetActive(isMusicMuted);
        if (soundSlashOverlay) soundSlashOverlay.SetActive(isSFXMuted);
        if (vibrateSlashOverlay) vibrateSlashOverlay.SetActive(isVibrationOff);
    }
}
