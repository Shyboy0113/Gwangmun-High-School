using UnityEngine;

/// <summary>
/// 몬스터가 쏘는 총알. 플레이어에 닿으면 데미지를 주고 사라진다.
/// </summary>
public class MonsterBullet : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lifeTime = 4f;   // 이 시간(초) 뒤 저절로 사라진다

    private int damage;

    public void Initialize(Vector2 direction, int damageAmount)
    {
        damage = damageAmount;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = direction.normalized * moveSpeed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null) health.TakeDamage(damage);

        Destroy(gameObject);
    }
}
