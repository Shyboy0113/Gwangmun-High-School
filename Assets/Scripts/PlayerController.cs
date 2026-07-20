using UnityEngine;

/// <summary>
/// 탑다운 8방향 이동. 중력도 점프도 없다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    // 유니티에서 자체 제공하는 컴포넌트
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    
    // 우리가 새로 생성한 컴포넌트
    private PlayerStats _playerStats;
    private Animator _animator;

    /// 마지막으로 움직인 방향. 가만히 서 있어도 값이 유지된다.
    public Vector2 FacingDirection { get; private set; } = Vector2.right;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        
        _playerStats = GetComponent<PlayerStats>();
        _animator = GetComponent<Animator>();

        // 탑다운이므로 중력과 회전을 끕니다.
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        if (GameManager.Instance is null || !GameManager.Instance.IsPlaying)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Move();
    }
    
    // 방향키 입력을 받아 8방향으로 움직입니다.
    void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // normalized를 빼면 대각선이 1.41배 빨라집니다.
        Vector2 direction = new Vector2(x, y).normalized;
        _rb.linearVelocity = direction * _playerStats.moveSpeed;

        if (direction != Vector2.zero)
        {
            FacingDirection = direction;
            
            // 캐릭터의 스프라이트를 xFlip 시켜줍니다.
            FlipSprite();

            if (_animator != null)
            {
                _animator.SetBool("isMoving", true);
            }
        }
        else
        {
            _animator.SetBool("isMoving", false);
        }

    }

    private void FlipSprite()
    {
        if (_sr != null)
            _sr.flipX = FacingDirection.x < 0;
    }
    
}
