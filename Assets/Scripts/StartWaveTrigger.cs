using UnityEngine;

public class StartWaveTrigger : MonoBehaviour
{
    [Tooltip("0 = wave 1, 1 = wave 2, etc.")]
    public int waveIndex;

    public void StartWave()
    {
        WavesManager.Instance?.StartWave(waveIndex);
    }
}
