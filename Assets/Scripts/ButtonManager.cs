using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
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


    //NextZone!!
    public void GoToNextZone()
    {
        Debug.Log("Next Zone!"); //다음 존으로 이동
    }


    //Help!!!
    public TextMeshProUGUI HelpText;
    public string originalProblem = "Draw a triangle!";

    private string[] hints = {
        "How many lines make a triangle?",
        "Each line should connect to the next — think angles!",
        "Use forward() and left() three times, with turns adding up to 180°."
    };

    private int currentHintIndex = 0;

    public void ShowNextHint()
    {
        StopAllCoroutines(); // 중복 호출 방지
        StartCoroutine(ShowHintThenRestore());
    }

    private IEnumerator ShowHintThenRestore()
    {
        HelpText.text = hints[currentHintIndex];

        currentHintIndex = (currentHintIndex + 1) % hints.Length;

        yield return new WaitForSeconds(3f); // 3초 후
        HelpText.text = originalProblem;
    }

    //Answer!!
    public TextMeshProUGUI AnswerText;

    private string originalText;
    private string answerText = "turtle.forward(100)\nturtle.left(120)\nturtle.forward(100)\nturtle.left(120)\nturtle.forward(100)";

    public void OnPointerDown(PointerEventData eventData)
    {
        originalText = AnswerText.text; // 기존 텍스트 저장
        AnswerText.text = answerText;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        AnswerText.text = originalText; // 원래 텍스트 복원
    }

    public void ShowAnswer()
    {
        Debug.Log("Answer!");
    }

     
    //Error!!
    public void ErrorButton()
    {
        Debug.Log("Something Wrong!"); //오류 알려주기
    }


    //Exit!!
    public void LoadFinishScene()
    {
        SceneManager.LoadScene("FinishScene");
    
        Debug.Log("Exit!"); //나가기
    }

}
