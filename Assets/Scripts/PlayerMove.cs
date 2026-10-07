using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 7f;

    // 실제 점프 설정
    public float jumpHeight = 0.5f;
    public float jumpDuration = 0.6f;

    private Animator animator;

    private bool isJumping = false;
    private float jumpTimer = 0f;
    private float jumpStartY;

    // Ctrl 상태 확인
    private bool wasCtrlPressed = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // 이동
        if (!isJumping)
        {
            Vector3 movement = new Vector3(moveX, moveY, 0f).normalized;
            transform.position += movement * moveSpeed * Time.deltaTime;
        }

        animator.SetFloat("MoveX", moveX);
        animator.SetFloat("MoveY", moveY);

        // 움직이고 있는지 확인
        bool isMoving = moveX != 0 || moveY != 0;
        animator.SetBool("IsMoving", isMoving);

        // 스페이스바를 누르면 점프
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            isJumping = true;
            jumpTimer = 0f;
            jumpStartY = transform.position.y;

            animator.SetBool("Jump", true);
        }

        // 실제 캐릭터 점프
        if (isJumping)
        {
            jumpTimer += Time.deltaTime;

            float progress = jumpTimer / 2f;

            // 위로 올라갔다가 내려오는 움직임
            float height = Mathf.Sin(progress * Mathf.PI) * jumpHeight;

            transform.position = new Vector3(
                transform.position.x,
                jumpStartY + height,
                transform.position.z
            );

            // 2초가 지나면 점프 종료
            if (jumpTimer >= 2f)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    jumpStartY,
                    transform.position.z
                );

                isJumping = false;
                animator.SetBool("Jump", false);
            }
        }

        // =========================
        // Ctrl → 뒷모습 서 있기
        // =========================

        bool ctrlPressed =
            Input.GetKey(KeyCode.LeftControl) ||
            Input.GetKey(KeyCode.RightControl);

        // Ctrl을 새로 눌렀을 때
        if (ctrlPressed && !wasCtrlPressed)
        {
            animator.SetBool("IsCtrl", true);
            animator.Play("Player_Idle_Back");
        }

        // Ctrl을 뗐을 때
        if (!ctrlPressed && wasCtrlPressed)
        {
            animator.SetBool("IsCtrl", false);
            animator.Play("Player_Idle");
        }

        wasCtrlPressed = ctrlPressed;

        // =========================
        // Shift → 울기
        // =========================

        if (Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift))
        {
            animator.SetBool("Cry", true);
        }
        else
        {
            animator.SetBool("Cry", false);
        }
    }
}