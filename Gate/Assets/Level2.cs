using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level2 : MonoBehaviour
{
    public PlayerController pc;

    private void Start() 
    {
        pc.TogglePortal();
    }

    public void NextScene()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
