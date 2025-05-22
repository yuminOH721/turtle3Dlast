using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class StartSceneAnimator : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public GameObject photoImage;
    public GameObject playButton;


    private void Start()
    {
        titleText.gameObject.SetActive(false);
        photoImage.SetActive(false);
        playButton.SetActive(false);

        StartCoroutine(PlayIntroSequence());
    }

    IEnumerator PlayIntroSequence()
    {
        yield return new WaitForSeconds(3f); // VR 시작 후 3초 대기

        // 제목 등장
        titleText.gameObject.SetActive(true);
        titleText.transform.localScale = Vector3.zero;
        titleText.color = new Color(1, 1, 1, 0);

        Vector3 targetScale = Vector3.one * 2.5f;
        float t = 0f;
        while (t < 0.5f)
        {
            t += Time.deltaTime * 2f;
            titleText.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            titleText.color = new Color(1, 1, 1, t);
            yield return null;
        }

        yield return new WaitForSeconds(3f); // 잠깐 머무름

        // 글씨 회전하며 사라짐
        float fadeTime = 1.5f;
        t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
            titleText.color = new Color(1, 1, 1, alpha);
            titleText.transform.Rotate(Vector3.up, 180f * Time.deltaTime);
            yield return null;
        }
        titleText.gameObject.SetActive(false);

        // 사진 등장 + 회전
        photoImage.SetActive(true);
        photoImage.transform.rotation = titleText.transform.rotation; // 같은 방향으로 회전 시작
        photoImage.AddComponent<Spin>();

        // 버튼도 함께 등장
        playButton.SetActive(true);

        // 부드러운 등장 연출 (옵션)
        StartCoroutine(FadeInUI(playButton));
    }

    IEnumerator FadeInUI(GameObject uiObject)
    {
        CanvasGroup cg = uiObject.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = uiObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
    }


}
