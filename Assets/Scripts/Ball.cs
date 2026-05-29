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

    // 카메라 밖으로 나가면 자동 삭제 — 메모리 낭비 방지
    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
