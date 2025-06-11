/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TutorialTurtle : MonoBehaviour
{
    [Header("버튼")]
    public GameObject okButton; // OK 버튼 GameObject

    [Header("거북이와 말풍선")]
    public GameObject turtle;
    public GameObject speechCanvas;
    public TextMeshProUGUI speechText;

    [Header("설정")]
    public float speed = 2.5f;
    public AudioSource flySound;

    [Header("설명 포인트")]
    public Transform FrontTarget;
    public Transform window1Target;
    public Transform window2Target;
    public Transform KeyBoardTarget;
    public Transform CubeTarget;
    public Transform window3Target;

    private int currentSpeechIndex = 0;
    [SerializeField] private bool isTurtleClickable = false;
    [SerializeField] private bool isMoving = false;

    private void Start()
    {
        turtle.SetActive(false);
        okButton.SetActive(false); // 처음에는 꺼두기
        StartCoroutine(InitTutorial());
    }

    IEnumerator InitTutorial()
    {
        yield return new WaitForSeconds(3f);
        turtle.SetActive(true);
        okButton.SetActive(true); // 거북이와 함께 등장!
        ShowSpeech(speechList[currentSpeechIndex]);
        isTurtleClickable = true;
    }

    private void Update()
    {
        if (!turtle.activeSelf) return;

        turtle.transform.LookAt(Camera.main.transform);
        speechCanvas.transform.LookAt(Camera.main.transform);
        speechCanvas.transform.Rotate(0, 180f, 0);
    }

    public void OnOkClicked()
    {
        if (!isTurtleClickable || isMoving) return;

        currentSpeechIndex++;
        if (currentSpeechIndex >= speechList.Count)
            return;

        switch (currentSpeechIndex)
        {
            case 3:
                StartCoroutine(MoveToTarget(window1Target, 3, 2));
                break;
            case 5:
                StartCoroutine(MoveToTarget(window2Target, 5, 3));
                break;
            case 8:
                StartCoroutine(MoveToTarget(KeyBoardTarget, 8, 3));
                break;
            case 11:
                StartCoroutine(MoveToTarget(CubeTarget, 11, 7));
                break;
            case 18:
                StartCoroutine(MoveToTarget(window3Target, 18, 11));
                break;
            case 29:
                StartCoroutine(MoveToTarget(FrontTarget, 29, 4));
                break;
            default:
                //currentSpeechIndex++;
                ShowSpeech(speechList[currentSpeechIndex]);
                break;
        }


    }

    public IEnumerator MoveToTarget(Transform target, int speechStartIndex, int speechCount)
    {
        isTurtleClickable = false;
        isMoving = true;
        flySound?.Play();

        while (Vector3.Distance(turtle.transform.position, target.position) > 0.1f)
        {
            turtle.transform.position = Vector3.MoveTowards(turtle.transform.position, target.position, speed * Time.deltaTime);
            yield return null;
        }

        flySound?.Stop();

        for (int i = 0; i < speechCount; i++)
        {
            currentSpeechIndex = speechStartIndex + i;
            //print(currentSpeechIndex);
            ShowSpeech(speechList[currentSpeechIndex]);
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
            yield return new WaitWhile(() => Input.GetMouseButton(0)); // 클릭 떼기까지 대기
        }

        isTurtleClickable = true;
        isMoving = false;
    }

    void ShowSpeech(string message)
    {
        speechCanvas.SetActive(true);
        speechText.text = message;
    }

    List<string> speechList = new List<string>()
    {
        // ✅ 사용자 앞
        "안녕! 나는 너의 코딩 친구, \n 거북이야!", //0
        "앞으로 이 공간에서 \n 파이썬을 쉽고 \n 재밌게 배울 수 있어!", //1
        "내가 천천히 \n 이곳을 소개해줄게.", //2

        // ✅ window1 (3~4) 
        "여기는 너가 풀 mission을 \n 보여주는 스크린이야.", //3
        "Help버튼을 누르면 \n 힌트도 살짝살짝 \n 보여주는 스크린이지!", //4

        // ✅ window2 (5~7)
        "이번엔 코드를 입력하고 \n 결과값까지 볼 수 있는 구역이야.", //5
        "위쪽은 입력창, \n 아래쪽은 터미널 화면이야.", //6
        "제시된 문제를 \n 위쪽 스크린에 입력하면 \n 출력 결과를 \n 이 터미널에서 볼 수 있어.", //7

        // ✅ 키보드 (8~10)
        "앞에 보이는 키보드를 사용하면 \n 입력창에 바로 입력 가능!", //8
        "한 번 해보자! ", //9
         "print(\"Hello World\") \n 를 입력해봐!", //10

        // ✅ 큐브 (11~17)
        "너가 입력한 결과가 \n  출력창뿐만 아니라 \n 큐브에서도 확인할 수 있어!", //11
        "너가 작성한 파이썬 코드가 \n 여기에서 3D로 실행되지!", //12
        "거북이가 그림을 그리거나, \n 캐릭터가 움직이기도 해!",//13
        "한 번 해보자!", //14
        "a = Turtle() \n a.forward(3) \n 를 입력해봐!!", //15

        "큐브와 출력창에 \n 동시에 출력되는 \n 코드를 작성해볼까?", //16
        "t = Turtle() \n a = 3 \n t.forward(a) \n print(a)", //17

        // ✅ window3(버튼존) (18~28)
        "이쪽은 버튼 존이야.\n 하나씩 알려줄게!", //18
        "Run: 코드를 실행해!", //19
        "Stop: 실행 중인 걸 멈춰.", //20
        "Error: 에러가 있는지 체크해줘! \n 버튼이 빨간색으로 바뀌면 \n 디버깅 오류가 있다는 뜻!", //21
        "Help: 힌트를 보여줄게.", //22
        "총 2개까지 보여줘. \n 다음 힌트를 보려면 한 번 더 눌러. \n 또 한 번 더 누르면 mission이 다시 떠~", //23
        "Answer: 정답 코드를 확인할 수 있어.", //24
        "한 번 누르면 입력창에 답이 떠! \n 다시 누르면 원래 입력 모드로 변경!!", //25
        "Reset: 코드를 초기 상태로 되돌려.", //26
        "Next: 다음 문제로 넘어갈 때 사용해.", //27
        "Exit: 학습을 종료해!", //28

        // ✅ 복귀 (29~32) - 사용자 앞
        "이제 이 공간이 어떤 곳인지 알겠지?", //29
        "이제 넌 첫 문제를 풀 준비가 됐어!", //30
        "화이팅! 천천히, 하지만 멋지게 배워보자", //31
        "'Next' 버튼을 누르면 바로 시작할 수 있어!", //32
    };
}
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialTurtle : MonoBehaviour
{
    [Header("버튼")]
    public GameObject okButton;

    [Header("거북이와 말풍선")]
    public GameObject turtle;
    public GameObject speechCanvas;
    public TextMeshProUGUI speechText;

    [Header("설정")]
    public float speed = 2.5f;
    public AudioSource flySound;

    [Header("설명 포인트")]
    public Transform FrontTarget;
    public Transform window1Target;
    public Transform window2Target;
    public Transform KeyBoardTarget;
    public Transform CubeTarget;
    public Transform window3Target;

    private int currentSpeechIndex = 0;
    [SerializeField] private bool isTurtleClickable = false;
    [SerializeField] private bool isMoving = false;

    // 이동 포인트 매핑: 대사 인덱스 -> (타겟 위치, 출력할 연속 대사 개수)
    private Dictionary<int, (Transform target, int count)> moveMap;

    private void Start()
    {
        // 매핑 초기화
        moveMap = new Dictionary<int, (Transform, int)>
        {
            { 3, (window1Target, 2) },
            { 5, (window2Target, 3) },
            { 8, (KeyBoardTarget, 3) },
            { 11, (CubeTarget, 7) },
            { 18, (window3Target, 11) },
            { 29, (FrontTarget, 4) }
        };

        turtle.SetActive(false);
        okButton.SetActive(false);
        StartCoroutine(InitTutorial());
    }

    IEnumerator InitTutorial()
    {
        yield return new WaitForSeconds(3f);
        turtle.SetActive(true);
        okButton.SetActive(true);
        ShowSpeech(speechList[currentSpeechIndex]);
        isTurtleClickable = true;
    }

    private void Update()
    {
        if (!turtle.activeSelf) return;

        turtle.transform.LookAt(Camera.main.transform);
        speechCanvas.transform.LookAt(Camera.main.transform);
        speechCanvas.transform.Rotate(0, 180f, 0);
    }

    public void OnOkClicked()
    {
        if (!isTurtleClickable || isMoving) return;

        // 클릭 즉시 잠금
        isTurtleClickable = false;

        currentSpeechIndex++;
        if (currentSpeechIndex >= speechList.Count) return;

        // 이동 매핑이 있으면 해당 코루틴 실행, 없으면 말풍선만 교체
        if (moveMap.TryGetValue(currentSpeechIndex, out var moveData))
        {
            StartCoroutine(MoveToTarget(moveData.target));
        }

        ShowSpeech(speechList[currentSpeechIndex]);
        StartCoroutine(EnableClickAfterDelay(2f));
    }

    // 오로지 이동만 수행하도록 간소화된 코루틴
    public IEnumerator MoveToTarget(Transform target)
    {
        isMoving = true;
        flySound?.Play();

        while (Vector3.Distance(turtle.transform.position, target.position) > 0.1f)
        {
            turtle.transform.position = Vector3.MoveTowards(
                turtle.transform.position,
                target.position,
                speed * Time.deltaTime
            );
            yield return null;
        }

        flySound?.Stop();
        isMoving = false;
    }


    void ShowSpeech(string message)
    {
        speechCanvas.SetActive(true);
        speechText.text = message;
    }

    IEnumerator EnableClickAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isTurtleClickable = true;
    }

    private List<string> speechList = new List<string>()
    {
        "안녕! 나는 너의 코딩 친구, \n 거북이야!",
        "앞으로 이 공간에서 \n 파이썬을 쉽고 \n 재밌게 배울 수 있어!",
        "내가 천천히 \n 이곳을 소개해줄게.",
        "여기는 너가 풀 mission을 \n 보여주는 스크린이야.",
        "Help버튼을 누르면 \n 힌트도 살짝살짝 \n 보여주는 스크린이지!",
        "이번엔 코드를 입력하고 \n 결과값까지 볼 수 있는 구역이야.",
        "위쪽은 입력창, \n 아래쪽은 터미널 화면이야.",
        "제시된 문제를 \n 위쪽 스크린에 입력하면 \n 출력 결과를 \n 이 터미널에서 볼 수 있어.",
        "앞에 보이는 키보드를 사용하면 \n 입력창에 바로 입력 가능!",
        "한 번 해보자! ",
        "print(\"Hello World\") \n 를 입력해봐!",
        "너가 입력한 결과가 \n  출력창뿐만 아니라 \n 큐브에서도 확인할 수 있어!",
        "너가 작성한 파이썬 코드가 \n 여기에서 3D로 실행되지!",
        "거북이가 그림을 그리거나, \n 캐릭터가 움직이기도 해!",
        "한 번 해보자!",
        "a = Turtle() \n a.forward(3) \n 를 입력해봐!!",
        "큐브와 출력창에 \n 동시에 출력되는 \n 코드를 작성해볼까?",
        "t = Turtle() \n a = 3 \n t.forward(a) \n print(a)",
        "이쪽은 버튼 존이야.\n 하나씩 알려줄게!",
        "Run: 코드를 실행해!",
        "Stop: 실행 중인 걸 멈춰.",
        "Error: 에러가 있는지 체크해줘! \n 버튼이 빨간색으로 바뀌면 \n 디버깅 오류가 있다는 뜻!",
        "Help: 힌트를 보여줄게.",
        "총 2개까지 보여줘. \n 다음 힌트를 보려면 한 번 더 눌러. \n 또 한 번 더 누르면 mission이 다시 떠~",
        "Answer: 정답 코드를 확인할 수 있어.",
        "한 번 누르면 입력창에 답이 떠! \n 다시 누르면 원래 입력 모드로 변경!!",
        "Reset: 코드를 초기 상태로 되돌려.",
        "Next: 다음 문제로 넘어갈 때 사용해.",
        "Exit: 학습을 종료해!",
        "이제 이 공간이 어떤 곳인지 알겠지?",
        "이제 넌 첫 문제를 풀 준비가 됐어!",
        "화이팅! 천천히, 하지만 멋지게 배워보자",
        "'Next' 버튼을 누르면 바로 시작할 수 있어!"
    };
}
