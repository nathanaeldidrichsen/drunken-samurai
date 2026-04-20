using UnityEngine;
using UnityEngine.EventSystems;

public class SingletonEventSystem : MonoBehaviour
{
    void Awake()
    {
        var all = FindObjectsOfType<EventSystem>();
        if (all.Length > 1)
        {
            Destroy(gameObject);
        }
    }
}
