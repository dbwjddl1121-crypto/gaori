using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    // 현재 상호작용 가능한 대상
    private GameObject currentTarget;

    public void SetTarget(GameObject target)
    {
        currentTarget = target;
    }

    public void ClearTarget(GameObject target)
    {
        if (currentTarget == target)
        {
            currentTarget = null;
        }
    }

    public GameObject GetCurrentTarget()
    {
        return currentTarget;
    }
}