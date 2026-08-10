using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 4f;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(
            horizontal,
            vertical,
            0f
        ).normalized;

        Vector3 newPosition =
            transform.position +
            movement *
            moveSpeed *
            Time.deltaTime;

        transform.position = newPosition;

        bool isMoving = movement.magnitude > 0.01f;

        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
        }

        if (horizontal != 0)
        {
            Vector3 scale = transform.localScale;

            scale.x =
                Mathf.Abs(scale.x) *
                Mathf.Sign(horizontal);

            transform.localScale = scale;
        }
    }
}