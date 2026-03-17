using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
    public float volume = 1;
    public AudioClip musicClip; // Reference to the music clip
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            // Call the method to change the game music in the SoundManager
            // SoundManager.Instance.ChangeGameMusic(musicClip, volume);
            hasTriggered = true;
        }
    }
}
