using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverByStatus : MonoBehaviour
{
    [Header("체력바 (Slider)")]
    public Slider healthBar;

    [Header("사망 화면 패널 (DeathPanel)")]
    public GameObject deathPanel;

    [Header("버튼")]
    public Button respawnButton;   // 다시 시작
    public Button homeButton;      // 타이틀 화면으로

    [Header("이동할 씬 이름 (Build Profiles에 등록된 이름과 똑같이)")]
    public string respawnSceneName = "GameScene_Team"; // 다시 시작할 씬
    public string homeSceneName = "MainMenu";          // 타이틀 씬

    private bool isGameOver = false;
    private bool hasBeenFilled = false;

    void Start()
    {
        if (deathPanel != null) deathPanel.SetActive(false);

        if (respawnButton != null) respawnButton.onClick.AddListener(Respawn);
        if (homeButton != null) homeButton.onClick.AddListener(GoHome);
    }

    void Update()
    {
        if (isGameOver || healthBar == null) return;

        if (healthBar.value > healthBar.minValue)
            hasBeenFilled = true;

        if (hasBeenFilled && healthBar.value <= healthBar.minValue)
        {
            ShowDeathScreen();
        }
    }

    void ShowDeathScreen()
    {
        isGameOver = true;

        if (deathPanel != null) deathPanel.SetActive(true);

        // 게임 일시정지 (물고기, 플레이어 움직임 멈춤)
        Time.timeScale = 0f;
    }

    void Respawn()
    {
        Time.timeScale = 1f; // 꼭 원래대로 되돌리기
        SceneManager.LoadScene(respawnSceneName);
    }

    void GoHome()
    {
        Time.timeScale = 1f; // 꼭 원래대로 되돌리기
        SceneManager.LoadScene(homeSceneName);
    }
}