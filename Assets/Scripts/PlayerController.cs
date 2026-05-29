using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;

    public bool FacingRight { get; private set; } = true;

    private Rigidbody2D rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");

        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);

        if (horizontal > 0) FacingRight = true;
        else if (horizontal < 0) FacingRight = false;

        if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
            GameManager.Instance.GameOver();

        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}