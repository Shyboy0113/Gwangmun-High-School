using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 언제, 어디에 몬스터를 낼지만 결정한다.
/// 무엇을 만들지는 MonsterFactory 가, 어떻게 조달할지는 PoolManager 가 안다.
/// </summary>
public class MonsterSpawner : MonoBehaviour
{
    [Header("몬스터 목록")]
    [Tooltip("학생이 만든 MonsterData 를 여기 넣으면 등장한다. " +
             "appearTime 이 0인 몬스터가 최소 하나는 있어야 한다")]
    [SerializeField] private List<MonsterData> monsterTable = new List<MonsterData>();

    [Header("연결")]
    [SerializeField] private MonsterFactory factory;

    [Header("스폰 설정")]
    [SerializeField] private float spawnInterval = 1.5f;

    [Tooltip("플레이어를 중심으로 이 반경의 원 둘레에 스폰한다. 화면 밖이어야 한다")]
    [SerializeField] private float spawnRadius = 12f;

    [Header("난이도 상승")]
    [SerializeField] private float intervalDecreaseRate = 0.03f;
    [SerializeField] private float minInterval = 0.3f;

    [Header("상한")]
    [Tooltip("동시에 살아있는 몬스터 수 상한. 학교 노트북 보호용")]
    [SerializeField] private int maxAliveMonsters = 200;

    private float timer;

    // 직렬화 필드를 직접 깎으면 에디터에서 플레이할 때마다 값이 줄어든 채로 남는다.
    // 원본은 그대로 두고 런타임 값을 따로 둔다.
    private float currentInterval;

    private readonly List<MonsterData> candidates = new List<MonsterData>();
    private static Transform cachedPlayer;

    void Start()
    {
        currentInterval = spawnInterval;
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        timer += Time.deltaTime;
        if (timer < currentInterval) return;

        timer = 0f;
        SpawnMonster();

        currentInterval = Mathf.Max(minInterval, currentInterval - intervalDecreaseRate);
    }

    void SpawnMonster()
    {
        if (factory == null) return;
        if (GameObject.FindGameObjectsWithTag("Monster").Length >= maxAliveMonsters) return;

        MonsterData data = PickMonster();
        if (data == null) return;

        factory.Create(data, RandomPositionAroundPlayer());
    }

    /// 지금 등장할 수 있는 몬스터 중 하나를 무작위로 고른다.
    MonsterData PickMonster()
    {
        candidates.Clear();

        float elapsed = GameManager.Instance.SurvivedTime;

        for (int i = 0; i < monsterTable.Count; i++)
        {
            if (monsterTable[i] == null) continue;
            if (elapsed < monsterTable[i].appearTime) continue;

            candidates.Add(monsterTable[i]);
        }

        if (candidates.Count == 0) return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    /// 플레이어 중심 원 둘레의 무작위 지점. 사방에서 몰려오게 만드는 핵심이다.
    Vector3 RandomPositionAroundPlayer()
    {
        Vector3 center = GetPlayerPosition();

        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * spawnRadius;

        return center + offset;
    }

    Vector3 GetPlayerPosition()
    {
        if (cachedPlayer == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            cachedPlayer = go != null ? go.transform : null;
        }
        return cachedPlayer != null ? cachedPlayer.position : Vector3.zero;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(GetPlayerPosition(), spawnRadius);
    }
}
