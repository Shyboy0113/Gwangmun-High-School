using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private float spawnInterval = 1f;   // 몇 초마다 생성할지
    [SerializeField] private float spawnRangeX  = 4f;   // 좌우 생성 범위
    [SerializeField] private float spawnY       = 6f;   // 화면 위쪽 Y 좌표

    [Header("난이도 상승")]
    [SerializeField] private float intervalDecreaseRate = 0.05f; // 시간이 지날수록 빨라지는 정도
    [SerializeField] private float minInterval          = 0.2f;  // 최소 간격 (너무 빠르지 않게)

    private float timer;

    void Update()
    {
        if (!GameManager.Instance.IsPlaying) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnBall();

            // 시간이 지날수록 스폰 간격 감소 → 난이도 상승
            spawnInterval = Mathf.Max(minInterval, spawnInterval - intervalDecreaseRate);
        }
    }

    void SpawnBall()
    {
        float randomX  = Random.Range(-spawnRangeX, spawnRangeX);
        Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);
        Instantiate(ballPrefab, spawnPos, Quaternion.identity);
    }
}
