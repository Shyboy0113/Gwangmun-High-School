using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 4f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 생성되자마자 아래로 이동 시작
        rb.linearVelocity = Vector2.down * fallSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
