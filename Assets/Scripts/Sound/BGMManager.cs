using UnityEngine;

/// <summary>
/// 배경음악(BGM)을 재생하는 컴포넌트.
///
/// 싱글톤이 아니다. GameManager 와 같은 GameObject 에 붙고,
/// GameManager 가 GetComponent 로 잡아 GameManager.Instance.Bgm 으로 노출한다.
/// (싱글톤은 GameManager 와 PoolManager 둘로 제한한다는 규칙을 따른다.)
/// </summary>
public class BGMManager : MonoBehaviour
{
    [Header("클립")]
    [Tooltip("게임 시작과 함께 반복 재생할 기본 배경음악")]
    [SerializeField] private AudioClip mainBgm;

    [Tooltip("(선택) 보스전용 배경음악. PlayBoss() 로 전환한다")]
    [SerializeField] private AudioClip bossBgm;

    [Header("설정")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.5f;

    // AudioSource 는 수동으로 붙이지 않는다. 여기서 직접 만들어
    // loop/playOnAwake 를 코드로 확정한다. (SFXManager 와 설정이 충돌하지 않게)
    private AudioSource source;

    void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;
        source.volume = volume;
    }

    /// 기본 배경음악을 재생한다. GameManager.Start 에서 호출한다.
    public void PlayMain()
    {
        Play(mainBgm);
    }

    /// 보스 배경음악으로 전환한다. (원하는 보스 스폰 지점에서 호출)
    public void PlayBoss()
    {
        Play(bossBgm);
    }

    /// 배경음악을 멈춘다. 게임오버·승리 시 호출한다.
    public void Stop()
    {
        if (source != null) source.Stop();
    }

    /// 볼륨을 0~1 사이로 바꾼다.
    public void SetVolume(float value)
    {
        volume = Mathf.Clamp01(value);
        if (source != null) source.volume = volume;
    }

    // 클립이 없으면 조용히 넘어간다. 같은 곡을 다시 요청하면 끊지 않고 그대로 둔다.
    void Play(AudioClip clip)
    {
        if (source == null || clip == null) return;
        if (source.isPlaying && source.clip == clip) return;

        source.clip = clip;
        source.volume = volume;
        source.Play();
    }
}
