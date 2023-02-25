using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float MoveSpeed = 10f;
    public float MoveSpeedMultiplier = 10f;
    private Vector2 _moveInput;
    private Rigidbody2D _rb;
    [SerializeField] private Vector2 _movement;
    
    private Animator _animator;

    [SerializeField] private float _runSpeed = 2.0f;
    [SerializeField] private float _jumpSpeed = 8.0f;
    private bool _isJumpCut;
    public float GravityScale = 2.0f;
    public float FastFallGravityMult = 3.0f;
    public float FallGravityMult = 2.5f;
    public float JumpCutGravityMult = 2.5f;
    public float MaxFastFallSpeed = 10.0f;
    public float MaxFallSpeed = 7.0f;

    public float JumpHangGravityMult = 0.5f;
    public float JumpHangTimeThreshold = 0.1f;
    private CapsuleCollider2D _capsuleCollider;
    private int _groundMask;

    public bool IsJumping { get; private set; }


    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _capsuleCollider = GetComponent<CapsuleCollider2D>();
        _groundMask = LayerMask.GetMask("Ground");
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        // For debugging
        _movement = _rb.velocity;
        JumpCheck();

        Run();
        FlipSprite();
        CalculateGravity();
    }

    void Run() 
    {
        Vector2 playerVelocity = new Vector2 (_moveInput.x*MoveSpeed, _rb.velocity.y);
        _rb.velocity = playerVelocity;

        bool playerHasHorizontalSpeed = Mathf.Abs(_rb.velocity.x) > Mathf.Epsilon;
        _animator.SetBool("isRunning", playerHasHorizontalSpeed);
    }

    void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(_rb.velocity.x) > Mathf.Epsilon;
        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(_rb.velocity.x), 1f);
        }
    }


    void Jump()
    {
        if (!IsJumping)
        {
            IsJumping = true;
            _rb.velocity = new Vector2(_rb.velocity.x, _jumpSpeed);
        }
    }


    void CalculateGravity() 
    {
        if (_rb.velocity.y < 0 && _moveInput.y < 0)
        {
            SetGravityScale(GravityScale*FastFallGravityMult);

			_rb.velocity = new Vector2(_rb.velocity.x, Mathf.Max(_rb.velocity.y, -MaxFastFallSpeed));
        }
        else if (_isJumpCut)
		{
			//Higher gravity if jump button released
			SetGravityScale(GravityScale * JumpCutGravityMult);
			_rb.velocity = new Vector2(_rb.velocity.x, Mathf.Max(_rb.velocity.y, -MaxFallSpeed));
		}
        else if (IsJumping && Mathf.Abs(_rb.velocity.y) < JumpHangTimeThreshold)
        {
            SetGravityScale(GravityScale * JumpHangGravityMult);
        }
        else if (_rb.velocity.y < 0)
        {
            SetGravityScale(GravityScale * FallGravityMult);
            _rb.velocity = new Vector2(_rb.velocity.x, Mathf.Max(_rb.velocity.y, -MaxFallSpeed));
        }
        else
        {
            SetGravityScale(GravityScale);
        }
    }

    void JumpCheck()
    {
        // Cast a ray straight down.
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, _capsuleCollider.bounds.extents.y + 0.2f, _groundMask);// If it hits something...
        if (hit.collider != null)
        {
            if (_rb.velocity.y <= 0f)
            {
                IsJumping = false;
                _isJumpCut = false;
            }
        }
        else 
        {
            IsJumping = true;
        }
    }


    #region HELPER METHODS
    public void SetGravityScale(float scale)
    {
        _rb.gravityScale = scale;
    }

    private bool CanJump()
    {
        return !IsJumping;
    }

    private bool CanJumpCut()
    {
		return IsJumping && _rb.velocity.y > 0;
    }
    #endregion

    #region RECEIVED MESAGGES
    
    void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            Jump();
        }
    }
    
    void OnJumpReleased(InputValue value)
    {
        if (CanJumpCut())
        {
            _isJumpCut = true;
        }
    }
    #endregion
}
