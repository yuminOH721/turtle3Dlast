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
        "no.1\n직선을 그리세요.\n\nTurtle() : 거북이 캐릭터를 만든다.\nrotateY(각도) : Y축으로 회전한다.\nforward(거리) : 바라보는 방향으로 이동한다.\nforward(3) : 큐브 한 칸 앞으로라는 뜻!\",",
        "no.2\n\'ㄱ\'을 그리세요.\n\nTurtle() : 거북이 캐릭터를 만든다.\nrotateX(각도) : X축으로 회전한다.\nrotateY(각도) : Y축으로 회전한다.\nforward(거리) : 바라보는 방향으로 이동한다.",
        "no.3\n사각형을 그리세요.\n\nTurtle() : 거북이 캐릭터를 만든다.\nrotateX(각도) : X축으로 회전한다.\nrotateY(각도) : Y축으로 회전한다.\nforward(거리) : 바라보는 방향으로 이동한다.",
        "no.4\n사각형을 while 반복문을\n이용해서 구현하세요\n\nwhile 조건: : 조건이 참인 동안 반복한다.\r\nnum += 1 : 변수 num에 1을 더한다.",
        "no.5\n사각형을 for 반복문을\n이용해서 구현하세요\n\nTurtle() : 거북이를 만든다.\nrotateX(각도) : X축으로 회전한다.\nrotateY(각도) : Y축으로 회전한다.\nforward(거리) : 바라보는 방향으로 이동한다.\nfor i in range(n) : 코드를 n번 반복한다.",
        "no.6\n빨간색 사각형을\nfor 반복문을\n이용해서 그리세요.\n\npencolor(색) : 펜 색상 설정\r\n\r\nfor i in range(4)\n: i가 0부터 3까지 \n총 4번 반복되도록 하는 반복문",
        "no.7\nif문을 활용해서\n빨간색 삼각형을\n그리세요. \n\npencolor(색) : 펜 색상 설정\nif (a == b):\r\n이건 \"a와 b가 같으면\" 이라는 뜻",
        "no.8\nelif와 else문을\n활용한 오각형을\n그리세요\n\n if a == 3:\r\n    # a가 3이면 실행\r\nelif a == 4:\r\n    # a가 4면 실행\r\nelse:\r\n    # 둘 다 아니면 실행"
    };

    private string[][] allHints = {
        new string[] { "\"거북이는 오른쪽으로\n방향을 틀고 앞으로\n쭉 나아갔어요.\"", "a = ______()\r\na.________(90)\r\na.forward(3)" },
        new string[] { "\"오른쪽으로 이동한 거북이가\n아래로 꺾여 \n'ㄱ' 모양을 만들었어\"", "a = ______()\r\na.________(90)\r\na.forward(3)\r\na.________(270)\r\na.forward(3)" },
        new string[] { "\"거북이는 오른쪽으로\n방향을 틀고 앞으로 이동했어.\n그러고 나서는 하늘로\n몸을 돌려서 위로 올라가더니,\n세 번을 더 회전하며\n사각형을 완성했지!\"", "\ra = ________()\r\na.________(90)\r\na.forward(3)\r\na.________(270)\r\na.forward(3)\r\na.________(270)\r\na.forward(3)\r\na.rotateX(270)\r\na.________(3)" },
        new string[] { "\"거북이는 오른쪽으로 방향을 틀고,\r\n숫자가 4가 되기 전까지 같은 동작을 반복했어.\r\n앞으로 나아가고, 아래로 몸을 꺾었지.\r\n매번 숫자를 하나씩 늘리면서!\"", "a = Turtle()\r\na.rotateY(90)\r\n______ = 0\r\nwhile ______ < ____:\r\n    a.forward(3)\r\n    a.rotateX(-90)\r\n    ______ += 1" },
        new string[] { "\"거북이는 오른쪽으로\n방향을 틀고 이동했어.\r\n앞으로 나아가고 몸을 꺾기를 4회 진행했지!\r\n그렇게 사각형을 그리며 움직였어!\"", "\ra = Turtle()\r\na.rotateY(90)\r\nfor __ in ______(__):\r\n    a._______(_)\r\n    a.rotateX(270)" },
        new string[] { "\"거북이는 빨간 펜을 들고\n네 번 반복해서 전진하고\n몸을 돌려 사각형을\n완성했어.\"", "a = Turtle()\r\na.________(90)\r\na.pencolor(____)\r\n\r\nfor i in ______(___):\r\n    a.________(3)\r\n    a.rotateX(270)" },
        new string[] { "\"조건이 참일 때\n빨간 펜을 들고,\n세 번 앞으로 가며\n몸을 돌려\n삼각형을 그렸어.\"", "a = Turtle()\r\na.rotateY(90)\r\nisRed = true\r\nif (________ == ____):\r\n    a.________(____)\r\n\r\nfor i in range(__):\r\n    a.forward(3)\r\n    a.rotateX(______)" },
        new string[] { "\"거북이는 변의 수에 따라\n다른 각도로\n돌 수 있어요.\r\nif문 조건을 맞춰\n오각형을 그릴 수 있게각\n도를 72도로 바꿔줘.\"", "a = Turtle()\r\na.rotateY(90)\r\nsides = 0\r\n\r\nif (sides == __):\r\n  angle = 120\r\nelif (sides == __):\r\n  angle = 90\r\n______:\r\n\tsides = 1\r\n  angle = 72\r\n\r\nfor i in range(sides):\r\n  a.forward(1.5)\r\n  a.rotateX(angle)" }
    };

    private string[] allAnswers = {
        "\ra = Turtle()\r\na.rotateY(90)\r\na.forward(3)",
        "\ra = Turtle()\r\na.rotateY(90)\r\na.forward(3)\r\na.rotateX(90)\r\na.forward(3)",
        "\ra = Turtle()\r\na.rotateY(90)\r\na.forward(3)\r\na.rotateX(270)\r\na.forward(3)\r\na.rotateX(270)\r\na.forward(3)\r\na.rotateX(270)\r\na.forward(3)",
        "\ra = Turtle()\r\na.rotateY(90)\r\nnum = 0\r\nwhile num < 4:\r\n\ta.forward(3)\r\n\ta.rotateX(-90)\r\n\tnum += 1",
        "\ra = Turtle()\r\na.rotateY(90)\r\nfor i in range(4):\r\n\ta.forward(3)\r\n\ta.rotateX(270)",
        "\ra = Turtle()\r\na.rotateY(90)\r\na.pencolor(red)\r\n\r\nfor i in range(4):\r\n\ta.forward(3)\r\n\ta.rotateX(270)",
        "\ra = Turtle()\r\na.rotateY(90)\r\nisRed = true\r\nif (isRed == true):\r\n\ta.pencolor(red)\r\nfor i in range(3):\r\na.forward(3)\r\na.rotateX(240)",
        "\ra = Turtle()\r\na.rotateY(90)\r\nsides = 0\r\n\r\nif (sides == 3):\r\n\tangle = 120\r\nelif (sides == 4):\r\n\tangle = 90\r\nelse:\r\n\tsides = 1\r\n\tangle = 72\r\n\r\nfor i in range(sides):\r\n\ta.forward(1.5)\r\n\ta.rotateX(angle)"
    };

    public GameObject nextCanvas;           // 새로 만든 Canvas 오브젝트
    public UnityEngine.UI.Image nextImage;  // Canvas 안의 Image
    public Sprite[] nextSprites;            // 문제 번호마다 보여줄 Sprite 배열

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

        // 이미지 보여주기
        if (nextCanvas != null)
        {
            nextCanvas.SetActive(true); // 캔버스 보이게
        }

        if (nextImage != null && nextSprites != null && NB < nextSprites.Length)
        {
            nextImage.sprite = nextSprites[NB];
        }

        Debug.Log("Next"); //다음으로 이동

        if (nextImage != null && nextSprites != null && NB < nextSprites.Length)
        {
            Debug.Log($"[Image 교체] NB: {NB}, Sprite: {nextSprites[NB]?.name}");
            nextImage.sprite = nextSprites[NB];
        }
        else
        {
            Debug.LogWarning("이미지를 바꾸는 조건이 안 맞아요!");
        }

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

    public void TutorialStop()
    {
        lastClickedButton = "Stop";

        if (turtleManager != null && turtleManager.terminalText != null)
        {
            turtleManager.terminalText.text = "STOP 버튼입니다.\n진행 중인 거북이의 움직임을 멈춥니다.";
        }
        else
        {
            Debug.LogError("TurtleManager 또는 terminalText가 할당되지 않았습니다.");
        }
    }

    public void TutorialError()
    {
        lastClickedButton = "Error";

        if (turtleManager != null)
        {
            turtleManager.OnTutorialErrorButtonClicked(); 
        }
        else
        {
            Debug.LogError("TurtleManager가 할당되지 않았습니다.");
        }
    }





}
