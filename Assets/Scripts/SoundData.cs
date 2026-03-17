using UnityEngine;

[System.Serializable]
public class SoundData
{
    public string id;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Range(0f, 0.5f)]
    public float pitchVariation = 0.05f;

    public float cooldown = 0f; // optional
}