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

    [SerializeField] private float impulseForce = 1f;

    private CinemachineImpulseSource impulseSource;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        impulseSource = GetComponent<CinemachineImpulseSource>();
        if (impulseSource == null)
            impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
    }

    public void ScreenShake()
    {
        impulseSource.GenerateImpulse(impulseForce);
    }
}


