using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;
using SceneLoad.Managers;

public class ButtonManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    private int NB;

    private string lastClickedButton = "";

    [SerializeField] private TurtleManager turtleManager;

    private void Start()
    {
        NB = SceneLoaderManager.selectedIndex;


        //if (!isHelpButton) return;
        if (NB == -1)
        {
            print("튜토리얼)");
        }
        else { 
            SetQuestion(NB);  // 문제 설정
        }
    
    }


    public void StopButtonClicked()
    {
        if (lastClickedButton == "Stop")
        {
            Debug.Log("Stop 버튼 연속 클릭 방지됨");
            return;
        }

        lastClickedButton = "Stop";

        Debug.Log("Stop!"); //일시정지
    }

    public void RunButtonClicked()
    {
        if (lastClickedButton == "Run")
        {
            Debug.Log("Run 버튼 연속 클릭 방지됨");
            return;
        }

        lastClickedButton = "Run";

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
        if (lastClickedButton == "Reset")
        {
            Debug.Log("Reset 버튼 연속 클릭 방지됨");
            return;
        }

        lastClickedButton = "Reset";

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




    //Help!!! + NextZone!!
    public TextMeshProUGUI HelpText;
    public bool isHelpButton = false;

    private string[] problems = {
        "1. python 1",
        "2. 2D 1",
        "3. python 2",
        "4. 2D 2",
        "5. python 3",
        "6. 2D 3",
        "7. 3D 1",
        "8. 3D 2"
    };

    private string[][] allHints = {
        new string[] { "hint 1-1", "hint 1-2" },
        new string[] { "hint 2-1", "hint 2-2" },
        new string[] { "hint 3-1", "hint 3-2" },
        new string[] { "hint 4-1", "hint 4-2" },
        new string[] { "hint 5-1", "hint 5-2" },
        new string[] { "hint 6-1", "hint 6-2" },
        new string[] { "hint 7-1", "hint 7-2" },
        new string[] { "hint 8-1", "hint 8-2" }
    };


    private string[] hints;  // 현재 문제의 힌트 배열
    private int currentHintIndex = 0;
    private int currentQuestionIndex = 0;
    private string originalProblem = "";

    //NextZone!!

    public void GoToNextZone()
    {
        /*if (lastClickedButton == "Next")
        {
            Debug.Log("Next 버튼 연속 클릭 방지됨");
            return;
        }

        lastClickedButton = "Next";*/

        currentHintIndex = 0;
        NB += 1;
        if (NB >= 8) // Number가 문제 개수 넘어가면 0으로
        {
            NB = 0;
        }

        SetQuestion(NB); // index에 해당하는 문제 설정

        Debug.Log("Next"); //다음으로 이동
    }


    public void SetQuestion(int index)
    {
        if (index < 0 || index >= problems.Length)
        {
            Debug.LogError("잘못된 문제 번호");
            return;
        }

        currentQuestionIndex = index;
        currentHintIndex = 0;
        originalProblem = problems[index];
        hints = allHints[index];
        HelpText.text = originalProblem;
    }

    public void ShowNextHint()
    {
        Debug.Log("Hint!");
        //StopAllCoroutines(); // 중복 호출 방지
        StartCoroutine(ShowHintThenRestore());
    }

    private IEnumerator ShowHintThenRestore()
    {
        if (HelpText == null)
        {
            Debug.LogError("HelpText가 연결되지 않았습니다!");
            yield break;
        }

        if (hints == null || hints.Length == 0)
        {
            Debug.LogError("힌트가 설정되지 않았습니다!");
            yield break;
        }

        HelpText.text = hints[currentHintIndex];
        currentHintIndex = (currentHintIndex + 1) % hints.Length;

        yield return new WaitForSeconds(3f);
        HelpText.text = originalProblem;
    }

    //Answer!!
    public TMP_InputField AnswerText;
    public bool isAnswerButton = false;

    private string originalText;
    private string answerText = "";

    public void OnPointerDown(PointerEventData eventData)
    {
        if(!isAnswerButton) return;

        if (AnswerText == null)
        {
            Debug.LogError("AnswerText가 연결되지 않았습니다!");
            return;
        }

        originalText = AnswerText.text; // 기존 텍스트 저장
        AnswerText.text = answerText;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isAnswerButton) return;

        if (AnswerText == null)
        {
            Debug.LogError("AnswerText가 연결되지 않았습니다!");
            return;
        }

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
