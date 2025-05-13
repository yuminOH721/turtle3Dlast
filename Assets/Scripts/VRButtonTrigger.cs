using UnityEngine;
using UnityEngine.UI; // UI 관련 네임스페이스 추가

[RequireComponent(typeof(Button))] // Button 컴포넌트가 꼭 붙어 있어야 함
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
}
