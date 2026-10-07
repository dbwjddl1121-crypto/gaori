using UnityEngine;

public class PlayerEnterWorkshop : MonoBehaviour
{
    public float entrySpeed = 3f;
    public float targetY = 0f;

    private PlayerMove playerMove;
    private Animator animator;

    private bool isEntering = true;

    void Start()
    {
        playerMove = GetComponent<PlayerMove>();
        animator = GetComponent<Animator>();

        // 자동으로 들어오는 동안 플레이어 조작 막기
        if (playerMove != null)
        {
            playerMove.enabled = false;
        }

        // 위쪽으로 걷는 애니메이션
        if (animator != null)
        {
            animator.SetFloat("MoveX", 0f);
            animator.SetFloat("MoveY", 1f);
            animator.SetBool("IsMoving", true);
        }
    }

    void Update()
    {
        if (!isEntering)
            return;

        // 아래에서 위로 이동
        transform.position += Vector3.up * entrySpeed * Time.deltaTime;

        // 목표 위치에 도착
        if (transform.position.y >= targetY)
        {
            transform.position = new Vector3(
                transform.position.x,
                targetY,
                transform.position.z
            );

            isEntering = false;

            // 걷기 정지
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
            }

            // 직접 조작 가능
            if (playerMove != null)
            {
                playerMove.enabled = true;
            }
        }
    }
}