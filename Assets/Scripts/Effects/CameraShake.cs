using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<CameraShake>();
            return instance;
        }
    }
    private static CameraShake instance;

    [Header("Noise Shake")]
    [SerializeField] private NoiseSettings defaultNoiseProfile;
    [SerializeField] private float baseAmplitude = 0.6f;
    [SerializeField] private float baseFrequency = 2.2f;
    [SerializeField] private float shakeDuration = 0.08f;

    private CinemachineBrain brain;
    private Camera mainCamera;
    private Coroutine shakeRoutine;
    private CinemachineBasicMultiChannelPerlin activeNoise;
    private float activeOriginalAmplitude;
    private float activeOriginalFrequency;
    private bool hasActiveOriginals;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        mainCamera = Camera.main;
        if (mainCamera != null)
            brain = mainCamera.GetComponent<CinemachineBrain>();
    }

    private void OnDisable()
    {
        StopAndResetActiveShake();
    }

    private void OnDestroy()
    {
        StopAndResetActiveShake();
    }

    public void ScreenShake()
    {
        ScreenShake(Vector2.zero, 1f);
    }

    // direction should represent hit direction (e.g. attack goes right => direction = Vector2.right)
    public void ScreenShake(Vector2 direction, float intensityScale = 1f)
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (brain == null && mainCamera != null)
            brain = mainCamera.GetComponent<CinemachineBrain>();

        if (brain == null)
            return;

        ICinemachineCamera activeCamera = brain.ActiveVirtualCamera;
        if (activeCamera == null)
            return;

        CinemachineVirtualCamera vcam = activeCamera as CinemachineVirtualCamera;
        if (vcam == null)
            return;

        CinemachineBasicMultiChannelPerlin noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (noise == null)
            noise = vcam.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        if (noise == null)
            return;

        if (noise.m_NoiseProfile == null && defaultNoiseProfile != null)
            noise.m_NoiseProfile = defaultNoiseProfile;

        if (noise.m_NoiseProfile == null)
        {
            Debug.LogWarning("CameraShake: No Noise Profile on active Cinemachine virtual camera. Assign one in vcam or CameraShake.defaultNoiseProfile.");
            return;
        }

        StopAndResetActiveShake();

        float targetAmplitude = Mathf.Max(0f, baseAmplitude * intensityScale);
        float targetFrequency = Mathf.Max(0f, baseFrequency);
        shakeRoutine = StartCoroutine(NoiseShakeRoutine(noise, targetAmplitude, targetFrequency, shakeDuration));
    }

    private void StopAndResetActiveShake()
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
        }

        RestoreActiveNoise();
    }

    private void RestoreActiveNoise()
    {
        if (activeNoise != null && hasActiveOriginals)
        {
            activeNoise.m_AmplitudeGain = activeOriginalAmplitude;
            activeNoise.m_FrequencyGain = activeOriginalFrequency;
        }

        activeNoise = null;
        hasActiveOriginals = false;
    }

    private System.Collections.IEnumerator NoiseShakeRoutine(
        CinemachineBasicMultiChannelPerlin noise,
        float targetAmplitude,
        float targetFrequency,
        float duration)
    {
        float originalAmplitude = noise.m_AmplitudeGain;
        float originalFrequency = noise.m_FrequencyGain;

        activeNoise = noise;
        activeOriginalAmplitude = originalAmplitude;
        activeOriginalFrequency = originalFrequency;
        hasActiveOriginals = true;

        noise.m_AmplitudeGain = Mathf.Max(originalAmplitude, targetAmplitude);
        noise.m_FrequencyGain = Mathf.Max(originalFrequency, targetFrequency);

        float time = 0f;
        float clampedDuration = Mathf.Max(0.01f, duration);

        while (time < clampedDuration)
        {
            float t = time / clampedDuration;
            noise.m_AmplitudeGain = Mathf.Lerp(targetAmplitude, originalAmplitude, t);
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        RestoreActiveNoise();
        shakeRoutine = null;
    }
}


