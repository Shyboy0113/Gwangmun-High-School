using UnityEngine;

/// <summary>
/// 플레이어의 모든 수치를 한곳에 모아둔 창고.
/// 강화 카드는 이 숫자들만 올리고, 다른 스크립트는 여기서 읽기만 한다.
/// 이 중간층이 없으면 강화 카드가 여러 스크립트를 직접 건드려야 한다.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 5f;

    [Header("공격")]
    public float damage = 1f;

    [Tooltip("발사 간격(초). 작을수록 빠르다. 0 이하가 되면 게임이 얼어붙는다")]
    public float fireRate = 0.3f;

    [Tooltip("한 번에 나가는 총알 수. 2발 이상이면 부채꼴로 퍼진다")]
    public int bulletCount = 1;

    [Header("생존")]
    public int maxHp = 10;

    [Header("수집")]
    [Tooltip("이 반경 안의 경험치 젬이 플레이어에게 빨려온다")]
    public float magnetRadius = 2f;
}
