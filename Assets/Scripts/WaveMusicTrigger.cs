using UnityEngine;

public class WaveMusicTrigger : MonoBehaviour
{
    public AudioClip combatMusic;
    [Range(0f, 1f)] public float combatVolume = 1f;
    public float fadeDuration = 1f;

    public void OnWaveStarted(int _) => SoundManager.Instance?.PlayMusic(combatMusic, fadeDuration, combatVolume);
    public void OnWaveCompleted(int _) => SoundManager.Instance?.RestoreMusic(fadeDuration);
}
