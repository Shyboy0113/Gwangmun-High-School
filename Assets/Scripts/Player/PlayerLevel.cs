using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 경험치를 모아 레벨업시키고, 레벨업하면 강화 카드를 띄운다.
/// 뱀서라이크의 성장 루프가 여기서 돈다.
/// </summary>
public class PlayerLevel : MonoBehaviour
{
    //기본 경험치 요구
    [SerializeField] private int baseExpToLevel = 5;

    //레벨업마다 늘어나는 요구 경험치 배율 지금은 1.3배
    [SerializeField] private float expGrowthRate = 1.3f;

    //업그레이드 매니저 직접참조
    [SerializeField] private UpgradeManager upgradeManager;

    //플레이어 경험치 바
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

    
    public void AddExp(int amount)
    {
        currentExp += amount;
        UpdateUI();

        if (currentExp >= expToNextLevel)
            LevelUp();
    }

    void LevelUp()
    {
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
