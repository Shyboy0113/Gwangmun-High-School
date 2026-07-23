using UnityEngine;

/// <summary>
/// 탑다운 8방향 이동. 중력도 점프도 없다.
/// </summary>
public class PlayerController : MonoBehaviour
{
    // 유니티에서 자체 제공하는 컴포넌트
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    // 우리가 새로 만든 컴포넌트
    private PlayerStats stats;
    private Animator animator;

    /// 마지막으로 움직인 방향. 가만히 서 있어도 값이 유지된다.
    public Vector2 FacingDirection = Vector2.right;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        stats = GetComponent<PlayerStats>();
        animator = GetComponent<Animator>();

        // 탑다운이므로 중력과 회전을 끈다.
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        // 게임이 플레이 중이 아니면 멈춘다.
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Move();
    }

    // 방향키 입력을 받아 8방향으로 움직인다.
    void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // normalized를 빼면 대각선이 1.41배 빨라진다.
        Vector2 direction = new Vector2(x, y).normalized;
        rb.linearVelocity = direction * stats.moveSpeed;

        // 움직이는 중이면 바라보는 방향을 기억하고 그림을 뒤집는다.
        if (direction != Vector2.zero)
        {
            FacingDirection = direction;
            FlipSprite();
        }

        // 걷기 애니메이션 켜고 끄기 (애니메이터가 있을 때만)
        if (animator != null)
            animator.SetBool("isMoving", direction != Vector2.zero);
    }

    // 왼쪽으로 가면 캐릭터 그림을 좌우로 뒤집는다.
    void FlipSprite()
    {
        if (sr != null)
            sr.flipX = FacingDirection.x < 0;
    }
}
