using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BottleMiniGame : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    // 단계 하나 = 이미지 + 그 이미지의 크기
    [Serializable]
    public class Stage
    {
        public string label;        // 구분용 이름 (예: 라벨 붙음)
        public Sprite sprite;       // 이미지
        public Vector2 size = new Vector2(300, 400);  // 가로, 세로 (0, 0이면 원본 크기)
    }

    [Header("병 단계 (순서대로: 라벨 붙음, 떼는 중1, 떼는 중2, 완전히 떨어짐)")]
    public Stage[] stages;

    [Header("진행 게이지 (선택)")]
    public Image progressFill;           // GaugeFill (높이가 진행도에 맞춰 위에서 아래로 차오름)

    [Header("난이도")]
    public float peelDistance = 900f;    // 끝까지 떼는 데 필요한 아래 방향 드래그 거리(픽셀). 클수록 어려움
    public float tearSpeed = 3500f;      // 이 속도(픽셀/초)보다 빨리 당기면 찢어짐. 작을수록 어려움
    public float tearPenalty = 0.1f;     // 찢어질 때 깎이는 진행도 (0~1)
    public bool resetOnRelease = true;   // 마우스를 놓으면 현재 단계 시작점으로 되돌아감

    [Header("찢어짐 효과")]
    public Color tearColor = new Color(1f, 0.4f, 0.4f);
    public float shakeTime = 0.25f;
    public float shakePower = 10f;

    [Header("완료 효과")]
    public UIBurstEffect burst;          // 팡 터지는 효과
    public float finishPunch = 1.25f;    // 완성 순간 커지는 배율
    public float finishDelay = 1.0f;     // 완성 후 창이 닫히기까지 대기 시간

    private Image bottleImage;
    private RectTransform rect;
    private Vector2 basePos;
    private Vector3 baseScale;

    private float progress;      // 0 ~ 1
    private bool finished;
    private float shakeTimer;
    private float smoothSpeed;   // 여러 프레임 평균 속도

    private void Awake()
    {
        bottleImage = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        basePos = rect.anchoredPosition;
        baseScale = transform.localScale;
    }

    // 미니게임 창이 켜질 때마다 처음 상태로 초기화
    private void OnEnable()
    {
        progress = 0f;
        finished = false;
        shakeTimer = 0f;
        smoothSpeed = 0f;

        transform.localScale = baseScale;
        rect.anchoredPosition = basePos;

        if (bottleImage != null)
            bottleImage.color = Color.white;

        if (stages != null && stages.Length > 0)
            ApplyStage(0);

        UpdateGauge();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (finished) return;

        smoothSpeed = 0f;
        transform.localScale = baseScale * 0.95f;   // 누르는 순간 살짝 눌리는 느낌
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (finished) return;
        if (stages == null || stages.Length == 0) return;

        // 속도를 여러 프레임에 걸쳐 평균내서 튀는 값 방지
        float dt = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
        float instantSpeed = eventData.delta.magnitude / dt;
        smoothSpeed = Mathf.Lerp(smoothSpeed, instantSpeed, 0.15f);

        // 너무 빠르면 라벨이 찢어짐
        if (smoothSpeed > tearSpeed)
        {
            progress = Mathf.Max(0f, progress - tearPenalty);
            shakeTimer = shakeTime;
            smoothSpeed = 0f;
            RefreshStage();
            UpdateGauge();
            return;
        }

        // 아래로 당긴 만큼만 진행 (위, 옆으로는 진행 안 됨)
        float down = Mathf.Max(0f, -eventData.delta.y);
        progress = Mathf.Clamp01(progress + down / Mathf.Max(1f, peelDistance));

        RefreshStage();
        UpdateGauge();

        if (progress >= 1f)
            Finish();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (finished) return;

        smoothSpeed = 0f;

        // 마우스를 놓으면 현재 단계의 시작점으로 되돌아감
        if (resetOnRelease)
        {
            int n = stages.Length;
            if (n > 1)
            {
                int stage = GetStageIndex();
                progress = stage / (float)(n - 1);
            }
            else
            {
                progress = 0f;
            }

            RefreshStage();
            UpdateGauge();
        }
    }

    // 진행도에 해당하는 단계 번호
    private int GetStageIndex()
    {
        int n = stages.Length;
        if (n <= 1) return 0;
        return Mathf.Min(Mathf.FloorToInt(progress * (n - 1)), n - 1);
    }

    private void RefreshStage()
    {
        ApplyStage(GetStageIndex());
    }

    // 게이지: 막대의 높이를 조절해서 위에서 아래로 차오르게 함 (이미지 필요 없음)
    private void UpdateGauge()
    {
        if (progressFill == null) return;

        RectTransform fr = progressFill.rectTransform;
        fr.anchorMin = new Vector2(0f, 1f - progress);
        fr.anchorMax = new Vector2(1f, 1f);
        fr.offsetMin = Vector2.zero;
        fr.offsetMax = Vector2.zero;
    }

    // 단계의 이미지와 크기를 적용
    private void ApplyStage(int index)
    {
        if (index < 0 || index >= stages.Length) return;

        Stage s = stages[index];
        if (s == null) return;

        bottleImage.sprite = s.sprite;

        if (s.size == Vector2.zero)
            bottleImage.SetNativeSize();     // 크기를 0, 0으로 두면 이미지 원본 크기
        else
            rect.sizeDelta = s.size;
    }

    private void Finish()
    {
        finished = true;
        progress = 1f;
        ApplyStage(stages.Length - 1);
        UpdateGauge();

        // 크게 커지고 팡 터짐
        transform.localScale = baseScale * finishPunch;
        bottleImage.color = Color.white;
        rect.anchoredPosition = basePos;

        if (burst != null)
            burst.Play();

        StartCoroutine(FinishRoutine());
    }

    private void Update()
    {
        // 바뀐 크기를 원래 크기로 부드럽게 복구
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            baseScale,
            Time.unscaledDeltaTime * 12f
        );

        if (finished) return;

        // 찢어짐 효과: 붉게 변하며 흔들림
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(shakeTimer / Mathf.Max(0.01f, shakeTime));

            bottleImage.color = Color.Lerp(Color.white, tearColor, p);
            rect.anchoredPosition = basePos + UnityEngine.Random.insideUnitCircle * shakePower * p;
        }
        else
        {
            bottleImage.color = Color.white;
            rect.anchoredPosition = basePos;
        }
    }

    private IEnumerator FinishRoutine()
    {
        yield return new WaitForSecondsRealtime(finishDelay);

        if (MiniGameManager.Instance != null)
            MiniGameManager.Instance.EndMiniGame(true);
    }
}