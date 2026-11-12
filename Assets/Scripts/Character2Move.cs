using UnityEngine;

public class Character2Move : MonoBehaviour
{
    public float moveSpeed = 4f;

    // 점프 설정
    public float jumpHeight = 1f;
    public float jumpDuration = 0.5f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isJumping = false;
    private float jumpTimer = 0f;
    private Vector3 startPosition;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        startPosition = transform.position;

        // 처음에는 오른쪽을 바라봄
        spriteRenderer.flipX = false;
    }

    void Update()
    {
        // 좌우 이동
        float moveX = Input.GetAxisRaw("Horizontal");

        // 점프 중이 아닐 때만 이동
        if (!isJumping)
        {
            transform.position += Vector3.right * moveX * moveSpeed * Time.deltaTime;
        }

        // 이동 방향에 따라 좌우 반전
        if (moveX > 0)
        {
            // 오른쪽
            spriteRenderer.flipX = false;
        }
        else if (moveX < 0)
        {
            // 왼쪽
            spriteRenderer.flipX = true;
        }

        // 걷기 애니메이션
        bool isMoving = moveX != 0 && !isJumping;
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