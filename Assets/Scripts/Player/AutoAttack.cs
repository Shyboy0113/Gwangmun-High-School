using UnityEngine;

/// <summary>
/// 가장 가까운 몬스터를 자동으로 찾아 총알을 쏜다.
/// </summary>
public class AutoAttack : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float fireOffset = 0.5f;    // 총알이 몸 안에서 생기지 않게 밀어내는 거리
    public float spreadAngle = 15f;    // 총알이 2발 이상일 때 부채꼴 각도

    private PlayerStats stats;
    private float lastFireTime;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        // 쿨타임
        if (Time.time < lastFireTime + stats.fireRate) return;

        Transform target = FindNearestMonster();
        if (target == null) return;

        Shoot(target);
        lastFireTime = Time.time;
    }

    // 가장 가까운 몬스터 찾기
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

    void Shoot(Transform target)
    {
        if (bulletPrefab == null) return;

        Vector2 baseDir = ((Vector2)target.position - (Vector2)transform.position).normalized;

        int count = Mathf.Max(1, stats.bulletCount);
        float startAngle = -spreadAngle * (count - 1) * 0.5f;

        for (int i = 0; i < count; i++)
        {
            Vector2 direction = Rotate(baseDir, startAngle + spreadAngle * i);
            Vector3 spawnPos = transform.position + (Vector3)(direction * fireOffset);

            GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null) bulletScript.Initialize(direction, stats.damage);
        }
    }

    // 방향 벡터를 각도(도)만큼 돌린다 (부채꼴 발사에 쓴다)
    static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}
