using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    [Tooltip("The GameEvent to listen to.")]
    public GameEvent gameEvent;

    [Tooltip("Actions to perform when the event is raised.")]
    public UnityEvent response;

    private void OnEnable() => gameEvent?.RegisterListener(this);
    private void OnDisable() => gameEvent?.UnregisterListener(this);

    public void OnEventRaised() => response?.Invoke();
}
