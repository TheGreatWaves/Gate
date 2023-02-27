using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Level1 : MonoBehaviour
{
    public UnityEvent onTriggerAction;
    // Start is called before the first frame update
    void Start()
    {
        onTriggerAction.Invoke();
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene("Level 2");
    }
}
