using UnityEngine;

/// <summary>
/// 배경 타일을 플레이어 위치에 맞춰 격자 단위로 스냅시켜 무한히 이어지는 것처럼 보이게 한다.
/// (배포본 — 학생은 안 건드린다)
///
/// 전제: 이 오브젝트의 스프라이트가 tileSize 크기로 반복(Tiled) 설정돼 있고,
/// 화면보다 충분히 커야 한다. 그래야 스냅하는 순간이 눈에 안 보인다.
/// </summary>
public class InfiniteBackground : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Tooltip("배경 스프라이트 한 칸의 크기. 이 단위로 스냅한다")]
    [SerializeField] private float tileSize = 20f;

    void Start()
    {
        if (target == null) FindTarget();
    }

    void LateUpdate()
    {
        if (target == null)
        {
            FindTarget();
            if (target == null) return;
        }

        if (tileSize <= 0f) return;

        float x = Mathf.Round(target.position.x / tileSize) * tileSize;
        float y = Mathf.Round(target.position.y / tileSize) * tileSize;

        transform.position = new Vector3(x, y, transform.position.z);
    }

    void FindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;
    }
}
