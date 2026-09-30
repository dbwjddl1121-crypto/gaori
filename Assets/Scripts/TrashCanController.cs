using UnityEngine;

public class TrashCanController : MonoBehaviour
{
    void OnMouseDown()
    {
        // 클릭이 감지되었는지 확인하기 위한 디버그 로그
        Debug.Log("쓰레기통 클릭 성공!");

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("IsOpen");
        }
        else
        {
            Debug.Log("하지만 Animator 컴포넌트가 없습니다!");
        }
    }
}