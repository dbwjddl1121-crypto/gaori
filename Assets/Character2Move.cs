using UnityEngine;

public class Character2Move : MonoBehaviour
{
    public float moveSpeed = 4f;

    // 점프 설정
    public float jumpHeight = 1f;
    public float jumpDuration = 0.5f;

    private Animator animator;
    private bool isJumping = false;
    private float jumpTimer = 0f;
    private Vector3 startPosition;

    void Start()
    {
        animator = GetComponent<Animator>();
        startPosition = transform.position;
    }

    void Update()
    {
        // 오른쪽 이동
        float moveX = Input.GetAxisRaw("Horizontal");

        // 왼쪽 이동 막기
        if (moveX < 0)
        {
            moveX = 0;
        }

        // 점프 중이 아닐 때만 이동
        if (!isJumping)
        {
            transform.position += Vector3.right * moveX * moveSpeed * Time.deltaTime;
        }

        // 걷기 애니메이션
        bool isMoving = moveX > 0 && !isJumping;
        animator.SetBool("IsMoving", isMoving);

        // Space → 점프 + 놀람
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            isJumping = true;
            jumpTimer = 0f;

            animator.SetBool("Jump", true);
        }

        // 점프 진행
        if (isJumping)
        {
            jumpTimer += Time.deltaTime;

            float progress = jumpTimer / jumpDuration;

            // 위로 올라갔다가 내려오는 움직임
            float height = Mathf.Sin(progress * Mathf.PI) * jumpHeight;

            transform.position = new Vector3(
                transform.position.x,
                startPosition.y + height,
                transform.position.z
            );

            // 점프 끝
            if (progress >= 1f)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    startPosition.y,
                    transform.position.z
                );

                isJumping = false;
                animator.SetBool("Jump", false);
            }
        }
    }
}