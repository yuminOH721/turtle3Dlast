using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CubeMaterialApplier : MonoBehaviour
{
    public Material cubeMaterial;

    void Start()
    {
        // 현재 씬에서 GameObject만 가져오기 (더 안전한 방식)
        GameObject[] allObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject rootObj in allObjects)
        {
            // 계층 구조를 포함해 모든 자식까지 검사
            foreach (Transform child in rootObj.GetComponentsInChildren<Transform>(true))
            {
                if (child.name.ToLower().Contains("cube") && child.GetComponent<Renderer>())
                {
                    child.GetComponent<Renderer>().material = cubeMaterial;
                }
            }
        }
    }
}

