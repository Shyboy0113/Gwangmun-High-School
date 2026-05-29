using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate    = 0.3f; // 몇 초에 한 번 발사 가능
    [SerializeField] private Vector3 fireOffset = new Vector3(0f, 0.5f, 0f); // 플레이어 위쪽에서 발사

    private float lastFireTime;

    void Update()
    {
        if (!GameManager.Instance.IsPlaying) return;

        // 스페이스바 + 쿨타임 체크
        if (Input.GetKey(KeyCode.Space) && Time.time >= lastFireTime + fireRate)
        {
            Shoot();
            lastFireTime = Time.time;
        }
    }

    void Shoot()
    {
        Vector3 spawnPos = transform.position + fireOffset;
        Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
    }
}