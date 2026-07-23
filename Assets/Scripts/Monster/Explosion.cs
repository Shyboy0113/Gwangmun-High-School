using UnityEngine;

/// <summary>
/// 폭발 이펙트의 수명 관리. 풀에서 꺼내지면 lifetime 초 뒤 스스로 풀에 반납한다.
///
/// 데미지 판정은 ExploderBehaviour 가 처리하고, 이 스크립트는 "잠깐 보여주고 사라지기"만 맡는다.
/// 이게 없으면 폭발 스프라이트가 화면에 붙박이로 남고 풀에도 반납되지 않는다.
/// </summary>
public class Explosion : MonoBehaviour
{
    [Tooltip("폭발 스프라이트가 화면에 유지되는 시간(초)")]
    [SerializeField] private float lifetime = 0.5f;

    // 풀에서 꺼내질 때마다(SetActive(true)) OnEnable 이 불려 타이머를 새로 건다.
    void OnEnable()
    {
        CancelInvoke();
        Invoke(nameof(Despawn), lifetime);
    }

    // 반납되어 비활성될 때 예약을 지운다. 재사용 시 이전 타이머가 겹쳐 조기 반납되는 것을 막는다.
    void OnDisable()
    {
        CancelInvoke();
    }

    void Despawn()
    {
        if (PoolManager.Instance != null)
            PoolManager.Instance.Release(gameObject);
        else
            Destroy(gameObject);
    }
}
