using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class DynamicInputField : MonoBehaviour
{
    [Tooltip("Input Field가 최대 이 높이까지 늘어납니다.")]
    public float MaxHeight = 300f;

    TMP_InputField _input;
    LayoutElement _layout;

    void Awake()
    {
        _input = GetComponent<TMP_InputField>();

        // LayoutElement 자동 추가
        _layout = GetComponent<LayoutElement>();
        if (_layout == null)
            _layout = gameObject.AddComponent<LayoutElement>();

        // 텍스트가 바뀔 때마다 호출
        _input.onValueChanged.AddListener(OnTextChanged);
        OnTextChanged(_input.text);
    }

    void OnTextChanged(string text)
    {
        // textComponent가 요구하는 높이
        float preferred = _input.textComponent.preferredHeight;
        // 패딩(위 + 아래) 여유분 20 더하기
        float target = preferred + 20f;
        // 최대 높이 제한
        _layout.preferredHeight = Mathf.Min(target, MaxHeight);
    }
}
