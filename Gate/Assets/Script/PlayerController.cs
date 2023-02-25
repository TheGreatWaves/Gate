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

    [SerializeField] private float _jumpSpeed = 8.0f;
    private bool _isJumpCut;
    public float GravityScale = 2.0f;
    public float FastFallGravityMult = 3.0f;
    public float FallGravityMult = 2.5f;
    public float JumpCutGravityMult = 2.5f;

    public float MaxFastFallSpeed = 18.0f;
    public float MaxFallSpeed = 14.0f;

    public float JumpHangGravityMult = 0.5f;
    public float JumpHangTimeThreshold = 0.1f;
    private CapsuleCollider2D _capsuleCollider;
    private int _groundMask;

    public bool IsJumping { get; private set; }

    [SerializeField] private Transform _gunPoint;
    [SerializeField] private GameObject _bulletTrail;

    // Flag for carry velocity
    public Vector2 CarryVelocity { get; set; } 
    public bool UseCarryVelocity;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _capsuleCollider = GetComponent<CapsuleCollider2D>();
        _groundMask = LayerMask.GetMask("Ground");
        UseCarryVelocity = false;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        // For debugging
        _movement = _rb.velocity;
        if (UseCarryVelocity)
        {
            CalculateGravity();
            return;
        }


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
            _animator.SetBool("isJumping", true);
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
            if (UseCarryVelocity)
            {
                _animator.SetBool("isRunning", false);

                // If we have movement on the x-axis for our carry 
                // velocity, we lessen the gravity for a further launch
                if (CarryVelocity.x > 0) 
                {
                    SetGravityScale(GravityScale * 0.5f);
                }
                else 
                {
                    SetGravityScale(GravityScale * FallGravityMult);
                }
            }
            else 
            {
                SetGravityScale(GravityScale * FallGravityMult);
            }
            
            _rb.velocity = new Vector2(_rb.velocity.x, Mathf.Max(_rb.velocity.y, -MaxFallSpeed));
        }
        else
        {
            SetGravityScale(GravityScale);
        }
    }


    // Grounded
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Portal"))
        {
            CarryVelocity = _rb.velocity;
        }
        else 
        {
            UseCarryVelocity = false;
            _animator.SetBool("isJumping", false);
            IsJumping = false;
            _isJumpCut = false;
        }

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _animator.SetBool("isJumping", true);
        IsJumping = true;
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
        UseCarryVelocity = false;
        _moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        UseCarryVelocity = false;
        if (value.isPressed)
        {
            Jump();
        }
    }
    
    void OnJumpReleased(InputValue value)
    {
        UseCarryVelocity = false;
        if (CanJumpCut())
        {
            _isJumpCut = true;
        }
    }

    void OnFire(InputValue value)
    {
        // Calculate the target position in world coordinates
        Vector2 targetScreenPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 targetWorldPosition = targetScreenPosition - (Vector2)_gunPoint.position;

        // Calculate the direction vector for the ray
        Vector2 rayDirection = targetWorldPosition.normalized;

        // Extend the ray offscreen by scaling the direction vector
        rayDirection *= 1000f;

        // Cast the ray and get the hit information
        RaycastHit2D hit = Physics2D.Raycast(_gunPoint.position, rayDirection, Mathf.Infinity, _groundMask);

        // Instantiate the bullet trail
        GameObject trail = Instantiate(_bulletTrail, _gunPoint.position, Quaternion.identity);
        BulletTrail trailScript = trail.GetComponent<BulletTrail>();

        // Set the target position for the bullet trail
        if (hit.collider != null)
        {
            trailScript.SetTargetPosition(hit.point);
        }
        else
        {
            trailScript.SetTargetPosition((Vector2)_gunPoint.position + rayDirection);
        }
    }

    #endregion    
}
