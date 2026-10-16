using UnityEngine;

// target 오브젝트가 켜져 있을 때만 이 오브젝트(의 자식들)를 보여줌
public class FollowActive : MonoBehaviour
{
    public GameObject target;   // WorkshopInteriorPanel

    void Update()
    {
        if (target == null) return;

        bool on = target.activeInHierarchy;
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf != on)
                child.gameObject.SetActive(on);
        }
    }
}