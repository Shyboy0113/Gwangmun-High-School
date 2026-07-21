using UnityEngine;

/// <summary>
/// 몬스터가 죽으면 떨어지는 경험치 젬.
/// 플레이어의 흡수 범위(magnetRadius) 안에 들어오면 빨려온다.
/// </summary>
public class ExpGem : MonoBehaviour
{
    public float moveSpeed = 8f;   // 빨려오는 속도

    private int exp = 1;
    private Transform player;
    private PlayerStats stats;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            stats = p.GetComponent<PlayerStats>();
        }
    }

    // Enemy가 죽으면서 경험치 양을 넣어 준다
    public void SetExp(int amount)
    {
        exp = amount;
    }

    void Update()
    {
        if (player == null || stats == null) return;

        // 흡수 범위 안이면 플레이어 쪽으로 이동
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance <= stats.magnetRadius)
        {
            transform.position = Vector2.MoveTowards(
                transform.position, player.position, moveSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerLevel level = other.GetComponent<PlayerLevel>();
        if (level != null) level.AddExp(exp);

        Destroy(gameObject);
    }
}
