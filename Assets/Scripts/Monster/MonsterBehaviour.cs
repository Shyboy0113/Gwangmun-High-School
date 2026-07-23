using UnityEngine;

/// <summary>
/// 몬스터가 "어떻게 움직이고 공격하는가"를 담는 부모 클래스.
///
/// 체력·피격·사망·젬 드롭은 MonsterController 가 맡는다. 여기는 행동만 안다.
/// 프리팹마다 이 클래스를 상속한 컴포넌트가 정확히 하나 붙어 있어야 한다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public abstract class MonsterBehaviour : MonoBehaviour
{
    protected MonsterData data;
    protected Rigidbody2D rb;
    protected Transform player;

    // 모든 몬스터가 공유하는 시각 처리(좌우 반전·이동 애니메이션)에 쓴다.
    // 자식은 속도(rb.linearVelocity)만 정하면 되고, 실제 반전/애니메이션은 부모가 맡는다.
    protected SpriteRenderer sr;
    protected Animator animator;

    // 플레이어를 매번 Find 하면 스폰마다 씬 전체를 훑게 된다.
    // 풀링 때문에 Init 이 초당 수십 번 불릴 수 있어 static 으로 캐싱한다.
    // 씬이 다시 로드되면 Unity 의 fake-null 로 자동 무효화된다.
    private static Transform cachedPlayer;
    
    protected static Transform FindPlayer()
    {
        if (cachedPlayer == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            cachedPlayer = go != null ? go.transform : null;
        }
        return cachedPlayer;
    }

    /// MonsterController.Init 이 데이터를 넘겨준다.
    public virtual void Init(MonsterData monsterData)
    {
        data = monsterData;

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();

        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        player = FindPlayer();
    }

    void FixedUpdate()
    {
        if (data == null) return;

        // 움직일 수 있는 상황인지 판단한다. (게임 진행 중 + 플레이어 존재)
        bool canAct = GameManager.Instance != null && GameManager.Instance.IsPlaying;
        if (canAct && player == null)
        {
            player = FindPlayer();
            canAct = player != null;
        }

        if (canAct)
            Tick();                       // 자식이 속도를 정한다.
        else
            rb.linearVelocity = Vector2.zero;

        // 자식이 정한 속도를 기준으로 반전/애니메이션을 부모가 일괄 적용한다.
        UpdateVisual();
    }

    /// 매 물리 프레임의 행동. 자식이 구현한다.
    protected abstract void Tick();

    /// <summary>
    /// 모든 몬스터 공통 시각 처리. 현재 속도를 보고 좌우 반전과 이동 애니메이션을 갱신한다.
    /// 자식은 속도만 정하면 되므로 반전/애니메이션 코드를 각자 둘 필요가 없다.
    /// </summary>
    protected virtual void UpdateVisual()
    {
        Vector2 velocity = rb.linearVelocity;
        bool moving = velocity.sqrMagnitude > 0.0001f;

        if (animator != null)
            animator.SetBool("isMoving", moving);

        // 수평 이동이 있을 때만 방향을 갱신한다. 멈추는 순간엔 마지막으로 보던 방향을 유지한다.
        if (sr != null && Mathf.Abs(velocity.x) > 0.01f)
            sr.flipX = velocity.x < 0f;
    }

    /// 죽는 순간 호출된다. 폭발형이 이걸 쓴다.
    public virtual void OnMonsterDeath() { }
}
