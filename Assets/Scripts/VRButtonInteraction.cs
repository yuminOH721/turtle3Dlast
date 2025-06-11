using UnityEngine;
using UnityEngine.UI; // UI 관련 네임스페이스 추가

public class VRButtonInteraction : MonoBehaviour
{
    private Button myButton;
    private Image targetImage;
    private RectTransform rectTransform;
    private Vector3 originalScale;

    public Color pressedColor = new Color(0.5f, 0.5f, 0.5f); // 눌렸을 때 색
    public Color normalColor = Color.white;

    private bool isPressed = false;

    private void Awake()
    {
        myButton = GetComponent<Button>();
        targetImage = myButton.targetGraphic as Image;

        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand") && !isPressed)
        {
            isPressed = true;

            // 눌림 효과
            rectTransform.localScale = originalScale * 0.95f;

            // 색상 변경
            if (targetImage != null)
                targetImage.color = myButton.colors.pressedColor;

            // 버튼 기능 실행
            myButton.onClick.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            isPressed = false;

            // 색상 복구
            if (targetImage != null)
                targetImage.color = myButton.colors.normalColor;

            // 스케일 복구
            rectTransform.localScale = originalScale;
        }
    }
}

/*[RequireComponent(typeof(Button))] // Button 컴포넌트가 꼭 붙어 있어야 함
public class VRButtonTrigger : MonoBehaviour
{
    private Button myButton;

    private void Awake()
    {
        myButton = GetComponent<Button>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hand"))
        {
            Debug.Log("✅ 손이 버튼에 닿음!");
            myButton.onClick.Invoke(); // 자기 자신의 OnClick 실행
        }
    }
}*/
