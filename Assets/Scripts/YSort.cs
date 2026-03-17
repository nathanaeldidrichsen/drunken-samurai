using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    private SpriteRenderer sr;

    [Header("Sorting")]
    public int sortingMultiplier = 100;

    [Tooltip("Positive = sort as if lower (for tall sprites like trees)")]
    public float yOffset = 0f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        float y = transform.position.y + yOffset;
        sr.sortingOrder = Mathf.RoundToInt(-y * sortingMultiplier);
    }

    void OnDrawGizmosSelected()
{
    Gizmos.color = Color.yellow;
    Gizmos.DrawSphere(transform.position + Vector3.up * yOffset, 0.05f);
}
}