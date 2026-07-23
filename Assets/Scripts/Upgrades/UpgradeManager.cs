using System.Collections.Generic;
using UnityEngine;

// 강화의 종류
public enum UpgradeType
{
    MoveSpeed,      // 이동 속도
    AttackDamage,   // 공격력
    AttackSpeed,    // 공격 속도 (쿨타임 감소)
    MagnetRadius,   // 젬 흡수 범위
    MaxHealth,      // 최대 체력
    BulletCount,    // 총알 수
}

// 강화 카드 한 장의 정보. 인스펙터에서 직접 채운다 (에셋 파일 안 만들어도 됨).
[System.Serializable]
public class Upgrade
{
    public string upgradeName = "새 강화";
    [TextArea] public string description = "설명";
    public Sprite icon;
    public UpgradeType type;
    public float value = 1f;   // 올려줄 양 (AttackSpeed 는 줄여줄 양)
}

/// <summary>
/// 레벨업하면 게임을 멈추고 강화 카드 3장을 띄운다.
/// 하나 고르면 PlayerStats 의 숫자가 올라간다.
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    [Header("강화 목록 (6개 이상 넣기)")]
    public List<Upgrade> allUpgrades = new List<Upgrade>();

    [Header("UI")]
    public GameObject cardPanel;
    public UpgradeCardUI[] cards = new UpgradeCardUI[3];

    [Header("연결")]
    public PlayerStats stats;
    public PlayerHealth playerHealth;

    void Start()
    {
        if (cardPanel != null) cardPanel.SetActive(false);
    }

    // PlayerLevel 이 레벨업할 때 부른다
    public void ShowCards()
    {
        List<Upgrade> picked = PickRandom(cards.Length);
        if (picked.Count == 0) return;   // 강화 목록이 비어 있으면 넘어감

        Time.timeScale = 0f;   // 고를 동안 게임 멈춤
        if (cardPanel != null) cardPanel.SetActive(true);

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] == null) continue;

            bool hasCard = i < picked.Count;
            cards[i].gameObject.SetActive(hasCard);
            if (hasCard) cards[i].Show(picked[i], this);
        }
    }

    // 겹치지 않게 무작위로 count 개 뽑는다
    List<Upgrade> PickRandom(int count)
    {
        List<Upgrade> pool = new List<Upgrade>();
        for (int i = 0; i < allUpgrades.Count; i++)
            if (allUpgrades[i] != null) pool.Add(allUpgrades[i]);

        List<Upgrade> result = new List<Upgrade>();
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);   // 뽑은 건 빼서 중복 방지
        }
        return result;
    }

    // 카드를 클릭하면 부른다. 고른 강화만큼 숫자를 올린다.
    public void ApplyUpgrade(Upgrade upgrade)
    {
        switch (upgrade.type)
        {
            case UpgradeType.MoveSpeed:
                stats.moveSpeed += upgrade.value;
                break;

            case UpgradeType.AttackDamage:
                stats.damage += (int)upgrade.value;
                break;

            case UpgradeType.AttackSpeed:
                // 쿨타임을 빼는 것이라, 0 이하로 내려가지 않게 막는다
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
        Time.timeScale = 1f;   // 다시 게임 시작
    }
}
