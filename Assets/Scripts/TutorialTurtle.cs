using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TutorialTurtle : MonoBehaviour
{
    [Header("거북이와 말풍선")]
    public GameObject turtle;             // 거북이 프리팹
    public GameObject speechBubble;       // 말풍선 UI (Canvas)
    public TextMeshProUGUI speechText;               // 말풍선 안의 텍스트
    public Vector3 offset = new Vector3(0, 2f, 0); // 말풍선 위치 오프셋

    [Header("설정")]
    public float speed = 2.5f;
    public AudioSource flySound;

    private int currentSpeechIndex = 0;
    private bool isTurtleClickable = false;

    private void Start()
    {
        turtle.SetActive(false);  // 처음엔 비활성화
        StartCoroutine(StartTutorial());
    }

    private void Update()
    {
        // 말풍선이 항상 사용자(카메라)를 보게
        if (speechBubble.activeSelf)
        {
            speechBubble.transform.LookAt(Camera.main.transform);
        }

        // 말풍선 위치 업데이트
        if (turtle.activeSelf)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(turtle.transform.position + offset);
            speechBubble.transform.position = screenPos;
        }
    }

    IEnumerator StartTutorial()
    {
        yield return new WaitForSeconds(3f);
        turtle.SetActive(true);
        ShowSpeech(speechList[0]);
        isTurtleClickable = true;
    }

    public void OnTurtleClicked()
    {
        if (!isTurtleClickable) return;

        currentSpeechIndex++;
        if (currentSpeechIndex >= speechList.Count) return;

        ShowSpeech(speechList[currentSpeechIndex]);
    }

    public IEnumerator MoveToTarget(Transform target, int speechStartIndex, int speechCount)
    {
        isTurtleClickable = false;
        flySound?.Play();

        while (Vector3.Distance(turtle.transform.position, target.position) > 0.1f)
        {
            turtle.transform.position = Vector3.MoveTowards(turtle.transform.position, target.position, speed * Time.deltaTime);
            yield return null;
        }

        flySound?.Stop();

        for (int i = 0; i < speechCount; i++)
        {
            ShowSpeech(speechList[speechStartIndex + i]);
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        }

        isTurtleClickable = true;
    }

    void ShowSpeech(string message)
    {
        speechBubble.SetActive(true);
        speechText.text = message;
    }

    List<string> speechList = new List<string>()
    {
        // ✅ 사용자 앞
        "안녕! 나는 너의 코딩 친구, 거북이야!",
        "앞으로 이 공간에서 파이썬을 쉽고 재밌게 배울 수 있어!",
        "내가 천천히 이곳을 소개해줄게. 나를 터치하면 다음으로 넘어가!",

        // ✅ window1 (3~6)
        "여기는 너가 풀 mission을 보여주는 스크린이야.",
        "Help버튼을 누르면 힌트도 살짝살짝 보여주는 스크린이지!",
        "너가 작성한 파이썬 코드가 여기에서 3D로 실행되는 걸 볼 수 있어!",
        "거북이가 그림을 그리거나, 캐릭터가 움직이기도 해!",

        // ✅ window2 (7~9)
        "이번엔 코드를 입력하고 결과값까지 볼 수 있는 구역이야.",
        "위쪽은 입력창, 아래쪽은 터미널 화면이야.",
        "제시된 문제를 위쪽 스크린에 입력하면 print문 결과나 에러 메시지는 이 터미널에서 볼 수 있어.",

        // ✅ 키보드 (10~12)
        "앞에 보이는 키보드를 사용하면 입력창에 바로 입력 가능!",
        "한 번 해보자! ",
        "print(\"Hello World\") 를 입력해봐!",

        // ✅ 큐브 (13~17)
        "너가 입력한 결과가 출력창뿐만 아니라 큐브에서도 확인할 수 있어!",
        "한 번 해보자!",
        "a = Turtle()",
        "a.forward(3)",
        "를 입력해봐!!",

        "큐브와 출력창에 동시에 출력되는 코드를 작성해볼까?",
        "t = Turtle()",
        "a = 100",
        "t.forward(a) //거북이 전진",
        "print(a) //100출력",

        // ✅ 버튼존 (22~32)
        "이쪽은 버튼 존이야. 하나씩 알려줄게!",
        "Run: 코드를 실행해!",
        "Stop: 실행 중인 걸 멈춰.",
        "Error: 에러가 있는지 체크해줘! 버튼이 빨간색으로 바뀌면 디버깅 오류가 있다는 뜻!",
        "Help: 힌트를 보여줄게.",
        "총 2개까지 보여줘. 다음 힌트를 보려면 한 번 더 눌러. 또 한 번 더 누르면 mission이 다시 떠~",
        "Answer: 정답 코드를 확인할 수 있어.",
        "한 번 누르면 입력창에 답이 떠! 다시 누르면 원래 입력 모드로 변경!!",
        "Reset: 코드를 초기 상태로 되돌려.",
        "Next: 다음 문제로 넘어갈 때 사용해.",
        "Exit: 학습을 종료해!",

        // ✅ 복귀 (33~36)
        "이제 이 공간이 어떤 곳인지 알겠지?",
        "이제 넌 첫 문제를 풀 준비가 됐어!",
        "화이팅! 천천히, 하지만 멋지게 배워보자",
        "'Next' 버튼을 누르면 바로 시작할 수 있어!",
    };
}
