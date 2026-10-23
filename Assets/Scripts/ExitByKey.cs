using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class ExitByKey : MonoBehaviour
{
    [Header("이동할 씬 이름 (Build Profiles에 등록된 이름과 똑같이)")]
    public string workshopSceneName = "GameScene";

    [Header("나가기 키")]
    public KeyCode exitKey = KeyCode.B;

    [Header("결과창")]
    public GameObject resultPanel;        // ResultPanel 연결
    public TMP_Text resultStatsText;      // ResultStatsText 연결
    public Button leaveButton;            // ResultLeaveButton 연결 (작업장으로 나가기)
    public Button closeButton;            // ResultCloseButton 연결 (계속 탐험하기)

    [Header("결과창 연출")]
    public float openDuration = 0.35f;    // 나타나는 시간
    public float closeDuration = 0.2f;    // 사라지는 시간
    public float startScale = 0.8f;       // 처음 크기 (1이 원래 크기)

    [Header("페이드")]
    public CanvasGroup fadePanel;        // ExitFadePanel 연결
    public float fadeInDuration = 0.5f;  // 씬 시작 시 밝아지는 시간
    public float fadeOutDuration = 0.5f; // 나갈 때 어두워지는 시간

    private bool isLeaving = false;
    private bool isResultOpen = false;

    private CanvasGroup resultGroup;
    private RectTransform resultRect;
    private Coroutine panelAnim;

    void Start()
    {
        if (resultPanel != null)
        {
            // 연출용 CanvasGroup / RectTransform 준비 (없으면 자동 추가)
            resultGroup = resultPanel.GetComponent<CanvasGroup>();
            if (resultGroup == null)
                resultGroup = resultPanel.AddComponent<CanvasGroup>();

            resultRect = resultPanel.GetComponent<RectTransform>();

            // 결과창은 처음에 꺼두기
            resultPanel.SetActive(false);
        }

        if (leaveButton != null)
            leaveButton.onClick.AddListener(LeaveToWorkshop);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseResult);

        if (fadePanel != null)
        {
            // 씬 시작 시: 켜서 밝아지는 연출 → 끝나면 다시 끔
            fadePanel.gameObject.SetActive(true);
            fadePanel.alpha = 1f;
            fadePanel.blocksRaycasts = false;
            StartCoroutine(Fade(1f, 0f, fadeInDuration, () =>
            {
                fadePanel.gameObject.SetActive(false);
            }));
        }
    }

    void Update()
    {
        if (isLeaving) return;

        if (Input.GetKeyDown(exitKey))
        {
            // 결과창이 열려 있으면 닫고, 닫혀 있으면 연다
            if (isResultOpen)
                CloseResult();
            else
                OpenResult();
        }
    }

    // 결과창 열기: 기록을 보여주고 게임 멈춤
    void OpenResult()
    {
        // 결과창이 연결 안 되어 있으면 예전처럼 바로 나감
        if (resultPanel == null)
        {
            LeaveToWorkshop();
            return;
        }

        isResultOpen = true;

        if (resultStatsText != null)
        {
            // 쓰레기 개수와 캐시는 "이번 탐험" 기준, 정화율은 이전 탐험까지 합친 값
            resultStatsText.text =
                "수거한 쓰레기: " + RunStats.trashCount + "개\n" +
                "번 캐시: " + RunStats.cashEarned + "\n" +
                "정화율: " + RunStats.PurificationPercent.ToString("F0") + "%";
        }

        // 꺼져 있던 창이면 처음 상태(투명 + 작게)로 맞추고 켜기
        if (!resultPanel.activeSelf)
        {
            resultGroup.alpha = 0f;
            resultRect.localScale = Vector3.one * startScale;
            resultPanel.SetActive(true);
        }

        Time.timeScale = 0f;

        if (panelAnim != null) StopCoroutine(panelAnim);
        panelAnim = StartCoroutine(AnimatePanel(true));
    }

    // 결과창 닫기: 게임 이어하기
    void CloseResult()
    {
        if (!isResultOpen) return;
        isResultOpen = false;

        Time.timeScale = 1f;

        if (resultPanel == null) return;

        if (panelAnim != null) StopCoroutine(panelAnim);
        panelAnim = StartCoroutine(AnimatePanel(false));
    }

    // 결과창 나타나기/사라지기 연출 (페이드 + 크기)
    IEnumerator AnimatePanel(bool opening)
    {
        float startAlpha = resultGroup.alpha;
        float startScaleNow = resultRect.localScale.x;

        float endAlpha = opening ? 1f : 0f;
        float endScale = opening ? 1f : startScale;
        float duration = opening ? openDuration : closeDuration;

        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);

            // 열 때는 살짝 튕기며 커지고, 닫을 때는 부드럽게 작아짐
            float scaleP = opening ? EaseOutBack(p) : p;

            resultGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, p);
            resultRect.localScale = Vector3.one * Mathf.LerpUnclamped(startScaleNow, endScale, scaleP);

            yield return null;
        }

        resultGroup.alpha = endAlpha;
        resultRect.localScale = Vector3.one * endScale;

        if (!opening)
            resultPanel.SetActive(false);
    }

    // 목표를 살짝 넘어갔다가 돌아오는 느낌의 곡선
    float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }

    // 작업장으로 나가기
    void LeaveToWorkshop()
    {
        if (isLeaving) return;
        isLeaving = true;

        // 나가기 전에 이 바다의 수거 개수(정화율)를 저장
        RunStats.CommitRun();

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (fadePanel != null)
        {
            // 나갈 때: 패널을 켜고 어두워지게 함
            fadePanel.gameObject.SetActive(true);
            fadePanel.blocksRaycasts = true; // 나가는 중 클릭 막기
            StartCoroutine(Fade(0f, 1f, fadeOutDuration, () =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(workshopSceneName);
            }));
        }
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(workshopSceneName);
        }
    }

    IEnumerator Fade(float from, float to, float duration, System.Action onDone)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            fadePanel.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        fadePanel.alpha = to;
        onDone?.Invoke();
    }
}