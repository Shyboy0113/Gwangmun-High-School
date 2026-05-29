using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    // 싱글톤 — 다른 스크립트에서 GameManager.Instance 로 접근
    public static GameManager Instance;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    // 외부에서는 읽기만 가능
    public bool IsPlaying { get; private set; }

    private int score;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        IsPlaying = true;
        gameOverPanel.SetActive(false);
        UpdateScoreUI();
    }

    void Update()
    {
        if (!IsPlaying) return;

        // 매 초마다 생존 점수 +1
        score += Mathf.RoundToInt(Time.deltaTime);
        UpdateScoreUI();
    }

    // 총알로 공 맞추면 외부에서 호출
    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        scoreText.text = "Score : " + score;
    }

    // 플레이어가 공에 맞으면 호출
    public void GameOver()
    {
        IsPlaying = false;
        gameOverPanel.SetActive(true);
        finalScoreText.text = "Final Score : " + score;
    }

    // 재시작 버튼에 연결
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}