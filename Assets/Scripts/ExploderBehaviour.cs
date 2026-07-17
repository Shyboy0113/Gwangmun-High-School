using UnityEngine;

/// <summary>
/// 추적하다가 죽는 순간 주변을 터뜨린다.
/// 총으로 쏴 죽여도 폭발하므로, 가까이서 잡으면 손해다.
/// </summary>
public class ExploderBehaviour : MonsterBehaviour
{
    [SerializeField] private GameObject explosionEffectPrefab;

    protected override void Tick()
    {
        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * data.moveSpeed;
    }

    public override void OnMonsterDeath()
    {
        if (data == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, data.explosionRadius);

        for (int i = 0; i < hits.Length; i++)
        {
            if (!hits[i].CompareTag("Player")) continue;

            PlayerHealth health = hits[i].GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(data.contactDamage);
        }

        if (explosionEffectPrefab != null)
            PoolManager.Instance.Get(explosionEffectPrefab, transform.position);
    }

    // 씬 뷰에서 폭발 반경을 눈으로 확인할 수 있게 한다.
    void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.explosionRadius);
    }
}
