using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 6f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 3f;

    private Vector2 direction = Vector2.right;
    private Rigidbody2D rb2d;
    private bool initialized;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (lifetime > 0f)
            Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector2 newDirection, float newSpeed, int newDamage, float newLifetime)
    {
        direction = newDirection.normalized;
        if (direction == Vector2.zero)
            direction = Vector2.right;

        speed = newSpeed;
        damage = newDamage;
        lifetime = newLifetime;
        initialized = true;

        if (rb2d != null)
            rb2d.velocity = direction * speed;

        if (lifetime > 0f)
            Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (rb2d == null && initialized)
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player.Instance?.GetHurt(damage);
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger && !other.CompareTag("Enemy"))
            Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Player.Instance?.GetHurt(damage);
            Destroy(gameObject);
            return;
        }

        if (!collision.collider.CompareTag("Enemy"))
            Destroy(gameObject);
    }
}
