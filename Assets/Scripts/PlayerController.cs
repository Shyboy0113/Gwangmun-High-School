using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 수평(좌우), 수직(상하) 입력 받기 — 방향키 or WASD
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical   = Input.GetAxisRaw("Vertical");

        // 대각선 이동 시 속도가 빨라지지 않도록 normalized
        Vector2 direction = new Vector2(horizontal, vertical).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            GameManager.Instance.GameOver();
        }
    }
}