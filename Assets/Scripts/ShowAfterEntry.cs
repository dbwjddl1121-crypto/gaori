using UnityEngine;

// 입장 연출(PlayerMove가 꺼져 있는 동안)에는 숨기고, 끝나면 아래에서 올라오며 나타남
[RequireComponent(typeof(CanvasGroup))]
public class ShowAfterEntry : MonoBehaviour
{
    public PlayerMove playerMove;       // Player의 PlayerMove

    [Header("연출 설정")]
    public float duration = 0.8f;       // 나타나는 데 걸리는 시간(초)
    public float slideDistance = 80f;   // 아래에서 올라오는 거리

    private CanvasGroup group;
    private RectTransform rect;
    private Vector2 targetPos;          // 최종 위치 (원래 배치한 자리)
    private float timer = 0f;
    private bool started = false;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();

        targetPos = rect.anchoredPosition;

        // 시작할 때는 숨기고, 아래로 내려둠
        group.alpha = 0f;
        group.blocksRaycasts = false;
        rect.anchoredPosition = targetPos + Vector2.down * slideDistance;
    }

    void Update()
    {
        if (playerMove == null) return;

        // 입장 연출이 끝나면 시작
        if (!started && playerMove.enabled)
            started = true;

        if (!started) return;

        if (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            // 처음엔 빠르고 끝에서 부드럽게 멈추는 곡선
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            group.alpha = eased;
            rect.anchoredPosition = Vector2.Lerp(
                targetPos + Vector2.down * slideDistance,
                targetPos,
                eased
            );

            if (t >= 1f) group.blocksRaycasts = true;
        }
    }
}