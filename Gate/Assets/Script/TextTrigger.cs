using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class TextTrigger : MonoBehaviour
{
    public TMP_Text textMesh;
    public bool _interactable;
    private bool _inside;
    public string altText;
    public UnityEvent onTriggerAction;

    void Start()
    {
        // disable text mesh by default
        textMesh = transform.GetChild(0).GetComponent<TMP_Text>();
        textMesh.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // check if the other collider is the player
        if (other.gameObject.CompareTag("Player"))
        {
            // enable text mesh when player enters the trigger area
            textMesh.enabled = true;
            _inside = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // check if the other collider is the player
        if (other.gameObject.CompareTag("Player"))
        {
            // disable text mesh when player exits the trigger area
            textMesh.enabled = false;
            _inside = false;
        }
    }

    private void Update() 
    {
        if (_inside && _interactable)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                textMesh.text = altText;
                onTriggerAction.Invoke();
            }
        }
    }
}
