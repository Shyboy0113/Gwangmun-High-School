using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 게임 상태와 생존 시간을 관리한다.
/// 뱀서라이크의 점수는 사실상 얼마나 오래 살아남았느냐다.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalTimeText;

    public bool IsPlaying { get; private set; }

    /// 게임 시작 후 흐른 시간(초). MonsterSpawner 가 appearTime 판정에 쓴다.
    public float SurvivedTime { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 강화 카드가 뜬 상태(timeScale = 0)에서 재시작하면
        // 게임이 멈춘 채로 시작된다. 반드시 되돌린다.
        Time.timeScale = 1f;

        IsPlaying = true;
        SurvivedTime = 0f;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        UpdateTimerUI();
    }

    void Update()
    {
        if (!IsPlaying) return;

        SurvivedTime += Time.deltaTime;
        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(SurvivedTime / 60f);
        int seconds = Mathf.FloorToInt(SurvivedTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    /// 플레이어 체력이 0 이하가 되면 PlayerHealth 가 호출한다.
    public void GameOver()
    {
        if (!IsPlaying) return;   // 중복 호출 무시

        IsPlaying = false;
        Time.timeScale = 0f;

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (finalTimeText != null)
        {
            int minutes = Mathf.FloorToInt(SurvivedTime / 60f);
            int seconds = Mathf.FloorToInt(SurvivedTime % 60f);
            finalTimeText.text = string.Format("생존 시간  {0:00}:{1:00}", minutes, seconds);
        }
    }

    /// 재시작 버튼에 연결한다.
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
