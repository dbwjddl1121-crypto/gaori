
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LabelDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Vector2 startPosition;
    private bool isCompleted = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isCompleted) return;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isCompleted) return;

        rectTransform.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isCompleted) return;

        // 시작 위치보다 충분히 아래로 내려갔는지 확인
        if (rectTransform.anchoredPosition.y < startPosition.y - 150f)
        {
            isCompleted = true;
            Debug.Log("생수병 라벨 제거 성공!");
        }
        else
        {
            // 충분히 내리지 못했다면 원래 위치로 돌아가기
            rectTransform.anchoredPosition = startPosition;
        }
    }
}
