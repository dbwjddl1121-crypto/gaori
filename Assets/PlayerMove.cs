using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 7f;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(moveX, moveY, 0f).normalized;
        transform.position += movement * moveSpeed * Time.deltaTime;

        animator.SetFloat("MoveX", moveX);
        animator.SetFloat("MoveY", moveY);

        // 움직이고 있는지 확인
        bool isMoving = moveX != 0 || moveY != 0;
        animator.SetBool("IsMoving", isMoving);

        // 스페이스바를 누르면 점프
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetBool("Jump", true);

            Invoke(nameof(EndJump), 2f);
        }

        // Shift를 누르고 있는 동안 울기
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            animator.SetBool("Cry", true);
        }
        else
        {
            animator.SetBool("Cry", false);
        }
    }

    void EndJump()
    {
        animator.SetBool("Jump", false);
    }
}