using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;
using SceneLoad.Managers;

public class ButtonManager : MonoBehaviour
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
        else
        {
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

        if (turtleManager != null)
            turtleManager.PrintError("Stop 버튼이 눌렸습니다");
        else if (TurtleManager.instance != null)
            TurtleManager.instance.PrintError("Stop 버튼이 눌렸습니다");
        else
            Debug.LogError("TurtleManager 인스턴스를 할당하세요.");
    }

    public void RunButtonClicked()
    {
        // if (lastClickedButton == "Run")
        // {
        //     Debug.Log("Run 버튼 연속 클릭 방지됨");
        //     return;
        // }

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
        "no.1\n사각형을 그리세요.\n\nTurtle() : 거북이 캐릭터를 만든다.\nrotateX(각도) : X축으로 회전한다.\nrotateY(각도) : Y축으로 회전한다.\nforward(거리) : 바라보는 방향으로 이동한다.\nforward(3) : 큐브 한 칸 앞으로라는 뜻!",
        "no.2\n사각형을 while 반복문을\n이용해서 구현하세요\n\nwhile 조건: : 조건이 참인 동안 반복한다.\r\n\r\nnum += 1 : 변수 num에 1을 더한다.",
        "no.3\n사각형을 for 반복문을\n이용해서 구현하세요\n\nTurtle() : 거북이를 만든다.\nrotateX(각도) : X축으로 회전한다.\nrotateY(각도) : Y축으로 회전한다.\nforward(거리) : 바라보는 방향으로 이동한다.\nfor i in range(n) : 코드를 n번 반복한다.",
        "no.4\n2D 2",
        "no.5\npython 3",
        "no.6\n2D 3",
        "no.7\n3D 1",
        "no.8\n3D 2"
    };

    private string[][] allHints = {
        new string[] { "\"거북이는 오른쪽으로\n방향을 틀고 앞으로 이동했어.\n그러고 나서는 하늘로\n몸을 돌려서 위로 올라가더니,\n세 번을 더 회전하며\n사각형을 완성했지!\"", "\ra = ________()\r\na.________(90)\r\na.forward(3)\r\na.________(270)\r\na.forward(3)\r\na.________(270)\r\na.forward(3)\r\na.rotateX(270)\r\na.________(3)" },
        new string[] { "\"거북이는 오른쪽으로 방향을 틀고,\r\n숫자가 4가 되기 전까지 같은 동작을 반복했어.\r\n앞으로 나아가고, 아래로 몸을 꺾었지.\r\n매번 숫자를 하나씩 늘리면서!\"", "a = Turtle()\r\na.rotateY(90)\r\n______ = 0\r\nwhile ______ < ____:\r\n    a.forward(3)\r\n    a.rotateX(-90)\r\n    ______ += 1" },
        new string[] { "\"거북이는 오른쪽으로\n방향을 틀고 이동했어.\r\n앞으로 나아가고 몸을 꺾기를 4회 진행했지!\r\n그렇게 사각형을 그리며 움직였어!\"", "\ra = Turtle()\r\na.rotateY(90)\r\nfor __ in ______(__):\r\n    a._______(_)\r\n    a.rotateX(270)" },
        new string[] { "hint 4-1", "hint 4-2" },
        new string[] { "hint 5-1", "hint 5-2" },
        new string[] { "hint 6-1", "hint 6-2" },
        new string[] { "hint 7-1", "hint 7-2" },
        new string[] { "hint 8-1", "hint 8-2" }
    };

    private string[] allAnswers = {
        "\ra = Turtle()\r\na.rotateY(90)\r\na.forward(3)\r\na.rotateX(270)\r\na.forward(3)\r\na.rotateX(270)\r\na.forward(3)\r\na.rotateX(270)\r\na.forward(3)",
        "\ra = Turtle()\r\na.rotateY(90)\r\nnum = 0\r\nwhile num < 4:\r\n\ta.forward(3)\r\n\ta.rotateX(-90)\r\n\tnum += 1",
        "\ra = Turtle()\r\na.rotateY(90)\r\nfor i in range(4):\r\n\ta.forward(3)\r\n\ta.rotateX(270)",
        "Answer4",
        "Answer5",
        "Answer6",
        "Answer7",
        "Answer8"
    };

    private string[] hints;  // 현재 문제의 힌트 배열
    private int currentHintIndex = 0;
    private int currentQuestionIndex = 0;
    private string originalProblem = "";

    public TextMeshProUGUI AnswerText; // 연결된 InputField
    public bool isAnswerButton = false;
    private bool isShowingAnswer = false;// 🔥 상태 토글 변수
    private int AnswerCount = 0;
    private string originalAnswerText = "";

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

    public void ToggleAnswer()
    {
        //isShowingAnswer = true;
        AnswerCount += 1;
        /*if (!isAnswerButton || AnswerText == null)
        {
            Debug.LogWarning("정답 버튼 동작 조건 불충분!");
            return;
        }*/


        if (!isShowingAnswer && AnswerCount == 1)
        {
            originalAnswerText = AnswerText.text;
            AnswerText.text = allAnswers[currentQuestionIndex];
            isShowingAnswer = true;
            Debug.Log("정답 표시!");
        }
        else if (AnswerCount == 2)
        {
            AnswerText.text = originalAnswerText;
            isShowingAnswer = false;
            AnswerCount = 0;
            Debug.Log("정답 복원!");
        }
    }

    //IPointerDownHandler, IPointerUpHandler

    /*public TMP_InputField AnswerText;
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
    }*/

    /*public void ShowAnswer()
    {
        if (AnswerText == null)
        {
            Debug.LogError("AnswerText가 연결되지 않았습니다!");
            return;
        }
        AnswerText.text = allAnswers[currentQuestionIndex];
    }*/

    //Error!!
    public void ErrorButton(string errorType)
    {
        if (lastClickedButton == "Error")
        {
            Debug.Log("Error 버튼 연속 클릭 방지됨");
            return;
        }

        lastClickedButton = "Error";

        Debug.Log("Error 버튼이 눌렸습니다.");
        turtleManager.OnErrorButtonClicked();
    }



    //Exit!!
    public void LoadFinishScene()
    {
        SceneManager.LoadScene("FinishScene");

        Debug.Log("Exit!"); //나가기
    }

}
