using UnityEngine;

public class ToggleSelectedButton : MonoBehaviour
{
    [SerializeField] private GameObject selectedGraphics;
    [SerializeField] private float pulseScale = 1.12f;
    [SerializeField] private float pulseDuration = 0.1f;

    private Coroutine pulseCoroutine;
    private Vector3 selectedGraphicsBaseScale = Vector3.one;

    private void Awake()
    {
        if (selectedGraphics != null)
            selectedGraphicsBaseScale = selectedGraphics.transform.localScale;

        ToggleSelected(false);
    }



    public void ToggleSelected(bool isSelected)
    {
        if (selectedGraphics == null)
            return;

        selectedGraphics.SetActive(isSelected);

        if (isSelected)
        {
            PulseSelectedGraphics();
        }
        else
            selectedGraphics.transform.localScale = selectedGraphicsBaseScale;
    }

    private void PulseSelectedGraphics()
    {
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);

        selectedGraphics.transform.localScale = selectedGraphicsBaseScale;
        pulseCoroutine = StartCoroutine(PulseSelectedGraphicsRoutine());
    }

    private System.Collections.IEnumerator PulseSelectedGraphicsRoutine()
    {
        Transform selectedTransform = selectedGraphics.transform;
        Vector3 targetScale = selectedGraphicsBaseScale * pulseScale;

        float elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            selectedTransform.localScale = Vector3.Lerp(selectedGraphicsBaseScale, targetScale, Mathf.Clamp01(elapsed / pulseDuration));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            selectedTransform.localScale = Vector3.Lerp(targetScale, selectedGraphicsBaseScale, Mathf.Clamp01(elapsed / pulseDuration));
            yield return null;
        }

        selectedTransform.localScale = selectedGraphicsBaseScale;
        pulseCoroutine = null;
    }
}
