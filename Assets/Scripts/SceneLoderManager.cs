using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoderManager : MonoBehaviour
{
    public void LoadGateScene()
    {
        SceneManager.LoadScene("GateScene");
    }

    public void LoadMainScene()
    {
        SceneManager.LoadScene("SampleScene");
    }

}

