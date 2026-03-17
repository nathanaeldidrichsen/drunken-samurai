using UnityEngine;
using UnityEngine.Audio;
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
}