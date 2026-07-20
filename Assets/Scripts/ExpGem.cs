using UnityEngine;

/// <summary>
/// 몬스터가 죽으면 떨어지는 경험치 젬.
/// 마그넷 반경 안에 들어오면 플레이어에게 빨려온다.
/// </summary>
public class ExpGem : MonoBehaviour
{
    [Tooltip("빨려올 때의 속도. 플레이어 이동 속도보다 빨라야 따라잡는다")]
    [SerializeField] private float moveSpeed = 8f;

    private int expAmount;

    private static Transform cachedPlayer;
    private static PlayerStats cachedStats;

    /// 몬스터가 죽으면서 호출한다. 풀에서 재사용되므로 매번 덮어쓴다.
    public void Init(int amount)
    {
        expAmount = amount;
    }

    // ─────────── 학생 빈칸 5 (3일차) ───────────
    // 플레이어가 마그넷 반경 안에 있으면 그쪽으로 다가간다.
    // 이게 없으면 젬을 하나하나 밟으러 다녀야 해서 게임이 노동이 된다.
    void Update()
    {
        if (!CachePlayer()) return;

        float distance = Vector2.Distance(transform.position, cachedPlayer.position);
        if (distance > cachedStats.magnetRadius) return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            cachedPlayer.position,
            moveSpeed * Time.deltaTime);
    }
    // ──────────────────────────────────────────

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // PlayerLevel.Instance 를 쓰지 않는다.
        // 싱글톤은 GameManager 와 PoolManager 둘로 제한하기로 했다.
        PlayerLevel level = other.GetComponent<PlayerLevel>();
        if (level != null) level.AddExp(expAmount);

        PoolManager.Instance.Release(gameObject);
    }

    bool CachePlayer()
    {
        if (cachedPlayer == null || cachedStats == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go == null) return false;

            cachedPlayer = go.transform;
            cachedStats = go.GetComponent<PlayerStats>();
        }

        return cachedPlayer != null && cachedStats != null;
    }
}
