using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] private TurtleManager turtleManager;

    public void StopButtonClicked()
    {
        Debug.Log("Stop!"); //일시정지
    }

    public void RunButtonClicked()
    {
        Debug.Log("Run!"); //����
        if (turtleManager != null)
        {
            turtleManager.ResetAllTurtles();
        }
        else if (TurtleManager.instance != null)
        {
            TurtleManager.instance.ResetAllTurtles();
        }
        else
        {
            Debug.LogError("TurtleManager 인스턴스를 할당 필요요");
            return;
        }

        if (TurtleManager.instance != null)
        {
            TurtleManager.instance.ExecuteCurrentCommand();
        }
        else
        {
            Debug.LogError("TurtleManager instance가 존재하지 않습니다.");
        }
    }

    public void ResetButtonClicked()
    {
        Debug.Log("Reset!"); // 초기 상태로 되돌리기

        if (turtleManager != null)
        {
            turtleManager.ResetAllTurtles();
        }
        else if (TurtleManager.instance != null)
        {
            TurtleManager.instance.ResetAllTurtles();
        }
        else
        {
            Debug.LogError("TurtleManager 인스턴스를 할당하거나, 싱글톤 인스턴스를 사용하세요.");
        }
    }

    public void GoToNextZone()
    {
        Debug.Log("Next Zone!"); //다음 존으로 이동
    }

    public void HelpButtonClicked()
    {
        Debug.Log("Let me help you"); //도움말
    }
    public void ShowAnswer()
    {
        Debug.Log("Answer!"); //소리크기조절
    }

    public void ErrorButton()
    {
        Debug.Log("Something Wrong!"); //오류 알려주기
    }

    public void ExitButtonClicked()
    {
        Debug.Log("Exit!"); //나가기
    }

}
