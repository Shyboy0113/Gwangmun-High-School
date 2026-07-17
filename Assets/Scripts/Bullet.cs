using UnityEngine;

/// <summary>
/// 플레이어의 총알. 몬스터에게 데미지를 전달하고 풀로 돌아간다.
/// 몬스터가 죽는지, 젬이 뭔지는 모른다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float lifeTime = 3f;

    private Rigidbody2D rb;
    private float damage;
    private float spawnTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 direction, float damageAmount)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        damage = damageAmount;
        spawnTime = Time.time;

        rb.gravityScale = 0f;
        rb.linearVelocity = direction.normalized * moveSpeed;
    }

    void Update()
    {
        // Destroy(gameObject, lifeTime) 를 쓰면 풀이 관리하던 객체가 사라져
        // 딕셔너리에 죽은 참조가 남는다. 반드시 풀에 돌려준다.
        if (Time.time >= spawnTime + lifeTime)
            PoolManager.Instance.Release(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Monster")) return;

        MonsterController monster = other.GetComponent<MonsterController>();
        if (monster != null) monster.TakeDamage(damage);

        PoolManager.Instance.Release(gameObject);
    }
}
