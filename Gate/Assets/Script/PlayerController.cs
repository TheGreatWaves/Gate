using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float moveSpeedMultiplier = 10f;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private Animator animator;

    [SerializeField] float runSpeed = 2.0f;
    [SerializeField] float jumpSpeed = 4.0f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Run();
        FlipSprite();
    }

    void Run() 
    {
        Vector2 playerVelocity = new Vector2 (moveInput.x*moveSpeed, rb.velocity.y);
        rb.velocity = playerVelocity;

        bool playerHasHorizontalSpeed = Mathf.Abs(rb.velocity.x) > Mathf.Epsilon;

        if (playerHasHorizontalSpeed)
        {
            animator.SetBool("isRunning", true);
        }
        else 
        {
            animator.SetBool("isRunning", false);
        }
    }

    void FlipSprite()
    {
        Debug.Log(rb.velocity.x);
        bool playerHasHorizontalSpeed = Mathf.Abs(rb.velocity.x) > Mathf.Epsilon;
        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(rb.velocity.x), 1f);
        }
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            rb.velocity += new Vector2(0f, jumpSpeed);
        }
    }
}
