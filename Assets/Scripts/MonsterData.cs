using UnityEngine;

/// 몬스터의 행동 방식. 팩토리가 이 값을 보고 어떤 프리팹을 만들지 결정한다.
public enum MonsterBehaviorType
{
    Chase,      // 플레이어를 향해 직선 추적
    Shooter,    // 사거리까지 접근한 뒤 멈춰서 발사
    Exploder,   // 추적하다 죽을 때 범위 폭발
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
}
