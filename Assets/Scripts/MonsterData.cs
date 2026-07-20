using UnityEngine;

/// 몬스터의 행동 방식. 팩토리가 이 값을 보고 어떤 프리팹을 만들지 결정한다.
public enum MonsterBehaviorType
{
    Chase,      // 플레이어를 향해 직선 추적
    Shooter,    // 사거리까지 접근한 뒤 멈춰서 발사
    Exploder,   // 추적하다 죽을 때 범위 폭발
    Boss,       // 느리게 추적하며 주기적으로 탄막을 쏘는 최종 보스
}

/// <summary>
/// 몬스터 한 종류의 설계도.
///
/// 이 에셋 하나를 복제해서 숫자와 그림만 바꾸면 새 몬스터가 된다.
/// 프리팹은 건드릴 필요가 없다.
/// </summary>
[CreateAssetMenu(fileName = "NewMonster", menuName = "뱀서라이크/몬스터 데이터")]
public class MonsterData : ScriptableObject
{
    [Header("겉모습")]
    public string monsterName = "새 몬스터";
    public Sprite sprite;

    [Tooltip("스프라이트를 그릴 시간이 없으면 색만 바꿔도 다른 몬스터가 된다")]
    public Color color = Color.white;

    public float size = 1f;

    [Header("행동")]
    public MonsterBehaviorType behaviorType = MonsterBehaviorType.Chase;

    [Tooltip("사격형 전용 — 이 거리까지 접근한 뒤 멈춰서 쏜다")]
    public float attackRange = 5f;

    [Tooltip("폭발형 전용 — 죽을 때 터지는 반경")]
    public float explosionRadius = 2f;

    [Tooltip("폭발형 전용 — 터질 때 반경 내 다른 몬스터에게 주는 데미지(N)")]
    public int explosionDamage = 5;

    [Header("능력치")]
    public int maxHp = 3;
    public float moveSpeed = 2f;
    public int contactDamage = 1;

    [Header("보상")]
    public int expAmount = 1;

    [Header("등장 조건")]
    [Tooltip("게임 시작 후 이 시간(초)이 지나야 등장한다. " +
             "0인 몬스터가 최소 하나는 있어야 초반에 적이 나온다")]
    public float appearTime = 0f;

    [Header("보스")]
    [Tooltip("체크하면 이 몬스터를 처치했을 때 게임 승리(클리어)로 처리한다. " +
             "보스는 일반 스폰 목록이 아니라 MonsterSpawner 의 bossData 로 등장한다")]
    public bool isBoss = false;
}
