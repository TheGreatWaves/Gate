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

    public void Connect(Portal other)
    {
        linkedPortal = other;
        other.linkedPortal = this;
    }

    public void CalculateDirection()
    {
        _linkedAngle = linkedPortal != null ? linkedPortal.transform.eulerAngles.z : 0;
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

    private void Awake() 
    {
        CalculateDirection();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (linkedPortal == null) return;

        if (!isTeleporting && other.gameObject.CompareTag("Player"))
        {
            isTeleporting = true;
            linkedPortal.isTeleporting = true;

            Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
            Vector2 playerVelocity = other.relativeVelocity;

            float angleOfRotation = linkedPortal.transform.eulerAngles.z - transform.eulerAngles.z;

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
                    playerVelocity = playerVelocity.RotateVector2(angleOfRotation);
                    playerVelocity = new Vector2(
                        (_linkedAngle == 90 || _linkedAngle == 270) ? 0f : playerVelocity.x,
                        (_linkedAngle ==  0 || _linkedAngle == 180) ? 0f : playerVelocity.y
                    );
                    break;
            }

            if (_direction != Direction.OPPOSITE_HORIZONTAL 
                && _direction != Direction.SAME_HORIZONTAL
                && _direction != Direction.SAME_VERTICAL
                )
            {
                // Rotate the player's transform based on the rotation difference between the two portals
                var rotationSnapshot = Quaternion.identity;
                var eulerAngleDiff = transform.eulerAngles - linkedPortal.transform.eulerAngles;
                other.transform.Rotate(Vector3.forward, eulerAngleDiff.z);
                isRotating = true;
                StartCoroutine(RotatePlayerCoroutine(rotationSnapshot.eulerAngles, 0.5f, other.gameObject));
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
        yield return new WaitForSeconds(0.1f);
        isTeleporting = false;
        linkedPortal.isTeleporting = false;
    }

    private bool isRotating;

    private IEnumerator RotatePlayerCoroutine(Vector3 targetRotation, float duration, GameObject player)
    {
        float timeElapsed = 0f;
        Quaternion startRotation = player.transform.rotation;
        Quaternion targetQuaternion = Quaternion.Euler(targetRotation);

        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            player.transform.rotation = Quaternion.Slerp(startRotation, targetQuaternion, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        player.transform.rotation = targetQuaternion;
        isRotating = false;
    }

    public bool CheckPortalGrounded(Vector2 offset = default(Vector2))
    {
        float rayDistance = 0.1f; // distance to cast the ray
        Vector2 topPosition = (Vector2)transform.GetChild(2).transform.position + offset;
        Vector2 bottomPosition = (Vector2)transform.GetChild(3).transform.position + offset;

        RaycastHit2D hit1 = Physics2D.Raycast(topPosition, transform.TransformDirection(Vector3.left), rayDistance, LayerMask.GetMask("Ground"));
        RaycastHit2D hit2 = Physics2D.Raycast(bottomPosition, transform.TransformDirection(Vector3.left), rayDistance, LayerMask.GetMask("Ground"));

        var pos1 = hit1.point;
        var pos2 = hit2.point;

        // Debug.Log("POSITION TOP" + topPosition);
        // Debug.Log("POSITION BOTTOM" + bottomPosition);

        return ((int)pos1.x == 0 && (int)pos1.y == 0) || ((int)pos2.x == 0 && (int)pos2.y == 0);
    }

}
