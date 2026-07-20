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
            // 플레이어에게는 접촉 데미지를 준다(기존 동작 유지).
            if (hits[i].CompareTag("Player"))
            {
                PlayerHealth health = hits[i].GetComponent<PlayerHealth>();
                if (health != null) health.TakeDamage(data.contactDamage);
                continue;
            }

            // 반경 안의 다른 몬스터에게 폭발 데미지(N)를 준다. 자기 자신은 건너뛴다.
            // (자신은 이미 isDead 라 TakeDamage 가 무시하지만, 명시적으로 제외해 의도를 드러낸다.)
            MonsterController other = hits[i].GetComponent<MonsterController>();
            if (other != null && other.gameObject != gameObject)
                other.TakeDamage(data.explosionDamage);
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
