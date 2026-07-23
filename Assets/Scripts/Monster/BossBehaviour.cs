using UnityEngine;

/// <summary>
/// 최종 보스. 플레이어를 느리게 추적하면서 일정 간격으로 부채꼴 탄막을 쏜다.
///
/// 큰 접촉 데미지는 MonsterController 의 트리거 판정이, 체력·사망은 MonsterController 가,
/// 승리 처리는 MonsterController.Die() 가 data.isBoss 를 보고 GameManager.Victory() 를 호출한다.
/// 여기는 "어떻게 싸우는가"(추적 + 탄막)만 담당한다.
/// </summary>
public class BossBehaviour : MonsterBehaviour
{
    [SerializeField] private GameObject monsterBulletPrefab;

    // 발사 간격·탄 수·부채꼴 각도는 MonsterData(SO)에서 온다.
    // 학생이 프리팹을 열지 않고 SO 값만 바꿔 보스 탄막을 조절할 수 있게 하기 위함이다.
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
        // 플레이어를 느리게 추적한다(속도는 data.moveSpeed).
        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * data.moveSpeed;

        TryFire(direction);
    }

    void TryFire(Vector2 aimDirection)
    {
        if (Time.time < lastFireTime + data.fireInterval) return;
        if (monsterBulletPrefab == null) return;

        lastFireTime = Time.time;

        // 조준 방향을 중심으로 부채꼴로 여러 발을 뿌린다. (AutoAttack 의 분열탄과 같은 방식)
        int count = Mathf.Max(1, data.bulletCount);
        float startAngle = -data.spreadAngle * (count - 1) * 0.5f;

        for (int i = 0; i < count; i++)
        {
            Vector2 dir = Rotate(aimDirection, startAngle + data.spreadAngle * i);

            GameObject bullet = PoolManager.Instance.Get(monsterBulletPrefab, transform.position);
            if (bullet == null) continue;

            MonsterBullet mb = bullet.GetComponent<MonsterBullet>();
            if (mb != null) mb.Initialize(dir, data.contactDamage);
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
