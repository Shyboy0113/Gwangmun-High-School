using UnityEngine;

/// <summary>
/// 사거리 밖에서는 다가오고, 사거리 안에 들어오면 멈춰서 총알을 쏜다.
/// 사거리는 MonsterData.attackRange 로 학생이 정한다.
/// </summary>
public class ShooterBehaviour : MonsterBehaviour
{
    [SerializeField] private GameObject monsterBulletPrefab;
    [SerializeField] private float fireInterval = 2f;

    private float lastFireTime;

    public override void Init(MonsterData monsterData)
    {
        base.Init(monsterData);

        // 풀에서 재사용될 때 이전 판의 발사 시각이 남아 있으면
        // 스폰하자마자 쏘거나 한참 안 쏜다. 반드시 초기화한다.
        lastFireTime = Time.time;
    }

    protected override void Tick()
    {
        Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
        float distance = toPlayer.magnitude;
        Vector2 direction = toPlayer.normalized;

        if (distance > data.attackRange)
        {
            rb.linearVelocity = direction * data.moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            TryFire(direction);
        }
    }

    void TryFire(Vector2 direction)
    {
        if (Time.time < lastFireTime + fireInterval) return;
        if (monsterBulletPrefab == null) return;

        lastFireTime = Time.time;

        GameObject bullet = PoolManager.Instance.Get(monsterBulletPrefab, transform.position);
        if (bullet == null) return;

        MonsterBullet monsterBullet = bullet.GetComponent<MonsterBullet>();
        if (monsterBullet != null) monsterBullet.Initialize(direction, data.contactDamage);
    }
}
