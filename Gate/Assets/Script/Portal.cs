using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Direction 
{
    LEFT,
    RIGHT,
    UP,
    DOWN
}

public class Portal : MonoBehaviour
{
    public Portal linkedPortal;
    public Transform spawnPoint;

    private bool isTeleporting;

    private void Start() 
    {
        spawnPoint = transform.GetChild(1);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!isTeleporting && other.gameObject.CompareTag("Player"))
        {
            isTeleporting = true;
            linkedPortal.isTeleporting = true;

            Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
            Vector2 playerVelocity = other.relativeVelocity;

            float angleOfRotation = linkedPortal.transform.eulerAngles.z - transform.eulerAngles.z;
            playerVelocity = playerVelocity.RotateVector2(angleOfRotation);

            other.gameObject.transform.position = linkedPortal.spawnPoint.position;
            rb.velocity = linkedPortal.transform.TransformDirection(playerVelocity);

            StartCoroutine(TeleportCooldown());
        }
    }

    private IEnumerator TeleportCooldown()
    {
        yield return new WaitForSeconds(0.3f);
        isTeleporting = false;
        linkedPortal.isTeleporting = false;
    }
}
