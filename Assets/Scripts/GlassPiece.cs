using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class GlassPiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rect;
    private RectTransform parentRect;
    private GlassMiniGame game;

    private Vector3 startPos;
    private Vector3 baseScale;
    private Vector2 grabOffset;
    private bool inited;
    private bool dragging;
    private bool returning;
    private bool placed;     // 신문지 위에 놓인 상태

    // 처음 한 번만 시작 위치와 크기를 기억함
    private void Init()
    {
        if (inited) return;

        rect = GetComponent<RectTransform>();
        parentRect = rect.parent as RectTransform;
        game = GetComponentInParent<GlassMiniGame>();
        startPos = rect.localPosition;
        baseScale = rect.localScale;
        inited = true;
    }

    // 미니게임이 켜질 때마다 처음 자리로 되돌리고 다시 보이게 함
    public void ResetPiece()
    {
        Init();

        StopAllCoroutines();
        dragging = false;
        returning = false;
        placed = false;

        rect.localPosition = startPos;
        rect.localScale = baseScale;
        gameObject.SetActive(true);
    }

    // 모든 조각이 모였을 때 신문지 가운데로 빨려 들어가며 사라짐
    public void Absorb(Vector3 targetLocalPos, float duration)
    {
        Init();
        StartCoroutine(AbsorbRoutine(targetLocalPos, duration));
    }

    private IEnumerator AbsorbRoutine(Vector3 targetLocalPos, float duration)
    {
        Vector3 from = rect.localPosition;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.01f, duration);
            float p = Mathf.Clamp01(t);

            rect.localPosition = Vector3.Lerp(from, targetLocalPos, p);
            rect.localScale = baseScale * (1f - p);

            yield return null;
        }

        gameObject.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Init();

        if (placed) return;
        if (game == null || !game.CanDrag) return;

        Vector2 local;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, eventData.position, eventData.pressEventCamera, out local))
        {
            grabOffset = (Vector2)rect.localPosition - local;
        }

        dragging = true;
        returning = false;
        rect.SetAsLastSibling();   // 드래그 중에는 맨 위에 보이게
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging) return;

        Vector2 local;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, eventData.position, eventData.pressEventCamera, out local))
        {
            rect.localPosition = local + grabOffset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        dragging = false;

        // 신문지 위에 놓았으면 그 자리에 남고, 아니면 원래 자리로 돌아감
        if (game != null && game.TryDrop(eventData.position, eventData.pressEventCamera))
            placed = true;
        else
            returning = true;
    }

    private void Update()
    {
        if (!returning) return;

        rect.localPosition = Vector3.Lerp(
            rect.localPosition,
            startPos,
            Time.unscaledDeltaTime * 14f
        );

        if ((rect.localPosition - startPos).sqrMagnitude < 1f)
        {
            rect.localPosition = startPos;
            returning = false;
        }
    }
}