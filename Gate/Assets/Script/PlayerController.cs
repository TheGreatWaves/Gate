using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public bool CanUsePortal { get; set; }

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
    private bool _isDead = false;

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
        if (_isDead || UseCarryVelocity)
        {
            CalculateGravity();

            if (_isDead && Mathf.Approximately(_rb.velocity.y, 0f))
            {
                _rb.bodyType = RigidbodyType2D.Static;
            }
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

    public void TogglePortal()
    {
        CanUsePortal = true;
    }

    public void TogglePortalOff()
    {
        CanUsePortal = false;
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
                SetGravityScale(GravityScale * FallGravityMult);
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
        if (other.CompareTag("Lazer"))
        {
            Die();
        }
        else if (other.CompareTag("IgnoreLazer") || other.CompareTag("Untagged"))
        {
            return;
        }
        else if (other.CompareTag("Portal") && Moving())
        {
            CarryVelocity = _rb.velocity;
            _isJumpCut = false;
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
        if (other.CompareTag("IgnoreLazer") || other.CompareTag("Untagged")) return;
        
        _animator.SetBool("isJumping", true);
        IsJumping = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("IgnoreLazer")) return;

        if (IsGrounded())
        {
            UseCarryVelocity = false;
            _animator.SetBool("isJumping", false);
            IsJumping = false;
            _isJumpCut = false;
        }
    }

    private void Shoot(PortalColour portalColour)
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
        var trailRenderer = trail.GetComponent<TrailRenderer>();

        switch (portalColour)
        {
            case PortalColour.Blue:
                trailRenderer.startColor = Color.blue;
                trailRenderer.endColor  = Color.cyan;
                break;
            case PortalColour.Orange:
                trailRenderer.startColor = new Color(1f, 0.5f, 0f); // orange start color
                trailRenderer.endColor = new Color(1f, 0.8f, 0.2f); // lighter orange end color
                break; 
            default:
                break;
        }

        BulletTrail trailScript = trail.GetComponent<BulletTrail>();

        // Set the target position for the bullet trail
        if (hit.collider != null)
        {
            Vector2 portalPosition = hit.point;
            var correction = Quaternion.Euler(0, 0, 90);

            Quaternion portalRotation = Quaternion.FromToRotation(Vector2.up, hit.normal) * correction;
            
            if (!hit.collider.transform.gameObject.CompareTag("IgnoreLazer"))
            {
                PortalGun.instance.ShootPortal(portalColour, portalPosition, portalRotation);
            }
            trailScript.SetTargetPosition(hit.point);
        }  
        else
        {
            trailScript.SetTargetPosition((Vector2)_gunPoint.position + rayDirection);
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

    bool Moving() 
    {
        return !(Mathf.Approximately(CarryVelocity.x, 0f) && Mathf.Approximately(CarryVelocity.y, 0f));
    }

    void Die() 
    {
        if (!_isDead)
        {
            _isDead = true;
            _animator.SetTrigger("died");
        }
    }

    bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.9f, LayerMask.GetMask("Ground"));
        return hit.collider != null;
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
        if (_isDead || !CanUsePortal) return;
        Shoot(PortalColour.Blue);
    }

    void OnAlternateFire(InputValue value)
    {
        if (_isDead || !CanUsePortal) return;
        Shoot(PortalColour.Orange);
    }
    #endregion    

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
