using UnityEngine;

/// <summary>
/// 사격형 몬스터가 쏘는 총알. Bullet 과 대칭이지만 플레이어를 때린다.
/// 데미지가 int 인 이유는 MonsterData.contactDamage 가 int 이기 때문이다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MonsterBullet : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float lifeTime = 4f;

    private Rigidbody2D rb;
    private int damage;
    private float spawnTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 direction, int damageAmount)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        damage = damageAmount;
        spawnTime = Time.time;

        rb.gravityScale = 0f;
        rb.linearVelocity = direction.normalized * moveSpeed;
    }

    void Update()
    {
        if (Time.time >= spawnTime + lifeTime)
            PoolManager.Instance.Release(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null) health.TakeDamage(damage);

        PoolManager.Instance.Release(gameObject);
    }
}
