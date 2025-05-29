using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_InputField))]
public class BluetoothKeyboardInput : MonoBehaviour, IUpdateSelectedHandler
{
    TMP_InputField inputField;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        // IME 조합 입력을 모두 허용
        inputField.onValidateInput += (s, i, c) => c;
    }

    public void OnUpdateSelected(BaseEventData eventData)
    {
        if (!inputField.isFocused) return;
        var kb = Keyboard.current;
        if (kb == null) return;

        // ─────── 탭 들여쓰기 ───────
        if (kb.tabKey.wasPressedThisFrame)
        {
            InsertAtCaret("\t");
            return;
        }

        // ─────── Ctrl + C 복사 ───────
        if ((kb.leftCtrlKey.isPressed || kb.rightCtrlKey.isPressed) && kb.cKey.wasPressedThisFrame)
        {
            CopySelectionToClipboard();
            return;
        }
        // ─────── Ctrl + V 붙여넣기 ───────
        if ((kb.leftCtrlKey.isPressed || kb.rightCtrlKey.isPressed) && kb.vKey.wasPressedThisFrame)
        {
            PasteFromClipboard();
            return;
        }
        // (나머지: Shift·Caps, 화살표·백스페이스·엔터 등은 내부 처리)
    }

    void CopySelectionToClipboard()
    {
        // 커서 위치와 선택 위치 구하기
        int pos = inputField.stringPosition;
        int sel = inputField.selectionAnchorPosition;
        int start = Mathf.Min(pos, sel);
        int end = Mathf.Max(pos, sel);
        int length = end - start;
        if (length > 0)
        {
            string selectedText = inputField.text.Substring(start, length);
            GUIUtility.systemCopyBuffer = selectedText;
        }
    }

    void PasteFromClipboard()
    {
        string clip = GUIUtility.systemCopyBuffer;
        if (!string.IsNullOrEmpty(clip))
            InsertAtCaret(clip);
    }

    void InsertAtCaret(string str)
    {
        int pos = inputField.stringPosition;
        string txt = inputField.text ?? "";
        txt = txt.Insert(pos, str);
        inputField.text = txt;
        // 커서 위치 업데이트
        inputField.stringPosition = pos + str.Length;
        inputField.caretPosition = inputField.stringPosition;
    }
}