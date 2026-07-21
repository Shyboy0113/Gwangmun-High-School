using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어의 체력과 무적 시간.
/// 뱀서라이크는 즉사가 아니라 깎이면서 버티는 게임이다.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Tooltip("한 번 맞은 뒤 이 시간(초) 동안은 다시 맞지 않는다. " +
             "없으면 몬스터에 닿는 순간 프레임마다 맞아서 즉사한다")]
    [SerializeField] private float invincibleTime = 0.5f;

    [Header("UI")]
    [SerializeField] private Image healthBarFill;

    private PlayerStats stats;
    private int currentHp;

    // 0으로 두면 게임 시작 직후(Time.time 이 0에 가까울 때) 무적이 걸린다.
    private float lastHitTime = -999f;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Start()
    {
        currentHp = stats.maxHp;
        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        if (Time.time < lastHitTime + invincibleTime) return;

        currentHp -= damage;
        lastHitTime = Time.time;

        UpdateHealthUI();

        if (currentHp <= 0)
            GameManager.Instance.GameOver();
    }
    // ──────────────────────────────────────────

    /// 최대 체력 강화를 먹으면 그만큼 회복시킨다.
    public void Heal(int amount)
    {
        currentHp = Mathf.Min(currentHp + amount, stats.maxHp);
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthBarFill == null) return;
        if (stats.maxHp <= 0) return;

        healthBarFill.fillAmount = Mathf.Clamp01((float)currentHp / stats.maxHp);
    }
}
