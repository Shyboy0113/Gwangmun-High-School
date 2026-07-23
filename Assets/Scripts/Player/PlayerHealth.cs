using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어의 체력과 무적 시간.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    //무적 시간 설정
    [SerializeField] private float invincibleTime = 0.5f;

    //체력 UI 설정
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
