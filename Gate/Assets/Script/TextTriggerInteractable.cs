using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextTriggerInteractable : MonoBehaviour
{
    public TMP_Text textMesh;
    public string itemName;
    private bool _inside;

    void Start()
    {
        // disable text mesh by default
        textMesh = transform.GetChild(0).GetComponent<TMP_Text>();
        textMesh.enabled = false;
        textMesh.text = "Press E to pickup " + itemName + ".";
        _inside = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            textMesh.enabled = true;
            _inside = true;
        }
    }

    private void Update() 
    {
        if (_inside)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                textMesh.text = "...";
                StartCoroutine(SelfDestruct(2.0f));
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _inside = false;
            textMesh.enabled = false;
        }
    }

    private IEnumerator SelfDestruct(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        gameObject.SetActive(false);
    }
}
