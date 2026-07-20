using UnityEngine;

/// <summary>
/// 카메라가 플레이어를 부드럽게 따라간다. (배포본 — 학생은 안 건드린다)
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    [Tooltip("작을수록 딱 붙어 따라온다. 0이면 즉시 추적")]
    [SerializeField] private float smoothTime = 0.15f;

    private Vector3 velocity;

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

        // z 는 유지해야 한다. 건드리면 카메라가 평면 속으로 들어가 아무것도 안 보인다.
        Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);

        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }

    void FindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) target = player.transform;
    }
}
