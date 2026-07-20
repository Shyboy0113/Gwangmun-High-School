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
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        player = FindPlayer();
    }

    void FixedUpdate()
    {
        if (data == null) return;

        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (player == null)
        {
            player = FindPlayer();
            if (player == null)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }
        }

        Tick();
    }

    /// 매 물리 프레임의 행동. 자식이 구현한다.
    protected abstract void Tick();

    /// 죽는 순간 호출된다. 폭발형이 이걸 쓴다.
    public virtual void OnMonsterDeath() { }
}
