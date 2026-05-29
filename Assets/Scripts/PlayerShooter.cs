using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate   = 0.3f;
    [SerializeField] private float fireOffset = 0.5f;

    private float lastFireTime;
    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (!GameManager.Instance.IsPlaying) return;

        if (Input.GetKey(KeyCode.Space) && Time.time >= lastFireTime + fireRate)
        {
            Shoot();
            lastFireTime = Time.time;
        }
    }

    void Shoot()
    {
        bool facingRight = playerController.FacingRight;
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        float offsetX     = facingRight ? fireOffset : -fireOffset;

        Vector3 spawnPos = transform.position + new Vector3(offsetX, 0f, 0f);
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        bullet.GetComponent<Bullet>().Initialize(direction);
    }
}