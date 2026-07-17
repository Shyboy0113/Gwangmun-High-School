using UnityEngine;

/// <summary>
/// 탑다운 8방향 이동. 중력도 점프도 없다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerStats stats;

    /// 마지막으로 움직인 방향. 가만히 서 있어도 값이 유지된다.
    public Vector2 FacingDirection { get; private set; } = Vector2.right;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();

        // 탑다운이므로 중력과 회전을 끈다.
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Move();
    }

    // ─────────── 학생 빈칸 1 (1일차) ───────────
    // 방향키 입력을 받아 8방향으로 움직인다.
    // normalized 를 빼면 대각선이 1.41배 빨라진다. 직접 지워보고 확인시킬 것.
    void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector2 direction = new Vector2(x, y).normalized;
        rb.linearVelocity = direction * stats.moveSpeed;

        if (direction != Vector2.zero)
            FacingDirection = direction;
    }
    // ──────────────────────────────────────────
}
