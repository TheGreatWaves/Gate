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
    private int _groundMask;

    public bool CanTeleport { set; get; }

    private void Start() 
    {
        spawnPoint = transform.GetChild(1);
        _groundMask = LayerMask.GetMask("Ground");
        CanTeleport = false;

        if (linkedPortal)
        {
            Connect(linkedPortal);
        }
    }

    public void Connect(Portal other)
    {
        linkedPortal = other;
        other.linkedPortal = this;

        CheckCanTeleport();
    }

    public void CheckCanTeleport()
    {
        var canTeleport = !(Mathf.Approximately(linkedPortal.gameObject.transform.position.z, -100f) || (Mathf.Approximately(transform.position.z, -100f)));
        linkedPortal.CanTeleport = canTeleport;
        CanTeleport = canTeleport;
    }

    public void CalculateDirection()
    {
        _linkedAngle = linkedPortal != null ? linkedPortal.transform.eulerAngles.z : 0;
        _portalAngle = transform.eulerAngles.z;
        _sameAngle = _linkedAngle == _portalAngle;
        _oppositeAngle = Mathf.Approximately(Mathf.Abs(_linkedAngle - _portalAngle), 180.0f);

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
            if (Mathf.Approximately(_portalAngle, 90f) || Mathf.Approximately(_portalAngle, 270f))
            {
                _direction = Direction.SAME_VERTICAL;
            }
            else if (Mathf.Approximately(_portalAngle, 0f) || Mathf.Approximately(_portalAngle, 180f))
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
        if (!CanTeleport || linkedPortal == null) return;

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
                    other.transform.Rotate(Vector3.forward, 180f);
                    isRotating = true;
                    StartCoroutine(RotatePlayerCoroutine(Quaternion.identity.eulerAngles, 0.5f, other.gameObject));
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

            if (_direction == Direction.OTHER)
            {
                // Rotate the player's transform based on the rotation difference between the two portals
                var eulerAngleDiff = transform.eulerAngles - linkedPortal.transform.eulerAngles;
                other.transform.Rotate(Vector3.forward, eulerAngleDiff.z);
                isRotating = true;
                StartCoroutine(RotatePlayerCoroutine(Quaternion.identity.eulerAngles, 0.5f, other.gameObject));
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

    public bool CheckPortalGrounded(Vector2 offset)
    {
        float rayDistance = 0.5f; // distance to cast the ray
        Vector2 topPosition = (Vector2)transform.GetChild(2).transform.position + offset;
        Vector2 bottomPosition = (Vector2)transform.GetChild(3).transform.position + offset;

        RaycastHit2D hit1 = Physics2D.Raycast(topPosition, topPosition + (Vector2)transform.TransformDirection(Vector3.left)*0.5f, rayDistance, _groundMask);
        RaycastHit2D hit2 = Physics2D.Raycast(bottomPosition, bottomPosition + (Vector2)transform.TransformDirection(Vector3.left)*0.5f, rayDistance, _groundMask);

        var pos1 = hit1.point;
        var pos2 = hit2.point;

        return !(((int)pos1.x == 0 && (int)pos1.y == 0) || ((int)pos2.x == 0 && (int)pos2.y == 0));
    }

    public bool NotInWall(Vector2 offset)
    {
        float rayDistance = 0.1f; // distance to cast the ray
        Vector2 topPosition = (Vector2)transform.GetChild(2).transform.position + offset;
        Vector2 bottomPosition = (Vector2)transform.GetChild(3).transform.position + offset;

        var vectorDirection = transform.TransformDirection(Vector3.right);

        RaycastHit2D hit1 = Physics2D.Raycast(topPosition + (Vector2)vectorDirection * 0.5f, vectorDirection, rayDistance, _groundMask);
        RaycastHit2D hit2 = Physics2D.Raycast(bottomPosition + (Vector2)vectorDirection * 0.5f, vectorDirection, rayDistance, _groundMask);

        var pos1 = hit1.point;
        var pos2 = hit2.point;

        return (((int)pos1.x == 0 && (int)pos1.y == 0) && ((int)pos2.x == 0 && (int)pos2.y == 0));
    }

    public bool ValidPlacement(Vector2 offset)
    {
        if (transform.CheckTransformDistance(linkedPortal.transform, 1.5f))
        {
            return false;
        }

        var notInWall = NotInWall(offset);
        var grounded = CheckPortalGrounded(offset);

        // Debug.Log("Grounded: " + grounded);
        // Debug.Log("Not In Wall: " + notInWall);

        return notInWall && grounded;
    }

}
