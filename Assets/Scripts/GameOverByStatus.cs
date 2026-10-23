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
    public GameObject deathPanel;
    public CanvasGroup deathPanelGroup;
    public TMP_Text statsText;
    public Image flashImage;

    [Header("버튼")]
    public Button respawnButton;
    public Button homeButton;

    [Header("사망 후 재시작 체력")]
    public float respawnHealth = 3f;

    [Header("씬 이름")]
    public string homeSceneName = "TitleScene";

    [Header("이 바다의 전체 쓰레기 수 (정화율 계산용)")]
    public int totalTrashInScene = 10;

    [Header("사망 연출")]
    public float slowMotionScale = 0.3f;
    public float flashDuration = 0.5f;
    public float shakePower = 0.3f;
    public float shakeDuration = 0.6f;
    public float panelFadeDuration = 1f;

    private bool isGameOver = false;
    private bool hasBeenFilled = false;

    // 현재 플레이 중인 스테이지 이름 저장
    private string currentSceneName;

    void Start()
    {
        Time.timeScale = 1f;

        // 현재 플레이 중인 씬 이름 기억
        currentSceneName = SceneManager.GetActiveScene().name;

        // 바다에 들어온 순간의 상태를 저장점으로 기록
        CollectedRecord.SaveCheckpoint();
        InventoryStore.SaveCheckpoint();
        RunStats.StartNewRun();
        RunStats.SetTotalTrash(totalTrashInScene);

        if (CashManager.Instance != null)
            CashManager.Instance.SaveCheckpoint();

        if (deathPanel != null)
            deathPanel.SetActive(false);

        if (flashImage != null)
        {
            Color c = flashImage.color;
            c.a = 0f;
            flashImage.color = c;
        }

        if (respawnButton != null)
            respawnButton.onClick.AddListener(Respawn);

        if (homeButton != null)
            homeButton.onClick.AddListener(GoHome);
    }

    void Update()
    {
        if (isGameOver || healthBar == null)
            return;

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

        // 2. 붉은 번쩍임과 카메라 흔들림
        Camera cam = Camera.main;
        Vector3 originalCameraPosition = Vector3.zero;

        if (cam != null)
            originalCameraPosition = cam.transform.position;

        float t = 0f;
        float total = Mathf.Max(flashDuration, shakeDuration);

        while (t < total)
        {
            t += Time.unscaledDeltaTime;

            if (flashImage != null && t < flashDuration)
            {
                Color c = flashImage.color;
                c.a = Mathf.Lerp(0.6f, 0f, t / flashDuration);
                flashImage.color = c;
            }

            if (cam != null && t < shakeDuration)
            {
                float power = shakePower * (1f - t / shakeDuration);
                cam.transform.position =
                    originalCameraPosition +
                    (Vector3)(Random.insideUnitCircle * power);
            }

            yield return null;
        }

        // 카메라 원래 위치로 복원
        if (cam != null)
            cam.transform.position = originalCameraPosition;

        // 3. 사망 기록 표시
        if (statsText != null)
        {
            statsText.text =
                "수거한 쓰레기: " + RunStats.trashCount + "개\n" +
                "잡은 물고기: " + RunStats.fishCaught + "마리\n" +
                "번 캐시: " + RunStats.cashEarned;
        }

        // 4. 사망 창 표시
        if (deathPanel != null)
            deathPanel.SetActive(true);

        if (deathPanelGroup != null)
            deathPanelGroup.alpha = 0f;

        t = 0f;

        while (t < panelFadeDuration)
        {
            t += Time.unscaledDeltaTime;

            if (deathPanelGroup != null)
            {
                deathPanelGroup.alpha =
                    Mathf.Lerp(0f, 1f, t / panelFadeDuration);
            }

            yield return null;
        }

        if (deathPanelGroup != null)
            deathPanelGroup.alpha = 1f;

        // 5. 게임 정지
        Time.timeScale = 0f;
    }

    // 다시 시작: 죽었던 스테이지를 다시 불러오기
    void Respawn()
    {
        Time.timeScale = 1f;

        PlayerVitals.savedHealth = respawnHealth;

        CollectedRecord.RestoreCheckpoint();
        InventoryStore.RestoreCheckpoint();
        RunStats.RestoreCheckpoint();

        if (CashManager.Instance != null)
            CashManager.Instance.RestoreCheckpoint();

        // 해안으로 고정하지 않고 현재 씬을 다시 로드
        SceneManager.LoadScene(currentSceneName);
    }

    // 홈으로 이동
    void GoHome()
    {
        Time.timeScale = 1f;

        PlayerVitals.savedHealth = respawnHealth;

        CollectedRecord.RestoreCheckpoint();
        InventoryStore.RestoreCheckpoint();
        RunStats.RestoreCheckpoint();

        if (CashManager.Instance != null)
            CashManager.Instance.RestoreCheckpoint();

        SceneManager.LoadScene(homeSceneName);
    }
}