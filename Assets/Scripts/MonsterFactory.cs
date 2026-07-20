using UnityEngine;

/// <summary>
/// 몬스터 생성 전담. behaviorType 을 보고 어떤 프리팹을 만들지 결정한다.
///
/// GoF 의 "팩토리 메서드"가 아니라 "심플 팩토리"다.
/// 팩토리 메서드는 Creator 추상 클래스 + ConcreteCreator 서브클래스 구조를 요구하는데,
/// 몬스터 3종에 Creator 클래스를 3개 더 만들어봐야 얻는 게 없다.
///
/// 세 층의 책임이 다르다:
///   MonsterSpawner  — 언제, 어디에
///   MonsterFactory  — 무엇을        ← 여기
///   PoolManager     — 어떻게 조달할지
/// </summary>
public class MonsterFactory : MonoBehaviour
{
    [Header("행동별 프리팹")]
    [SerializeField] private GameObject chasePrefab;
    [SerializeField] private GameObject shooterPrefab;
    [SerializeField] private GameObject exploderPrefab;

    [Header("예열(미리 생성) 개수")]
    [Tooltip("시작할 때 이 수만큼 미리 만들어 비활성 풀에 채운다. " +
             "이 수를 넘어서면 게임 도중 자동으로 더 생성된다(0이면 예열 안 함).")]
    [SerializeField] private int chasePrewarm = 20;
    [SerializeField] private int shooterPrewarm = 10;
    [SerializeField] private int exploderPrewarm = 10;

    // PoolManager 는 Awake 에서 Instance 를 세팅하므로, Start 에서 호출하면 항상 준비돼 있다.
    void Start()
    {
        if (PoolManager.Instance == null) return;

        PoolManager.Instance.Prewarm(chasePrefab, chasePrewarm);
        PoolManager.Instance.Prewarm(shooterPrefab, shooterPrewarm);
        PoolManager.Instance.Prewarm(exploderPrefab, exploderPrewarm);
    }

    public GameObject Create(MonsterData data, Vector3 position)
    {
        if (data == null) return null;

        GameObject prefab = SelectPrefab(data.behaviorType);
        if (prefab == null)
        {
            Debug.LogError($"[MonsterFactory] {data.behaviorType} 프리팹이 인스펙터에 연결되지 않았습니다.");
            return null;
        }

        GameObject monster = PoolManager.Instance.Get(prefab, position);
        if (monster == null) return null;

        MonsterController controller = monster.GetComponent<MonsterController>();
        if (controller != null) controller.Init(data);

        return monster;
    }

    /// 이 switch 가 팩토리의 전부다. 행동이 하나뿐이었다면
    /// 이 클래스는 Instantiate 한 줄을 감싼 껍데기였을 것이다.
    GameObject SelectPrefab(MonsterBehaviorType type)
    {
        switch (type)
        {
            case MonsterBehaviorType.Shooter:  return shooterPrefab;
            case MonsterBehaviorType.Exploder: return exploderPrefab;
            default:                           return chasePrefab;
        }
    }
}
