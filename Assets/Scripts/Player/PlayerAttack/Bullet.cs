using UnityEngine;

/// <summary>
/// 플레이어가 쏘는 총알. 몬스터에 닿으면 데미지를 주고 사라진다.
/// </summary>
public class Bullet : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float lifeTime = 3f;   // 이 시간(초) 뒤 저절로 사라진다

    private int damage;

    public void Initialize(Vector2 direction, int damageAmount)
    {
        damage = damageAmount;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = direction.normalized * moveSpeed;

        // 시간이 지나면 알아서 없어지게
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Monster")) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null) enemy.TakeDamage(damage);

        Destroy(gameObject);
    }
}
