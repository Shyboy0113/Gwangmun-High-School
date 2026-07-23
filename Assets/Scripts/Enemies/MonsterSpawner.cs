using System.Collections.Generic;
using UnityEngine;

// 몬스터 한 종류를 언제부터 낼지 묶어 둔 것.
[System.Serializable]
public class SpawnEntry
{
    public GameObject enemyPrefab;   // Enemy 스크립트가 붙은 몬스터 프리팹
    public float appearTime = 0f;    // 게임 시작 후 이 시간(초)이 지나야 등장 (0이면 처음부터)
}

/// <summary>
/// 몬스터를 계속 만들어 낸다. 시간이 갈수록 점점 빨리 나온다.
/// bossPrefab 을 넣어 두면 bossSpawnTime 초에 보스가 나온다.
/// </summary>
public class MonsterSpawner : MonoBehaviour
{
    [Header("몬스터 목록")]
    public List<SpawnEntry> enemies = new List<SpawnEntry>();

    [Header("스폰 설정")]
    public float spawnInterval = 1.5f;   // 몬스터가 나오는 간격(초)
    public float spawnRadius = 12f;      // 플레이어 주변 이 거리(원)에 나온다 (화면 밖)
    public int maxAlive = 200;           // 동시에 살아있는 몬스터 최대 수 (노트북 보호)

    [Header("난이도")]
    public float intervalDecrease = 0.03f; // 한 번 낼 때마다 간격을 이만큼 줄인다
    public float minInterval = 0.3f;       // 간격이 이보다 짧아지진 않는다

    [Header("보스 (마지막)")]
    public GameObject bossPrefab;      // 비워 두면 보스전 없음
    public float bossSpawnTime = 180f; // 이 시간(초)이 되면 보스가 나온다

    private float timer;
    private float currentInterval;
    private bool bossSpawned;
    private Transform player;

    void Start()
    {
        currentInterval = spawnInterval;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        // 게임이 안 돌면 아무것도 안 한다
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

        // 보스 등장: 시간이 되면 보스 한 번만 내고, 일반 몬스터는 멈춘다
        if (!bossSpawned && bossPrefab != null && GameManager.Instance.SurvivedTime >= bossSpawnTime)
        {
            Instantiate(bossPrefab, RandomPositionAroundPlayer(), Quaternion.identity);
            bossSpawned = true;
            return;
        }
        if (bossSpawned) return;

        // 시간마다 몬스터 하나 소환
        timer += Time.deltaTime;
        if (timer < currentInterval) return;
        timer = 0f;

        SpawnOne();

        // 다음부터는 조금 더 빨리 나오게
        currentInterval = Mathf.Max(minInterval, currentInterval - intervalDecrease);
    }

    void SpawnOne()
    {
        // 너무 많으면 그만
        if (GameObject.FindGameObjectsWithTag("Monster").Length >= maxAlive) return;

        GameObject prefab = PickEnemy();
        if (prefab == null) return;

        Instantiate(prefab, RandomPositionAroundPlayer(), Quaternion.identity);
    }

    // 지금 나올 수 있는 몬스터 중 하나를 무작위로 고른다
    GameObject PickEnemy()
    {
        float now = GameManager.Instance.SurvivedTime;

        List<GameObject> ready = new List<GameObject>();
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].enemyPrefab == null) continue;
            if (now < enemies[i].appearTime) continue;
            ready.Add(enemies[i].enemyPrefab);
        }

        if (ready.Count == 0) return null;
        return ready[Random.Range(0, ready.Count)];
    }

    // 플레이어 주변 원 둘레의 무작위 위치 (사방에서 몰려오게)
    Vector3 RandomPositionAroundPlayer()
    {
        Vector3 center = player != null ? player.position : Vector3.zero;

        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * spawnRadius;
        return center + offset;
    }
}
