using UnityEngine;

// target 오브젝트가 켜져 있을 때만 이 오브젝트(의 자식들)를 보여줌
// 자식을 끄지 않고 투명도/클릭만 제어해서 다른 스크립트와 충돌하지 않음
[RequireComponent(typeof(CanvasGroup))]
public class FollowActive : MonoBehaviour
{
    public GameObject target;   // WorkshopInteriorPanel

    private CanvasGroup group;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (target == null) return;

        bool on = target.activeInHierarchy;
        group.alpha = on ? 1f : 0f;
        group.interactable = on;
        group.blocksRaycasts = on;
    }
}