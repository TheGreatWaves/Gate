using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugger : MonoBehaviour
{

    public static Debugger instance;
    public GameObject DebugTexture;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void DebugAt(Vector2 position)
    {
        Instantiate(DebugTexture, position, Quaternion.identity);
    }
}
