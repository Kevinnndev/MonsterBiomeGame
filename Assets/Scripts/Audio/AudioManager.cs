using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource bgmSource;

    [Header("Audio Clips")]
    public AudioClip clickSound;
    public AudioClip placeMonsterSound;
    public AudioClip errorSound;
    public AudioClip winSound;
    public AudioClip loseSound;

    private bool isMusicMuted = false;
    private bool isSFXMuted = false;

    public bool IsMusicMuted => isMusicMuted;
    public bool IsSFXMuted => isSFXMuted;

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource && clip) sfxSource.PlayOneShot(clip);
    }

    public void PlayClick() => PlaySFX(clickSound);
    public void PlayPlaceMonster() => PlaySFX(placeMonsterSound);
    public void PlayError() => PlaySFX(errorSound);
    public void PlayWin() => PlaySFX(winSound);
    public void PlayLose() => PlaySFX(loseSound);

    public void ToggleMusic()
    {
        PlayClick();
        isMusicMuted = !isMusicMuted;
        if (bgmSource) bgmSource.mute = isMusicMuted;
    }

    public void ToggleSFX()
    {
        isSFXMuted = !isSFXMuted;
        if (sfxSource) sfxSource.mute = isSFXMuted;
        if (!isSFXMuted) PlayClick();
    }
}
