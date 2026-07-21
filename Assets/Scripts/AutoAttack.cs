using UnityEngine;

/// <summary>
/// 뱀서라이크를 뱀서라이크로 만드는 스크립트.
/// 플레이어는 조준도 발사도 하지 않는다. 이동만 하면 무기가 알아서 나간다.
/// </summary>
[RequireComponent(typeof(PlayerStats))]
public class AutoAttack : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;

    [Tooltip("총알이 플레이어 몸 안에서 생기지 않게 밀어내는 거리")]
    [SerializeField] private float fireOffset = 0.5f;

    [Tooltip("총알이 2발 이상일 때 부채꼴로 벌어지는 각도")]
    [SerializeField] private float spreadAngle = 15f;

    private PlayerStats stats;
    private float lastFireTime;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        // 쿨타임 패턴. PlayerHealth 의 무적 시간도 똑같은 형태다.
        if (Time.time < lastFireTime + stats.fireRate) return;

        Transform target = FindNearestMonster();
        if (target == null) return;

        Shoot(target);
        GameManager.Instance?.Sfx?.PlayShoot();
        lastFireTime = Time.time;
    }

    // ─────────── 학생 빈칸 3 (2일차) ───────────
    // 살아있는 몬스터를 전부 가져와서, 가장 가까운 놈을 찾아 돌려준다.
    // 배열을 돌면서 최솟값 찾기 — 파이썬 수업에서 해본 그 문제다.
    //
    // FindGameObjectsWithTag 는 씬 전체를 훑는 느린 방식이다.
    // 실무라면 스포너가 목록을 들고 있게 한다. 여기서는 배열 순회를
    // 가르치는 게 더 중요해서 일부러 이렇게 뒀다. (발사할 때만 부르므로 체감 없음)
    Transform FindNearestMonster()
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");

        Transform nearest = null;
        float minDist = Mathf.Infinity;

        for (int i = 0; i < monsters.Length; i++)
        {
            float dist = Vector2.Distance(transform.position, monsters[i].transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                nearest = monsters[i].transform;
            }
        }

        return nearest;
    }
    // ──────────────────────────────────────────

    void Shoot(Transform target)
    {
        if (bulletPrefab == null) return;

        Vector2 baseDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;

        int count = Mathf.Max(1, stats.bulletCount);
        float startAngle = -spreadAngle * (count - 1) * 0.5f;

        for (int i = 0; i < count; i++)
        {
            Vector2 direction = Rotate(baseDirection, startAngle + spreadAngle * i);
            Vector3 spawnPos = transform.position + (Vector3)(direction * fireOffset);

            GameObject bullet = PoolManager.Instance.Get(bulletPrefab, spawnPos);
            if (bullet == null) continue;

            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null) bulletScript.Initialize(direction, stats.damage);
        }
    }

    static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);

        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}
