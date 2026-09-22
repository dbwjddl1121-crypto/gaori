using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 4f;

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
    }
}