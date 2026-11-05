using System.Collections;
using UnityEngine;
using TMPro;

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager Instance;

    [Header("미니게임 UI")]
    public GameObject miniGamePanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI guideText;

    [Header("시작 딜레이")]
    public float startDelay = 1.0f;     // 쓰레기를 버린 뒤 미니게임이 켜지기까지 대기 시간

    [Header("등장/퇴장 효과")]
    public CanvasGroup panelGroup;      // MiniGamePanel의 Canvas Group
    public RectTransform windowRect;    // Window 오브젝트
    public float openDuration = 0.35f;  // 켜지는 데 걸리는 시간
    public float closeDuration = 0.2f;  // 꺼지는 데 걸리는 시간
    public float startScale = 0.6f;     // 켜질 때 시작 크기
    public float endScale = 0.85f;      // 꺼질 때 마지막 크기

    [Header("완료 글씨 효과")]
    public TextMeshProUGUI completeText;     // 창이 닫힌 뒤 뜨는 글씨
    public string completeMessage = "Complete!";
    public float completeHoldTime = 0.7f;    // 글씨가 유지되는 시간

    [Header("완료 보상 (캐시)")]
    public int colaCanReward = 300;     // 콜라캔 완료 보상
    public int bottleReward = 300;      // 생수병 완료 보상
    public int glassReward = 300;       // 깨진 유리 완료 보상

    [Header("쓰레기별 미니게임")]
    public CanMiniGame canGame;
    public BottleMiniGame bottleGame;
    public GlassMiniGame glassGame;

    public bool IsPlaying { get; private set; }

    private string currentItem;
    private bool isShowing;      // 창이 실제로 켜진 상태
    private bool isClosing;
    private Coroutine effectRoutine;
    private Coroutine completeRoutine;
    private Vector2 completeBasePos;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 게임 시작 시 미니게임 패널 숨기기
        if (miniGamePanel != null)
            miniGamePanel.SetActive(false);

        if (completeText != null)
        {
            completeBasePos = completeText.rectTransform.anchoredPosition;
            completeText.gameObject.SetActive(false);
        }

        HideAllGames();
    }

    // 미니게임이 있는 쓰레기인지 확인
    public static bool HasMiniGame(string itemName)
    {
        return itemName == "생수병"
            || itemName == "깨진 유리"
            || itemName == "콜라캔";
    }

    // 쓰레기별 보상 금액
    private int GetReward(string itemName)
    {
        switch (itemName)
        {
            case "콜라캔": return colaCanReward;
            case "생수병": return bottleReward;
            case "깨진 유리": return glassReward;
        }

        return 0;
    }

    public void StartMiniGame(string itemName)
    {
        if (IsPlaying) return;
        if (!HasMiniGame(itemName)) return;

        if (miniGamePanel == null)
        {
            Debug.LogWarning("[미니게임 오류] MiniGamePanel이 연결되지 않았습니다.");
            return;
        }

        // 딜레이 중에도 다른 쓰레기를 못 버리게 바로 재생 중으로 표시
        IsPlaying = true;
        isShowing = false;
        isClosing = false;
        currentItem = itemName;

        if (effectRoutine != null) StopCoroutine(effectRoutine);
        effectRoutine = StartCoroutine(OpenRoutine(itemName));
    }

    public void EndMiniGame(bool success)
    {
        if (!IsPlaying || !isShowing || isClosing) return;

        Debug.Log($"[미니게임] {currentItem} 종료 - {(success ? "성공" : "실패")}");

        isClosing = true;

        if (effectRoutine != null) StopCoroutine(effectRoutine);
        effectRoutine = StartCoroutine(CloseRoutine(success));
    }

    // 미니게임 도중에 안내 문구를 바꿀 때 사용
    public void SetGuide(string guide)
    {
        if (guideText != null) guideText.text = guide;
    }

    // ───── 등장 효과 (딜레이 후 시작) ─────
    private IEnumerator OpenRoutine(string itemName)
    {
        // 뚜껑 열리는 모습을 볼 수 있게 잠깐 기다림
        yield return new WaitForSecondsRealtime(startDelay);

        HideAllGames();

        bool isKor = Localization.Current == Language.Korean;

        // 쓰레기별 제목, 안내 문구 설정 및 게임 켜기
        switch (itemName)
        {
            case "생수병":
                SetTexts(
                    isKor ? "생수병 라벨 떼기" : "Peel the Bottle Label",
                    isKor ? "라벨을 아래로 천천히 당겨 떼어 주세요! (너무 빠르면 찢어져요)"
                          : "Slowly pull the label down to peel it off! (Pulling too fast will tear it)"
                );
                if (bottleGame != null)
                    bottleGame.gameObject.SetActive(true);
                else
                    Debug.LogWarning("[미니게임 오류] BottleGame이 연결되지 않았습니다.");
                break;

            case "깨진 유리":
                SetTexts(
                    isKor ? "깨진 유리 포장하기" : "Wrap the Broken Glass",
                    isKor ? "유리 조각을 신문지 위로 옮겨 주세요!" : "Move the glass shards onto the newspaper!"
                );
                if (glassGame != null)
                    glassGame.gameObject.SetActive(true);   // 켜질 때 안내 문구를 스스로 설정함
                else
                    Debug.LogWarning("[미니게임 오류] GlassGame이 연결되지 않았습니다.");
                break;

            case "콜라캔":
                SetTexts(
                    isKor ? "콜라캔 찌그러뜨리기" : "Crush the Can",
                    isKor ? "캔을 연타해서 찌그러뜨려 주세요!" : "Tap repeatedly to crush the can!"
                );
                if (canGame != null)
                    canGame.gameObject.SetActive(true);
                else
                    Debug.LogWarning("[미니게임 오류] CanGame이 연결되지 않았습니다.");
                break;
        }

        isShowing = true;
        miniGamePanel.SetActive(true);
        Debug.Log($"[미니게임] {itemName} 미니게임 시작!");

        bool hasEffect = panelGroup != null && windowRect != null;

        if (hasEffect)
        {
            panelGroup.alpha = 0f;
            panelGroup.blocksRaycasts = false;   // 효과 중에는 클릭 막기
            windowRect.localScale = Vector3.one * startScale;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / Mathf.Max(0.01f, openDuration);
                float p = Mathf.Clamp01(t);

                panelGroup.alpha = Mathf.Clamp01(p * 2f);   // 빠르게 나타남
                float s = Mathf.LerpUnclamped(startScale, 1f, EaseOutBack(p));
                windowRect.localScale = Vector3.one * s;

                yield return null;
            }

            panelGroup.alpha = 1f;
            windowRect.localScale = Vector3.one;
            panelGroup.blocksRaycasts = true;
        }

        effectRoutine = null;
    }

    // ───── 퇴장 효과 ─────
    private IEnumerator CloseRoutine(bool success)
    {
        // 정리하기 전에 어떤 쓰레기였는지 기억해 둠 (보상 계산용)
        string finishedItem = currentItem;

        bool hasEffect = panelGroup != null && windowRect != null;

        if (hasEffect)
        {
            panelGroup.blocksRaycasts = false;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / Mathf.Max(0.01f, closeDuration);
                float p = Mathf.Clamp01(t);

                panelGroup.alpha = 1f - p;
                float s = Mathf.Lerp(1f, endScale, p);
                windowRect.localScale = Vector3.one * s;

                yield return null;
            }
        }

        // 완전히 닫힌 뒤 정리
        HideAllGames();

        if (miniGamePanel != null)
            miniGamePanel.SetActive(false);

        IsPlaying = false;
        isShowing = false;
        isClosing = false;
        currentItem = null;
        effectRoutine = null;

        // 성공했을 때만 보상 지급 + 완료 글씨 띄우기
        if (success)
        {
            GiveReward(finishedItem);

            if (completeRoutine != null) StopCoroutine(completeRoutine);
            completeRoutine = StartCoroutine(CompleteRoutine());
        }
    }

    // ───── 보상 지급 ─────
    private void GiveReward(string itemName)
    {
        int reward = GetReward(itemName);
        if (reward <= 0) return;

        if (CashManager.Instance != null)
        {
            CashManager.Instance.AddCash(reward);
            Debug.Log($"[미니게임] {itemName} 완료 보상 +{reward} 캐시");
        }
        else
        {
            Debug.LogWarning("[미니게임 오류] CashManager를 찾을 수 없어 보상을 지급하지 못했습니다.");
        }
    }

    // ───── 완료 글씨 효과 ─────
    private IEnumerator CompleteRoutine()
    {
        if (completeText == null) yield break;

        RectTransform rt = completeText.rectTransform;

        bool isKor = Localization.Current == Language.Korean;
        completeText.text = isKor ? completeMessage : completeMessageEn;
        completeText.gameObject.SetActive(true);

        // 통통 튀며 나타남
        float popTime = 0.3f;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / popTime;
            float p = Mathf.Clamp01(t);

            rt.localScale = Vector3.one * Mathf.LerpUnclamped(0.5f, 1f, EaseOutBack(p));
            rt.anchoredPosition = completeBasePos;
            SetCompleteAlpha(Mathf.Clamp01(p * 2f));

            yield return null;
        }

        rt.localScale = Vector3.one;
        SetCompleteAlpha(1f);

        // 잠깐 유지
        yield return new WaitForSecondsRealtime(completeHoldTime);

        // 위로 올라가며 사라짐
        float fadeTime = 0.3f;
        t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeTime;
            float p = Mathf.Clamp01(t);

            rt.anchoredPosition = completeBasePos + new Vector2(0f, 40f * p);
            SetCompleteAlpha(1f - p);

            yield return null;
        }

        rt.anchoredPosition = completeBasePos;
        completeText.gameObject.SetActive(false);
        completeRoutine = null;
    }

    private void SetCompleteAlpha(float a)
    {
        Color c = completeText.color;
        c.a = a;
        completeText.color = c;
    }

    // 끝에서 살짝 튕기는 느낌의 이징
    private float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }

    private void HideAllGames()
    {
        if (canGame != null)
            canGame.gameObject.SetActive(false);

        if (bottleGame != null)
            bottleGame.gameObject.SetActive(false);

        if (glassGame != null)
            glassGame.gameObject.SetActive(false);
    }

    private void SetTexts(string title, string guide)
    {
        if (titleText != null) titleText.text = title;
        if (guideText != null) guideText.text = guide;
    }

    [Header("완료 글씨 (영어)")]
    public string completeMessageEn = "Complete!";
}
