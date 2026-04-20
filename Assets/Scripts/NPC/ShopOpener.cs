using UnityEngine;

public class ShopOpener : MonoBehaviour
{
    public void OpenShop()
    {
        ShopManager.Instance?.OpenShop();
    }
}
