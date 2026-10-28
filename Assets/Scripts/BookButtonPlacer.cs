using UnityEngine;

// 작업장 패널이 켜져 있으면 도감 버튼을 다른 자리로 옮김
[RequireComponent(typeof(RectTransform))]
public class BookButtonPlacer : MonoBehaviour
{
    [Header("연결")]
    public GameObject workshopPanel;      // WorkshopInteriorPanel

    [Header("작업장에서의 위치")]
    public Vector2 workshopPos = new Vector2(-70f, 200f);   // 작업장에서 버튼이 놓일 자리

    private RectTransform rect;
    private Vector2 normalPos;            // 바다 화면에서의 위치 (시작할 때의 자리를 기억)
    private bool lastInWorkshop = false;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        normalPos = rect.anchoredPosition;
        Debug.Log("[BookButtonPlacer] Awake - normalPos 저장됨: " + normalPos);
    }

    void Update()
    {
        if (workshopPanel == null) return;

        bool inWorkshop = workshopPanel.activeInHierarchy;
        if (inWorkshop == lastInWorkshop) return;   // 화면이 바뀔 때만 위치를 바꿈

        lastInWorkshop = inWorkshop;
        rect.anchoredPosition = inWorkshop ? workshopPos : normalPos;
        Debug.Log("[BookButtonPlacer] 위치 변경 -> inWorkshop=" + inWorkshop + ", 적용된 좌표=" + rect.anchoredPosition);
    }
}
