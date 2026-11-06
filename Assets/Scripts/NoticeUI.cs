using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class NoticeUI : MonoBehaviour
{
    static NoticeUI instance;

    [Header("폰트 (비워두면 맑은 고딕 자동 사용)")]
    public TMP_FontAsset fontAsset;

    [Header("글씨")]
    public float fontSize = 24f;
    public Color textColor = Color.white;
    public TextAlignmentOptions alignment = TextAlignmentOptions.BottomRight;

    [Header("테두리 (두께 0이면 없음)")]
    public Color outlineColor = Color.black;
    [Range(0f, 1f)] public float outlineWidth = 0.25f;

    [Header("위치 / 영역 (화면 오른쪽 아래 기준)")]
    public Vector2 margin = new Vector2(40f, 40f);     // 오른쪽, 아래에서 떨어진 거리
    public Vector2 boxSize = new Vector2(700f, 50f);   // 글씨가 들어가는 영역

    [Header("표시 시간")]
    public float showDuration = 1.8f;
    public float fadeInTime = 0.15f;
    public float fadeOutTime = 0.3f;

    [Header("테스트 (실행 중 체크하면 문구가 계속 보임)")]
    public bool keepShowing = false;
    public string testKorean = "상점이 너무 멀리 있습니다.";
    public string testEnglish = "The shop is too far away.";

    CanvasGroup group;
    RectTransform textRect;
    TMP_Text label;
    Coroutine routine;
    TMP_FontAsset autoFont;
    bool wasKeepShowing;

    // 사용법: NoticeUI.Show("한국어 문구", "English text");
    public static void Show(string ko, string en, float duration = -1f)
    {
        if (instance == null)
            instance = FindFirstObjectByType<NoticeUI>();

        // 씬에 NoticeUI 오브젝트가 없으면 기본 설정으로 자동 생성
        if (instance == null)
            instance = new GameObject("NoticeUI").AddComponent<NoticeUI>();

        bool isKor = Localization.Current == Language.Korean;
        float d = duration < 0f ? instance.showDuration : duration;

        instance.Display(isKor ? ko : en, d);
    }

    void Awake()
    {
        // 작업장 미리보기로 같이 불러온 씬의 오브젝트가 덮어쓰지 않게 함
        if (instance == null || gameObject.scene == SceneManager.GetActiveScene())
            instance = this;

        Build();
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    void Build()
    {
        var canvasGo = new GameObject(
            "NoticeCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
        canvasGo.transform.SetParent(transform, false);

        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 350;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        group = canvasGo.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        var txt = new GameObject("Text", typeof(RectTransform));
        txt.transform.SetParent(canvasGo.transform, false);

        textRect = txt.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(1f, 0f);
        textRect.anchorMax = new Vector2(1f, 0f);
        textRect.pivot = new Vector2(1f, 0f);

        label = txt.AddComponent<TextMeshProUGUI>();
        label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;

        ApplyStyle();
    }

    // Inspector 값을 화면에 적용 (실행 중에 값을 바꾸면 바로 반영됨)
    void ApplyStyle()
    {
        if (label == null) return;

        TMP_FontAsset f = fontAsset;
        if (f == null)
        {
            if (autoFont == null) autoFont = FindKoreanFont();
            f = autoFont;
        }
        if (f != null) label.font = f;

        label.fontSize = fontSize;
        label.color = textColor;
        label.alignment = alignment;

        label.outlineColor = (Color32)outlineColor;
        label.outlineWidth = outlineWidth;

        textRect.anchoredPosition = new Vector2(-margin.x, margin.y);
        textRect.sizeDelta = boxSize;
    }

    void OnValidate()
    {
        ApplyStyle();
    }

    // 항상 같은 한글 폰트를 고르기
    TMP_FontAsset FindKoreanFont()
    {
        // Resources 폴더에 NoticeFont라는 이름의 TMP 폰트를 넣어 두면 그걸 우선 사용
        TMP_FontAsset res = Resources.Load<TMP_FontAsset>("NoticeFont");
        if (res != null) return res;

        // 게임에서 쓰는 맑은 고딕 폰트를 이름으로 찾기
        foreach (var fa in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
        {
            if (fa != null && fa.name.ToLower().Contains("malgun"))
                return fa;
        }

        // 못 찾으면 한글이 들어 있는 아무 폰트
        TMP_Text[] all = FindObjectsByType<TMP_Text>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var t in all)
        {
            if (t == null || t.font == null) continue;
            if (t.font.HasCharacter('가', true, true))
                return t.font;
        }

        return TMP_Settings.defaultFontAsset;
    }

    void Update()
    {
        if (label == null) return;

        if (keepShowing)
        {
            bool isKor = Localization.Current == Language.Korean;
            label.text = isKor ? testKorean : testEnglish;
            group.alpha = 1f;
        }
        else if (wasKeepShowing)
        {
            group.alpha = 0f;
        }

        wasKeepShowing = keepShowing;
    }

    void Display(string message, float duration)
    {
        label.text = message;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(duration));
    }

    IEnumerator ShowRoutine(float duration)
    {
        float fadeIn = Mathf.Max(0.01f, fadeInTime);
        float fadeOut = Mathf.Max(0.01f, fadeOutTime);

        // 이미 떠 있으면 그 밝기에서 이어서 시작 (연타해도 깜빡이지 않게)
        float t = group.alpha * fadeIn;

        while (t < fadeIn)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Clamp01(t / fadeIn);
            yield return null;
        }
        group.alpha = 1f;

        yield return new WaitForSecondsRealtime(duration);

        t = 0f;
        while (t < fadeOut)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = 1f - Mathf.Clamp01(t / fadeOut);
            yield return null;
        }
        group.alpha = 0f;
        routine = null;
    }
}