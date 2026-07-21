using UnityEngine;

/// <summary>
/// 모든 몬스터의 공통 부분 — 체력, 피격, 사망, 젬 드롭, 접촉 데미지.
///
/// 어떻게 움직이고 공격하는지는 모른다. 그건 MonsterBehaviour 의 몫이다.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class MonsterController : MonoBehaviour
{
    [SerializeField] private GameObject expGemPrefab;

    private MonsterData data;
    private MonsterBehaviour behaviour;
    private SpriteRenderer spriteRenderer;

    private int currentHp;
    private bool isDead;

    void Awake()
    {
        CacheComponents();
    }

    void CacheComponents()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (behaviour == null) behaviour = GetComponent<MonsterBehaviour>();
    }

    /// <summary>
    /// 팩토리가 스폰 직후 호출한다. 데이터를 몸에 바르고 행동 컴포넌트에 넘긴다.
    /// 풀에서 재사용될 때도 매번 호출되므로 모든 상태를 여기서 덮어써야 한다.
    /// </summary>
    public void Init(MonsterData monsterData)
    {
        CacheComponents();

        data = monsterData;
        currentHp = data.maxHp;
        isDead = false;

        spriteRenderer.sprite = data.sprite;
        spriteRenderer.color = data.color;
        transform.localScale = Vector3.one * data.size;

        if (behaviour != null) behaviour.Init(data);
    }

    /// 총알이 호출한다. 데미지가 float 인 이유는 PlayerStats.damage 가 float 이기 때문이다.
    public void TakeDamage(float amount)
    {
        if (isDead || data == null) return;

        currentHp -= Mathf.RoundToInt(amount);

        if (currentHp <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // 폭발형은 여기서 터진다. 젬 드롭보다 먼저 불러야
        // 폭발로 죽은 몬스터의 젬이 정상적으로 나온다.
        if (behaviour != null) behaviour.OnMonsterDeath();

        GameManager.Instance?.Sfx?.PlayMonsterDeath();

        DropGem();

        // 보스를 처치하면 게임 클리어(최종전 승리).
        if (data.isBoss && GameManager.Instance != null)
            GameManager.Instance.Victory();

        PoolManager.Instance.Release(gameObject);
    }

    void DropGem()
    {
        if (expGemPrefab == null || data == null) return;

        GameObject gem = PoolManager.Instance.Get(expGemPrefab, transform.position);
        if (gem == null) return;

        ExpGem expGem = gem.GetComponent<ExpGem>();
        if (expGem != null) expGem.Init(data.expAmount);
    }

    // 몬스터 콜라이더는 트리거다. 비트리거로 두면 Kinematic 몬스터가
    // Dynamic 플레이어를 물리적으로 밀어내 조작이 불가능해진다.
    void OnTriggerEnter2D(Collider2D other) { TryDamagePlayer(other); }
    void OnTriggerStay2D(Collider2D other)  { TryDamagePlayer(other); }

    void TryDamagePlayer(Collider2D other)
    {
        if (isDead || data == null) return;
        if (!other.CompareTag("Player")) return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();

        // 붙어 있는 동안 계속 때린다. 연사는 PlayerHealth 의 무적 시간이 막는다.
        if (health != null) health.TakeDamage(data.contactDamage);
    }
}
