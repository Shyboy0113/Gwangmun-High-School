using UnityEngine;

/// <summary>
/// 효과음(SFX)을 재생하는 컴포넌트.
///
/// 싱글톤이 아니다. GameManager 와 같은 GameObject 에 붙고,
/// GameManager 가 GetComponent 로 잡아 GameManager.Instance.Sfx 로 노출한다.
///
/// 이벤트마다 이름 붙은 메서드(PlayShoot 등)를 제공한다. 각 메서드는
/// 자기 클립을 null 체크하므로, Inspector 에서 클립을 안 넣은 효과음은
/// 자동으로 무음이 된다. → 모든 이벤트에 배선하되 실제 소리 여부는
/// 클립을 넣느냐로 결정한다.
/// </summary>
public class SFXManager : MonoBehaviour
{
    [Header("설정")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.7f;

    [Header("플레이어")]
    [SerializeField] private AudioClip shoot;
    [SerializeField] private AudioClip hit;
    [SerializeField] private AudioClip playerHurt;
    [SerializeField] private AudioClip gemPickup;
    [SerializeField] private AudioClip levelUp;
    [SerializeField] private AudioClip upgradeSelect;

    [Header("몬스터")]
    [SerializeField] private AudioClip monsterDeath;
    [SerializeField] private AudioClip explosion;
    [SerializeField] private AudioClip enemyShoot;

    [Header("게임 종료")]
    [SerializeField] private AudioClip gameOver;
    [SerializeField] private AudioClip victory;

    // 겹쳐 나는 효과음을 위해 PlayOneShot 을 쓴다. loop 는 항상 꺼 둔다.
    private AudioSource source;

    void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.loop = false;
        source.playOnAwake = false;
    }

    public void PlayShoot()         { Play(shoot); }
    public void PlayHit()           { Play(hit); }
    public void PlayPlayerHurt()    { Play(playerHurt); }
    public void PlayGemPickup()     { Play(gemPickup); }
    public void PlayLevelUp()       { Play(levelUp); }
    public void PlayUpgradeSelect() { Play(upgradeSelect); }
    public void PlayMonsterDeath()  { Play(monsterDeath); }
    public void PlayExplosion()     { Play(explosion); }
    public void PlayEnemyShoot()    { Play(enemyShoot); }
    public void PlayGameOver()      { Play(gameOver); }
    public void PlayVictory()       { Play(victory); }

    /// 볼륨을 0~1 사이로 바꾼다.
    public void SetVolume(float value)
    {
        volume = Mathf.Clamp01(value);
    }

    // 클립이 없으면 조용히 넘어간다.
    // PlayOneShot 은 timeScale=0(강화 카드·게임오버) 상태에서도 정상 재생된다.
    void Play(AudioClip clip)
    {
        if (source == null || clip == null) return;
        source.PlayOneShot(clip, volume);
    }
}
