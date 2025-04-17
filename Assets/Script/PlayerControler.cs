using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    public Animator animator;

    private Vector2 moveInput;
    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Di chuyển trái/phải
        float move = Input.GetAxisRaw("Horizontal");
        moveInput = new Vector2(move, 0);
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

        // Set biến chạy
        animator.SetBool("isRunning", move != 0);

        // Tấn công
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetBool("isAttacking", true);
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            animator.SetBool("isAttacking", false);
        }
    }
}