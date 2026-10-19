using UnityEngine;

// PlayerMove를 건드리지 않고, 발 영역이 벽에 닿으면 이동을 되돌림
public class PlayerWallBlock : MonoBehaviour
{
    [Header("연결")]
    public BoxCollider2D footCollider;   // Player 아래의 FootCollider
    public PlayerMove playerMove;        // 입장 연출 중에는 검사하지 않기 위해 사용
    public LayerMask wallLayer;          // Wall 레이어

    private Animator animator;
    private Vector3 lastPos;

    void Start()
    {
        animator = GetComponent<Animator>();
        lastPos = transform.position;
    }

    void LateUpdate()
    {
        // 입장 연출 중(PlayerMove가 꺼져 있음)에는 그냥 통과
        if (playerMove != null && !playerMove.enabled)
        {
            lastPos = transform.position;
            return;
        }

        // 점프 중에는 검사하지 않음 (점프 중엔 좌우 이동이 없음)
        if (animator != null && animator.GetBool("Jump"))
        {
            lastPos = transform.position;
            return;
        }

        Vector3 newPos = transform.position;

        if (IsBlocked(newPos))
        {
            // 벽을 따라 미끄러지도록 가로만, 세로만 따로 시도
            Vector3 tryX = new Vector3(newPos.x, lastPos.y, newPos.z);
            Vector3 tryY = new Vector3(lastPos.x, newPos.y, newPos.z);

            if (!IsBlocked(tryX)) newPos = tryX;
            else if (!IsBlocked(tryY)) newPos = tryY;
            else newPos = lastPos;

            transform.position = newPos;
        }

        lastPos = transform.position;
    }

    // pos 위치에 발 영역을 놓았을 때 벽과 겹치는지 검사
    bool IsBlocked(Vector3 pos)
    {
        if (footCollider == null) return false;

        Transform ft = footCollider.transform;
        Vector2 center = (Vector2)ft.TransformPoint(footCollider.offset)
                         + (Vector2)(pos - transform.position);
        Vector2 scale = ft.lossyScale;
        Vector2 size = new Vector2(
            footCollider.size.x * Mathf.Abs(scale.x),
            footCollider.size.y * Mathf.Abs(scale.y));

        return Physics2D.OverlapBox(center, size, 0f, wallLayer) != null;
    }
}