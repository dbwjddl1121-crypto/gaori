using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameOverByStatus : MonoBehaviour
{
    [Header("체력바 (Slider)")]
    public Slider healthBar;

    [Header("사망 화면")]
    public GameObject deathPanel;        // DeathPanel
    public CanvasGroup deathPanelGroup;  // DeathPanel의 Canvas Group
    public TMP_Text statsText;           // StatsText
    public Image flashImage;             // DeathFlash의 Image

    [Header("버튼")]
    public Button respawnButton;
    public Button homeButton;

    [Header("사망 후 재시작 체력")]
    public float respawnHealth = 3f;   // 체력 '한 칸' 분량 (Health Damage Per Tick 값과 똑같이)

    [Header("씬 이름 (Build Profiles에 등록된 이름과 똑같이)")]
    public string respawnSceneName = "GameScene_Team";
    public string homeSceneName = "MainMenu";

    [Header("사망 연출")]
    public float slowMotionScale = 0.3f;  // 슬로모션 속도
    public float flashDuration = 0.5f;    // 붉은 번쩍임 시간
    public float shakePower = 0.3f;       // 카메라 흔들림 세기
    public float shakeDuration = 0.6f;    // 카메라 흔들림 시간
    public float panelFadeDuration = 1f;  // 사망 창이 나타나는 시간

    private bool isGameOver = false;
    private bool hasBeenFilled = false;

    void Start()
    {
        Time.timeScale = 1f;

        // 바다에 들어온 순간의 상태를 저장점으로 기록
        CollectedRecord.SaveCheckpoint();
        InventoryStore.SaveCheckpoint();
        RunStats.StartNewRun();   // 입장할 때마다 이번 기록을 0으로
        if (CashManager.Instance != null) CashManager.Instance.SaveCheckpoint();

        if (deathPanel != null) deathPanel.SetActive(false);
        if (flashImage != null)
        {
            Color c = flashImage.color;
            c.a = 0f;
            flashImage.color = c;
        }

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
            isGameOver = true;
            StartCoroutine(DeathSequence());
        }
    }

    IEnumerator DeathSequence()
    {
        // 1. 슬로모션 시작
        Time.timeScale = slowMotionScale;

        // 2. 붉은 번쩍임 + 카메라 흔들림 (동시에 진행)
        Camera cam = Camera.main;
        float t = 0f;
        float total = Mathf.Max(flashDuration, shakeDuration);

        while (t < total)
        {
            t += Time.unscaledDeltaTime;

            // 붉은 번쩍임: 확 켜졌다가 점점 옅어짐
            if (flashImage != null && t < flashDuration)
            {
                Color c = flashImage.color;
                c.a = Mathf.Lerp(0.6f, 0f, t / flashDuration);
                flashImage.color = c;
            }

            // 카메라 흔들림: 점점 약해짐
            if (cam != null && t < shakeDuration)
            {
                float power = shakePower * (1f - t / shakeDuration);
                cam.transform.position += (Vector3)(Random.insideUnitCircle * power);
            }

            yield return null;
        }

        // 3. 기록 표시 문구 만들기
        if (statsText != null)
        {
            statsText.text =
                "수거한 쓰레기: " + RunStats.trashCount + "개\n" +
                "잡은 물고기: " + RunStats.fishCaught + "마리\n" +
                "번 캐시: " + RunStats.cashEarned;
        }

        // 4. 사망 창 서서히 나타나기
        if (deathPanel != null) deathPanel.SetActive(true);
        if (deathPanelGroup != null) deathPanelGroup.alpha = 0f;

        t = 0f;
        while (t < panelFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            if (deathPanelGroup != null)
                deathPanelGroup.alpha = Mathf.Lerp(0f, 1f, t / panelFadeDuration);
            yield return null;
        }
        if (deathPanelGroup != null) deathPanelGroup.alpha = 1f;

        // 5. 게임 완전 정지
        Time.timeScale = 0f;
    }

    // 다시 시작: 마지막으로 들어왔을 때 상태로 복원
    void Respawn()
    {
        Time.timeScale = 1f;
        PlayerVitals.savedHealth = respawnHealth;   // 추가
        CollectedRecord.RestoreCheckpoint();
        InventoryStore.RestoreCheckpoint();   // 추가됨
        RunStats.RestoreCheckpoint();
        if (CashManager.Instance != null) CashManager.Instance.RestoreCheckpoint();
        SceneManager.LoadScene(respawnSceneName);
    }

    // 타이틀로: 다시하기와 똑같이 마지막 입장 시점 상태로 복원
    void GoHome()
    {
        Time.timeScale = 1f;
        PlayerVitals.savedHealth = respawnHealth;   // 추가
        CollectedRecord.RestoreCheckpoint();
        InventoryStore.RestoreCheckpoint();
        RunStats.RestoreCheckpoint();
        if (CashManager.Instance != null) CashManager.Instance.RestoreCheckpoint();
        SceneManager.LoadScene(homeSceneName);
    }
}