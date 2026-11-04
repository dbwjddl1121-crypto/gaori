using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GlassMiniGame : MonoBehaviour
{
    // 단계 하나 = 이미지 + 그 이미지의 크기
    [Serializable]
    public class Stage
    {
        public string label;        // 구분용 이름 (예: 펼친 신문지)
        public Sprite sprite;       // 이미지
        public Vector2 size = new Vector2(400, 400);  // 가로, 세로 (0, 0이면 원본 크기)
    }

    [Header("신문지")]
    public Image newspaperImage;

    [Header("신문지 단계 (순서대로: 펼친 신문지, 구기는 중1, 구기는 중2, 완전히 구겨짐)")]
    public Stage[] stages;

    [Header("안내 문구")]
    public string collectGuide = "유리 조각을 신문지 위로 옮겨 주세요!";
    public string crumpleGuide = "신문지를 연타해서 구겨 주세요!";

    [Header("난이도")]
    public int clicksPerStage = 4;     // 단계가 바뀌는 데 필요한 클릭 수. 클수록 어려움

    [Header("조각이 빨려 들어가는 효과")]
    public float absorbWait = 0.4f;      // 마지막 조각을 놓은 뒤 빨려 들어가기까지 대기 시간
    public float absorbDuration = 0.35f; // 빨려 들어가는 데 걸리는 시간

    [Header("완료 효과")]
    public UIBurstEffect burst;        // 팡 터지는 효과
    public float finishPunch = 1.25f;  // 완성 순간 커지는 배율
    public float finishDelay = 1.0f;   // 완성 후 창이 닫히기까지 대기 시간

    private RectTransform newspaperRect;
    private Vector3 baseScale;
    private GlassPiece[] pieces;

    private int collectedCount;
    private int clickCount;
    private bool crumpling;     // 신문지 구기는 단계인지
    private bool finished;
    private bool collectDone;   // 조각을 다 모은 상태 (구기기 시작 전 대기 포함)

    // 조각을 끌 수 있는 상태인지 (옮기는 단계에서만)
    public bool CanDrag
    {
        get { return !collectDone && !crumpling && !finished; }
    }

    private void Awake()
    {
        if (newspaperImage != null)
        {
            newspaperRect = newspaperImage.rectTransform;
            baseScale = newspaperRect.localScale;
        }
    }

    // 미니게임 창이 켜질 때마다 처음 상태로 초기화
    private void OnEnable()
    {
        collectedCount = 0;
        clickCount = 0;
        crumpling = false;
        finished = false;
        collectDone = false;

        if (newspaperRect != null)
            newspaperRect.localScale = baseScale;

        // 자식 중 유리조각을 모두 찾아서 처음 자리로 되돌림 (개수는 자유)
        pieces = GetComponentsInChildren<GlassPiece>(true);
        foreach (GlassPiece p in pieces)
            p.ResetPiece();

        if (stages != null && stages.Length > 0)
            ApplyStage(0);

        if (MiniGameManager.Instance != null)
            MiniGameManager.Instance.SetGuide(collectGuide);

        // 조각이 하나도 없으면 바로 구기는 단계
        if (pieces.Length == 0)
        {
            collectDone = true;
            StartCoroutine(StartCrumpleRoutine());
        }
    }

    // 조각을 놓았을 때 호출됨. 신문지 위면 true
    public bool TryDrop(Vector2 screenPos, Camera cam)
    {
        if (!CanDrag || newspaperRect == null) return false;

        if (!RectTransformUtility.RectangleContainsScreenPoint(newspaperRect, screenPos, cam))
            return false;

        collectedCount++;
        newspaperRect.localScale = baseScale * 1.08f;   // 조각이 들어갈 때 살짝 출렁임

        // 모두 모았으면 잠깐 뒤에 조각이 빨려 들어가고 구기는 단계 시작
        if (collectedCount >= pieces.Length)
        {
            collectDone = true;
            StartCoroutine(StartCrumpleRoutine());
        }

        return true;
    }

    private IEnumerator StartCrumpleRoutine()
    {
        // 마지막 조각이 신문지 위에 놓인 모습을 잠깐 보여줌
        yield return new WaitForSecondsRealtime(absorbWait);

        // 조각들이 신문지 가운데로 빨려 들어감
        if (newspaperRect != null)
        {
            Vector3 target = newspaperRect.localPosition;
            foreach (GlassPiece p in pieces)
            {
                if (p != null && p.gameObject.activeSelf)
                    p.Absorb(target, absorbDuration);
            }
        }

        yield return new WaitForSecondsRealtime(absorbDuration);

        if (newspaperRect != null)
            newspaperRect.localScale = baseScale * 1.15f;   // 다 들어간 순간 살짝 출렁임

        crumpling = true;
        clickCount = 0;

        if (MiniGameManager.Instance != null)
            MiniGameManager.Instance.SetGuide(crumpleGuide);
    }

    private void Update()
    {
        if (newspaperRect == null) return;

        // 바뀐 크기를 원래 크기로 부드럽게 복구
        newspaperRect.localScale = Vector3.Lerp(
            newspaperRect.localScale,
            baseScale,
            Time.unscaledDeltaTime * 12f
        );

        // 구기는 단계: 신문지 위를 클릭할 때마다 진행
        if (crumpling && !finished && Input.GetMouseButtonDown(0))
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(newspaperRect, Input.mousePosition, null))
                Crumple();
        }
    }

    private void Crumple()
    {
        if (stages == null || stages.Length == 0) return;

        clickCount++;

        int lastStage = stages.Length - 1;
        int stage = Mathf.Min(clickCount / Mathf.Max(1, clicksPerStage), lastStage);
        ApplyStage(stage);

        if (stage == lastStage)
        {
            // 마지막 이미지가 되면 완료: 크게 커지고 팡 터짐
            finished = true;
            newspaperRect.localScale = baseScale * finishPunch;

            if (burst != null)
                burst.Play();

            StartCoroutine(FinishRoutine());
        }
        else
        {
            // 평소 클릭은 살짝 눌리는 느낌
            newspaperRect.localScale = baseScale * 0.9f;
        }
    }

    // 단계의 이미지와 크기를 적용
    private void ApplyStage(int index)
    {
        if (newspaperImage == null) return;
        if (index < 0 || index >= stages.Length) return;

        Stage s = stages[index];
        if (s == null) return;

        newspaperImage.sprite = s.sprite;

        if (s.size == Vector2.zero)
            newspaperImage.SetNativeSize();     // 크기를 0, 0으로 두면 이미지 원본 크기
        else
            newspaperRect.sizeDelta = s.size;
    }

    private IEnumerator FinishRoutine()
    {
        yield return new WaitForSecondsRealtime(finishDelay);

        if (MiniGameManager.Instance != null)
            MiniGameManager.Instance.EndMiniGame(true);
    }
}