using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TButtonManager : MonoBehaviour
{
    public TextMeshProUGUI errorText;
    public TextMeshProUGUI answerText;
    public TextMeshProUGUI helpText;

    private Coroutine errorCoroutine;
    private Coroutine answerCoroutine;
    private Coroutine helpCoroutine;

    public void OnErrorButtonClick()
    {
        if (errorCoroutine != null)
            StopCoroutine(errorCoroutine);

        errorCoroutine = StartCoroutine(ShowAndHideMessage(errorText, "코드의 오류를 알려드려요!"));
    }

    public void OnAnswerButtonClick()
    {
        if (answerCoroutine != null)
            StopCoroutine(answerCoroutine);

        answerCoroutine = StartCoroutine(ShowAndHideMessage(answerText, "문제의 답을 알려드려요!"));
    }

    public void OnHelpButtonClick()
    {
        if (helpCoroutine != null)
            StopCoroutine(helpCoroutine);

        helpCoroutine = StartCoroutine(ShowAndHideMessage(helpText, "문제를 풀 수 있는\n힌트 두 개 줄 예정!"));
    }

    private IEnumerator ShowAndHideMessage(TextMeshProUGUI targetText, string message)
    {
        targetText.text = message;
        targetText.gameObject.SetActive(true);

        yield return new WaitForSeconds(3f);

        targetText.gameObject.SetActive(false);
    }


}
