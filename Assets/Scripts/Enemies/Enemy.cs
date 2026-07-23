using UnityEngine;

// 몬스터 종류. 인스펙터에서 골라서 바꾼다.
public enum EnemyType
{
    Chase,     // 그냥 쫓아옴
    Shooter,   // 가까이 오면 멈춰서 총 쏨
    Exploder,  // 죽을 때 터짐
    Boss,      // 느리게 쫓아오며 부채꼴로 여러 발, 잡으면 게임 승리
}

/// <summary>
/// 몬스터 하나를 담당하는 스크립트. 이 스크립트 하나면 몬스터 끝.
/// 종류(type)만 바꾸면 움직임과 공격이 달라진다.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("종류")]
    public EnemyType type = EnemyType.Chase;

    [Header("공통 능력치")]
    public int maxHp = 3;
    public float moveSpeed = 2f;
    public int touchDamage = 1;   // 플레이어에 닿으면 주는 데미지
    public int expReward = 1;     // 죽을 때 떨구는 경험치
    public GameObject expGemPrefab;

    [Header("사격형(Shooter) / 보스(Boss)가 쏘는 총알")]
    public GameObject enemyBulletPrefab;

    [Header("사격형(Shooter) 전용")]
    public float attackRange = 5f;   // 이 거리 안에 오면 멈춰서 쏜다
    public float fireInterval = 2f;  // 총 쏘는 간격(초)

    [Header("폭발형(Exploder) 전용")]
    public float explosionRadius = 2f;   // 터지는 반경
    public int explosionDamage = 5;      // 반경 안 몬스터에게 주는 데미지
    public GameObject explosionPrefab;   // 폭발 이펙트

    [Header("보스(Boss) 전용")]
    public int bossBulletCount = 5;       // 한 번에 쏘는 총알 수
    public float bossSpreadAngle = 20f;   // 부채꼴로 벌어지는 각도
    public float bossFireInterval = 2.5f; // 탄막 쏘는 간격(초)

    private int hp;
    private float lastFireTime;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Transform player;

    void Start()
    {
        hp = maxHp;

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;          // 위에서 보는 게임이라 중력 없음
        sr = GetComponent<SpriteRenderer>();

        // 플레이어 찾기 ("Player" 태그가 붙어 있어야 한다)
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void FixedUpdate()
    {
        // 플레이어가 없거나 게임이 안 돌면 멈춘다
        if (player == null || GameManager.Instance == null || !GameManager.Instance.IsPlaying)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        // 플레이어 쪽 방향과 거리
        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);

        if (type == EnemyType.Shooter && distance <= attackRange)
        {
            // 사거리 안: 멈춰서 총알 1발씩
            rb.linearVelocity = Vector2.zero;
            Shoot(dir, 1, 0f, fireInterval);
        }
        else if (type == EnemyType.Boss)
        {
            // 보스: 느리게 쫓아오며 부채꼴 탄막
            rb.linearVelocity = dir * moveSpeed;
            Shoot(dir, bossBulletCount, bossSpreadAngle, bossFireInterval);
        }
        else
        {
            // Chase / Exploder / 사거리 밖 Shooter: 그냥 쫓아온다
            rb.linearVelocity = dir * moveSpeed;
        }

        // 왼쪽으로 가면 그림 좌우 뒤집기
        if (sr != null && Mathf.Abs(rb.linearVelocity.x) > 0.01f)
            sr.flipX = rb.linearVelocity.x < 0f;
    }

    // 총알 쏘기 (사격형은 1발, 보스는 부채꼴로 여러 발)
    void Shoot(Vector2 aimDir, int count, float spread, float interval)
    {
        if (enemyBulletPrefab == null) return;
        if (Time.time < lastFireTime + interval) return;   // 쿨타임
        lastFireTime = Time.time;

        float startAngle = -spread * (count - 1) * 0.5f;
        for (int i = 0; i < count; i++)
        {
            Vector2 d = Rotate(aimDir, startAngle + spread * i);

            GameObject b = Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
            MonsterBullet mb = b.GetComponent<MonsterBullet>();
            if (mb != null) mb.Initialize(d, touchDamage);
        }
    }

    // 플레이어 총알이 부르는 함수
    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0) Die();
    }

    void Die()
    {
        // 폭발형이면 터진다
        if (type == EnemyType.Exploder) Explode();

        // 경험치 젬 떨구기
        if (expGemPrefab != null)
        {
            GameObject gem = Instantiate(expGemPrefab, transform.position, Quaternion.identity);
            ExpGem e = gem.GetComponent<ExpGem>();
            if (e != null) e.SetExp(expReward);
        }

        // 보스를 잡으면 게임 승리
        if (type == EnemyType.Boss && GameManager.Instance != null)
            GameManager.Instance.Victory();

        Destroy(gameObject);
    }

    // 죽을 때 주변을 터뜨린다 (폭발형 전용)
    void Explode()
    {
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth h = hit.GetComponent<PlayerHealth>();
                if (h != null) h.TakeDamage(touchDamage);
            }
            else
            {
                // 반경 안의 다른 몬스터에게 폭발 데미지
                Enemy other = hit.GetComponent<Enemy>();
                if (other != null && other != this) other.TakeDamage(explosionDamage);
            }
        }
    }

    // 플레이어에 닿아 있는 동안 계속 데미지 (무적 시간은 PlayerHealth가 처리)
    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth h = other.GetComponent<PlayerHealth>();
        if (h != null) h.TakeDamage(touchDamage);
    }

    // 방향 벡터를 각도(도)만큼 돌린다. 부채꼴 발사에 쓴다.
    static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }

    // 씬 화면에서 폭발 반경을 빨간 원으로 보여준다 (폭발형 확인용)
    void OnDrawGizmosSelected()
    {
        if (type != EnemyType.Exploder) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
