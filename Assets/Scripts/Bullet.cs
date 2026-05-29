using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float lifeTime  = 3f; // 일정 시간 후 자동 삭제

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 위쪽으로 이동
        rb.linearVelocity = Vector2.up * moveSpeed;

        // lifeTime 초 후 자동 삭제
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            GameManager.Instance.AddScore(10);
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}