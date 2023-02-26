using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public Transform ExitArea;
    private Direction _direction;
    private float _padAngle;
    public float speedLimit;


    private void Start() 
    {
        CalculateDirection();
        speedLimit = 30f;
    }

    public void CalculateDirection()
    {
        _padAngle = transform.eulerAngles.z;

        if (Mathf.Approximately(_padAngle, 0) || Mathf.Approximately(_padAngle, 180))
        {
            _direction = Direction.SAME_VERTICAL;
        }
        else if (Mathf.Approximately(_padAngle, 90) || Mathf.Approximately(_padAngle, 270))
        {
            _direction = Direction.SAME_HORIZONTAL;
        }
        else
        {
            _direction = Direction.OTHER;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
        Vector2 playerVelocity = other.relativeVelocity;

        switch (_direction)
        {
            case Direction.SAME_HORIZONTAL:
                playerVelocity = new Vector2(-playerVelocity.x, playerVelocity.y);
                other.transform.localScale = other.transform.localScale.WithAxis(Axis.X, -other.transform.localScale.x);
                break;

            case Direction.SAME_VERTICAL:
                playerVelocity = new Vector2(playerVelocity.x, -playerVelocity.y);
                break;

            case Direction.OTHER:
                playerVelocity = playerVelocity.RotateVector2(transform.rotation.eulerAngles.z);
                break;
        }

        PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
        playerController.UseCarryVelocity = true;
        var nextSpeed = playerVelocity * 1.2f;
        rb.velocity = nextSpeed.magnitude < speedLimit*speedLimit ? nextSpeed : rb.velocity;
    }

}
