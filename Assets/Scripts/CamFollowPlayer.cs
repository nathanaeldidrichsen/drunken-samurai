using UnityEngine;
using Cinemachine;

public class CamFollowPlayer : MonoBehaviour
{
    private CinemachineVirtualCamera vcam;

    void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        if (vcam == null)
        {
            Debug.LogError("CamFollowPlayer: Missing CinemachineVirtualCamera on this GameObject.");
            return;
        }

        if (Player.Instance == null)
        {
            Debug.LogError("CamFollowPlayer: Player instance not found.");
            return;
        }

        vcam.Follow = Player.Instance.transform;
    }
}
