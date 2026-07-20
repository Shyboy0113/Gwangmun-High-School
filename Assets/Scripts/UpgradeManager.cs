using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 레벨업하면 게임을 멈추고 강화 카드 3장을 띄운다.
/// 학생이 하나 고르면 PlayerStats 의 숫자가 올라간다.
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    [Header("강화 목록")]
    [Tooltip("여기 있는 것 중에서 3장을 중복 없이 뽑는다. " +
             "6종 미만이면 매번 같은 카드만 나와 뽑는 재미가 없다")]
    [SerializeField] private List<UpgradeData> allUpgrades = new List<UpgradeData>();

    [Header("UI")]
    [SerializeField] private GameObject cardPanel;
    [SerializeField] private UpgradeCardUI[] cards = new UpgradeCardUI[3];

    [Header("연결")]
    [SerializeField] private PlayerStats stats;
    [SerializeField] private PlayerHealth playerHealth;

    private readonly List<UpgradeData> pickBuffer = new List<UpgradeData>();
    private readonly List<UpgradeData> candidateBuffer = new List<UpgradeData>();

    void Start()
    {
        if (cardPanel != null) cardPanel.SetActive(false);
    }

    /// PlayerLevel 이 레벨업할 때 호출한다.
    public void ShowCards()
    {
        PickRandom(cards.Length);

        if (pickBuffer.Count == 0) return;   // 강화 목록이 비어 있으면 그냥 넘어간다

        Time.timeScale = 0f;

        if (cardPanel != null) cardPanel.SetActive(true);

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] == null) continue;

            bool hasCard = i < pickBuffer.Count;
            cards[i].gameObject.SetActive(hasCard);

            if (hasCard) cards[i].Show(pickBuffer[i], this);
        }
    }

    /// 중복 없이 랜덤으로 count 장을 뽑는다. (배포본 — 학생은 안 건드린다)
    void PickRandom(int count)
    {
        pickBuffer.Clear();

        candidateBuffer.Clear();
        for (int i = 0; i < allUpgrades.Count; i++)
            if (allUpgrades[i] != null) candidateBuffer.Add(allUpgrades[i]);

        for (int i = 0; i < count && candidateBuffer.Count > 0; i++)
        {
            int index = Random.Range(0, candidateBuffer.Count);
            pickBuffer.Add(candidateBuffer[index]);
            candidateBuffer.RemoveAt(index);   // 뽑은 건 후보에서 빼야 중복이 안 나온다
        }
    }

    // ─────────── 학생 빈칸 7 (3일차) ───────────
    // 카드를 클릭하면 호출된다. 고른 강화에 따라 창고의 숫자를 올린다.
    //
    // AttackSpeed 에만 Mathf.Max 가 붙은 걸 눈여겨볼 것.
    // 공속 강화는 쿨타임을 "빼는" 것이라, 0 이하로 내려가면
    // 한 프레임에 총알이 무한히 나오면서 게임이 얼어붙는다.
    public void ApplyUpgrade(UpgradeData upgrade)
    {
        switch (upgrade.type)
        {
            case UpgradeType.MoveSpeed:
                stats.moveSpeed += upgrade.value;
                break;

            case UpgradeType.AttackDamage:
                stats.damage += upgrade.value;
                break;

            case UpgradeType.AttackSpeed:
                stats.fireRate = Mathf.Max(0.05f, stats.fireRate - upgrade.value);
                break;

            case UpgradeType.MagnetRadius:
                stats.magnetRadius += upgrade.value;
                break;

            case UpgradeType.MaxHealth:
                stats.maxHp += (int)upgrade.value;
                if (playerHealth != null) playerHealth.Heal((int)upgrade.value);
                break;

            case UpgradeType.BulletCount:
                stats.bulletCount += (int)upgrade.value;
                break;
        }

        if (cardPanel != null) cardPanel.SetActive(false);
        Time.timeScale = 1f;
    }
    // ──────────────────────────────────────────
}
