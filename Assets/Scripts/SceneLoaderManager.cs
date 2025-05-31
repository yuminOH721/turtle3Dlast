using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneLoad.Managers
{
    public class SceneLoaderManager : MonoBehaviour
    {
        // 선택한 번호를 저장하는 변수 (다른 씬에서도 접근 가능하게 static)
        public static int selectedIndex = 0;


        public void LoadGateScene()
        {
            SceneManager.LoadScene("GateScene");
        }

        public void LoadMainScene(int buttonNumber)
        {
            selectedIndex = buttonNumber - 1;  // 선택번호 - 1 로 인덱스 계산
            Debug.Log($"Selected index: {selectedIndex}");
            SceneManager.LoadScene("SampleScene");  // MainScene으로 이동
        }
    }
}

