using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 경험치를 모아 레벨업시키고, 레벨업하면 강화 카드를 띄운다.
/// 뱀서라이크의 성장 루프가 여기서 돈다.
/// </summary>
public class PlayerLevel : MonoBehaviour
{
    [Header("경험치 곡선")]
    [Tooltip("1레벨에서 2레벨로 가는 데 필요한 경험치")]
    [SerializeField] private int baseExpToLevel = 5;

    [Tooltip("레벨당 필요 경험치 증가 배율. 1.3이면 레벨마다 30%씩 늘어난다")]
    [SerializeField] private float expGrowthRate = 1.3f;

    [Header("연결")]
    [SerializeField] private UpgradeManager upgradeManager;

    [Header("UI")]
    [SerializeField] private Image expBarFill;
    [SerializeField] private TextMeshProUGUI levelText;

    private int currentLevel = 1;
    private int currentExp;
    private int expToNextLevel;

    public int CurrentLevel => currentLevel;

    void Start()
    {
        expToNextLevel = baseExpToLevel;
        UpdateUI();
    }

    // ─────────── 학생 빈칸 6 (3일차) ───────────
    // 젬을 먹으면 호출된다. 경험치를 더하고, 목표치를 넘었으면 레벨업.
    public void AddExp(int amount)
    {
        currentExp += amount;
        UpdateUI();

        if (currentExp >= expToNextLevel)
            LevelUp();
    }
    // ──────────────────────────────────────────

    void LevelUp()
    {
        // 초과분은 다음 레벨로 이월한다. 버리면 학생들이 손해라고 느낀다.
        currentExp -= expToNextLevel;
        currentLevel++;

        expToNextLevel = Mathf.RoundToInt(baseExpToLevel * Mathf.Pow(expGrowthRate, currentLevel - 1));

        UpdateUI();

        if (upgradeManager != null) upgradeManager.ShowCards();
    }

    void UpdateUI()
    {
        if (levelText != null) levelText.text = "Lv. " + currentLevel;

        if (expBarFill != null && expToNextLevel > 0)
            expBarFill.fillAmount = Mathf.Clamp01((float)currentExp / expToNextLevel);
    }
}
