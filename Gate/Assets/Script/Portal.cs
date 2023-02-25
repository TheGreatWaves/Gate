using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Direction 
{
    OPPOSITE_HORIZONTAL, 
    OPPOSITE_VERTICAL,
    SAME_HORIZONTAL,
    SAME_VERTICAL,
    OTHER
}

public class Portal : MonoBehaviour
{
    public Portal linkedPortal;
    public Transform spawnPoint;

    private bool isTeleporting;
    private float _linkedAngle;
    private float _portalAngle;
    private bool _sameAngle, _oppositeAngle;
    private Direction _direction;

    private void Start() 
    {
        spawnPoint = transform.GetChild(1);
        
    }

    private void Awake() 
    {
        _linkedAngle = linkedPortal.transform.eulerAngles.z;
        _portalAngle = transform.eulerAngles.z;
        _sameAngle = _linkedAngle == _portalAngle;
        _oppositeAngle = Mathf.Abs(_linkedAngle - _portalAngle) == 180;

        if (_sameAngle)
        {
            if (Mathf.Approximately(_portalAngle, 90) || Mathf.Approximately(_portalAngle, 270))
            {
                _direction = Direction.OPPOSITE_VERTICAL;
            }
            else if (Mathf.Approximately(_portalAngle, 0) || Mathf.Approximately(_portalAngle, 180))
            {
                _direction = Direction.OPPOSITE_HORIZONTAL;
            }
            else
            {
                _direction = Direction.OTHER;
            }
        }
        else if (_oppositeAngle)
        {
            if (Mathf.Approximately(_portalAngle, 90) || Mathf.Approximately(_portalAngle, 270))
            {
                _direction = Direction.SAME_VERTICAL;
            }
            else if (Mathf.Approximately(_portalAngle, 0) || Mathf.Approximately(_portalAngle, 180))
            {
                _direction = Direction.SAME_HORIZONTAL;
            }
            else
            {
                _direction = Direction.OTHER;
            }
        }
        else 
        {
            _direction = Direction.OTHER;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!isTeleporting && other.gameObject.CompareTag("Player"))
        {
            isTeleporting = true;
            linkedPortal.isTeleporting = true;

            Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
            Vector2 playerVelocity = other.relativeVelocity;

            switch (_direction)
            {
                case Direction.OPPOSITE_HORIZONTAL:
                    playerVelocity = new Vector2(-playerVelocity.x, 0f);
                    other.transform.localScale = other.transform.localScale.WithAxis(Axis.X, -other.transform.localScale.x);
                    break;

                case Direction.OPPOSITE_VERTICAL:
                    playerVelocity = new Vector2(0f, -playerVelocity.y);
                    break;

                case Direction.SAME_HORIZONTAL:
                    break;

                case Direction.SAME_VERTICAL:
                    break;

                default:
                    float angleOfRotation = linkedPortal.transform.eulerAngles.z - transform.eulerAngles.z;
                    playerVelocity = playerVelocity.RotateVector2(angleOfRotation);
                    playerVelocity = new Vector2(
                        (_linkedAngle == 90 || _linkedAngle == 270) ? 0f : playerVelocity.x,
                        (_linkedAngle ==  0 || _linkedAngle == 180) ? 0f : playerVelocity.y
                    );
                    break;
            }

            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            playerController.UseCarryVelocity = true;

            other.gameObject.transform.position = linkedPortal.spawnPoint.position;
            rb.velocity = playerVelocity;

            StartCoroutine(TeleportCooldown());
        }
    }

    private IEnumerator TeleportCooldown()
    {
        yield return new WaitForSeconds(0.2f);
        isTeleporting = false;
        linkedPortal.isTeleporting = false;
    }
}
