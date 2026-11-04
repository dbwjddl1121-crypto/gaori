using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CanMiniGame : MonoBehaviour, IPointerClickHandler
{
    // 단계 하나 = 이미지 + 그 이미지의 크기
    [Serializable]
    public class Stage
    {
        public string label;        // 구분용 이름 (예: 온전함)
        public Sprite sprite;       // 이미지
        public Vector2 size = new Vector2(300, 300);  // 가로, 세로 (0, 0이면 원본 크기)
    }

    [Header("캔 단계 (순서대로: 온전함, 밟는 과정1, 밟는 과정2, 완전 찌그러짐)")]
    public Stage[] stages;

    [Header("완료 효과")]
    public UIBurstEffect burst;        // 팡 터지는 효과
    public float finishPunch = 1.25f;  // 완성 순간 커지는 배율

    [Header("설정")]
    public int clicksPerStage = 3;     // 단계가 바뀌는 데 필요한 클릭 수
    public float finishDelay = 1.0f;   // 완성 후 창이 닫히기까지 대기 시간

    private Image canImage;
    private RectTransform rect;
    private int clickCount;
    private bool finished;
    private Vector3 baseScale;

    private void Awake()
    {
        canImage = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        baseScale = transform.localScale;
    }

    // 미니게임 창이 켜질 때마다 처음 상태로 초기화
    private void OnEnable()
    {
        clickCount = 0;
        finished = false;
        transform.localScale = baseScale;

        if (stages != null && stages.Length > 0)
            ApplyStage(0);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (finished) return;
        if (stages == null || stages.Length == 0) return;

        clickCount++;

        // 현재 단계 계산
        int lastStage = stages.Length - 1;
        int stage = Mathf.Min(clickCount / clicksPerStage, lastStage);
        ApplyStage(stage);

        if (stage == lastStage)
        {
            // 마지막 이미지가 되면 완료: 크게 커지고 팡 터짐
            finished = true;
            transform.localScale = baseScale * finishPunch;

            if (burst != null)
                burst.Play();

            StartCoroutine(FinishRoutine());
        }
        else
        {
            // 평소 클릭은 살짝 눌리는 느낌
            transform.localScale = baseScale * 0.9f;
        }
    }

    // 단계의 이미지와 크기를 적용
    private void ApplyStage(int index)
    {
        Stage s = stages[index];
        if (s == null) return;

        canImage.sprite = s.sprite;

        if (s.size == Vector2.zero)
            canImage.SetNativeSize();          // 크기를 0, 0으로 두면 이미지 원본 크기
        else
            rect.sizeDelta = s.size;
    }

    private void Update()
    {
        // 바뀐 크기를 원래 크기로 부드럽게 복구
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            baseScale,
            Time.unscaledDeltaTime * 12f
        );
    }

    private IEnumerator FinishRoutine()
    {
        yield return new WaitForSecondsRealtime(finishDelay);

        if (MiniGameManager.Instance != null)
            MiniGameManager.Instance.EndMiniGame(true);
    }
}