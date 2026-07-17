using UnityEngine;

/// 강화의 종류. 새로운 종류를 추가하려면 여기에 넣고
/// UpgradeManager.ApplyUpgrade 의 switch 에도 case 를 더해야 한다.
/// (빨리 끝낸 학생용 심화 과제)
public enum UpgradeType
{
    MoveSpeed,      // 이동 속도
    AttackDamage,   // 공격력
    AttackSpeed,    // 공격 속도 (쿨타임 감소)
    MagnetRadius,   // 젬 흡수 범위
    MaxHealth,      // 최대 체력
    BulletCount,    // 발사 수
}

/// <summary>
/// 강화 카드 한 장의 설계도.
///
/// 카드를 "추가"하는 건 이 에셋을 만들어 UpgradeManager 목록에 넣기만 하면 된다.
/// 코딩이 필요 없다.
/// </summary>
[CreateAssetMenu(fileName = "NewUpgrade", menuName = "뱀서라이크/강화 데이터")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName = "새 강화";

    [TextArea]
    public string description = "설명";

    public Sprite icon;

    public UpgradeType type;

    [Tooltip("올려줄 양. AttackSpeed 는 '빼줄' 양이다 (쿨타임이 줄어야 빨라지므로)")]
    public float value = 1f;
}
