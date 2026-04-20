using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Mixer")]
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup musicGroup;

    [Header("SFX Pool")]
    public int poolSize = 12;

    private List<AudioSource> sfxSources = new();
    private Dictionary<string, float> lastPlayTime = new();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < poolSize; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = sfxGroup;
            src.playOnAwake = false;
            sfxSources.Add(src);
        }
    }

    public void PlaySFX(SoundData sound)
    {
        if (sound == null || sound.clip == null)
            return;

        // Cooldown check (for footsteps etc.)
        if (sound.cooldown > 0f)
        {
            if (lastPlayTime.TryGetValue(sound.id, out float lastTime))
            {
                if (Time.time - lastTime < sound.cooldown)
                    return;
            }

            lastPlayTime[sound.id] = Time.time;
        }

        AudioSource src = GetFreeSource();
        if (src == null) return;

        src.clip = sound.clip;
        src.volume = sound.volume;
        src.pitch = 1f + Random.Range(-sound.pitchVariation, sound.pitchVariation);
        src.Play();
    }

    private AudioSource GetFreeSource()
    {
        foreach (var src in sfxSources)
        {
            if (!src.isPlaying)
                return src;
        }

        // Steal oldest sound (better than failing silently)
        return sfxSources[0];
    }

    // --- Music ---

    [Header("Music")]
    public AudioSource musicSource;
    private Coroutine musicTransitionCoroutine;
    private AudioClip previousClip;
    private float previousVolume = 1f;
    private float previousPitch = 1f;

    public void PlayMusic(AudioClip clip, float fadeDuration = 1f, float targetVolume = 1f)
    {
        if (musicSource == null) return;

        if (musicTransitionCoroutine != null)
            StopCoroutine(musicTransitionCoroutine);

        previousClip = musicSource.clip;
        previousVolume = musicSource.volume;
        previousPitch = musicSource.pitch;
        musicTransitionCoroutine = StartCoroutine(CrossfadeMusic(clip, musicSource.volume, targetVolume, 1f, fadeDuration));
    }

    public void RestoreMusic(float fadeDuration = 1f)
    {
        if (previousClip == null) return;

        if (musicTransitionCoroutine != null)
            StopCoroutine(musicTransitionCoroutine);

        musicTransitionCoroutine = StartCoroutine(CrossfadeMusic(previousClip, musicSource.volume, previousVolume, previousPitch, fadeDuration));
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip, float startVolume, float targetVolume, float targetPitch, float duration)
    {
        // Fade out
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        musicSource.clip = newClip;
        musicSource.pitch = targetPitch;
        musicSource.Play();

        // Fade in to target volume
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }

        musicSource.volume = targetVolume;
        musicTransitionCoroutine = null;
    }
}