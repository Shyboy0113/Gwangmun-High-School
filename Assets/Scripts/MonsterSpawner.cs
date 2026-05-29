using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private float spawnInterval = 1.5f;  // 몇 초마다 생성
    [SerializeField] private float spawnRangeY   = 4f;   // 위아래 생성 범위
    [SerializeField] private float spawnX        = 9f;   // 화면 좌우 끝 X 좌표

    [Header("난이도 상승")]
    [SerializeField] private float intervalDecreaseRate = 0.03f;
    [SerializeField] private float minInterval          = 0.3f;

    private float timer;

    void Update()
    {
        if (!GameManager.Instance.IsPlaying) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnMonster();

            // 시간이 지날수록 스폰 간격 감소
            spawnInterval = Mathf.Max(minInterval, spawnInterval - intervalDecreaseRate);
        }
    }

    void SpawnMonster()
    {
        float randomY = Random.Range(-spawnRangeY, spawnRangeY);

        // 왼쪽 또는 오른쪽 중 랜덤으로 선택
        float side = Random.value > 0.5f ? spawnX : -spawnX;

        Vector3 spawnPos = new Vector3(side, randomY, 0f);
        Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
    }
}